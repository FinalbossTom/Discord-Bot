using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Bot.Commands
{
    abstract public class ContextCommand
    {
        virtual public MessageCommandBuilder MessageCommandBuilder { get; } = new MessageCommandBuilder();
        abstract public string Name { get; }
        abstract public string Description { get; }
        abstract public PermissionLevel NeedPermission { get; }

        protected ContextCommand()
        {
            MessageCommandBuilder.WithName(Name);
            MessageCommandBuilder.WithContextTypes(InteractionContextType.Guild);

        }

        virtual public int Run(SocketMessageCommand command)
        {
            return 1;
        }

        virtual public string GetErrorMessage(int ErrorCode)
        {
            return ErrorCode switch
            {
                1 => "Not Implemented",
                2 => "Invalid Syntax",
                3 => "Invalid Target",
                _ => "Fatal Error",
            };
        }

    }
}
