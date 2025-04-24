using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Bot.Commands.Message
{
    public class AttachReaction : ContextCommand
    {
        public override string Name { get; } = "Attach Reaction";
        public override string Description { get; } = "For use with Auto-Roles.";
        public override PermissionLevel NeedPermission { get; } = PermissionLevel.Admin;

        private Dictionary<ulong,ulong> UserMessageMemory = new Dictionary<ulong, ulong>();

        public AttachReaction() : base() { }

        public async void RunAsync(SocketMessageCommand command)
        {
            if (command == null) return;

            UserMessageMemory[command.User.Id] = command.Data.Message.Id;
        }
    }

}
