using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Bot.Commands.Slash.Admin
{
    public class ReactionMessage : SlashCommand
    {
        override public SlashCommandBuilder SlashCommandBuilder { get; } = new SlashCommandBuilder();

        public override string Name { get; } = "ReactionMessage";

        public override string[] Aliases { get; } = ["MessageReaction"];

        public override string Description { get; } = "Create 'Reaction Messages' used for assigning Auto-Roles and such.";

        public override CommandCategory Category { get; } = CommandCategory.Admin;

        public override PermissionLevel NeedPermission { get; } = PermissionLevel.Admin;

        public ReactionMessage() : base()
        {
            SlashCommandBuilder.AddOption("create", ApplicationCommandOptionType.SubCommand, "Create a new Reaction Message");

        }

        public override int Run(SocketSlashCommand command)
        {

            return 1;
        }
    }
}
