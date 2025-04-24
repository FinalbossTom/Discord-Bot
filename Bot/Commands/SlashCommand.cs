using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Bot.Commands
{
    public enum CommandCategory
    {
        Unknown = 0,
        General = 1,
        Admin = 2,
        Account = 3,
        Event = 4,
        Fun = 5,
        Utility = 6,

    }

    public enum PermissionLevel
    {
        None = 0,
        User = 1,
        Role = 2,
        Moderator = 3,
        Admin = 4,
        Owner = 5,
        Debug = 99,
    }

    abstract public class SlashCommand
    {

        virtual public SlashCommandBuilder SlashCommandBuilder { get; } = new SlashCommandBuilder();
        abstract public string Name { get; }
        abstract public string[] Aliases { get; }
        abstract public string Description { get; }
        abstract public CommandCategory Category { get; }
        abstract public PermissionLevel NeedPermission { get; }
        virtual public bool NeedAccount { get; } = false;


        protected SlashCommand() { 
            SlashCommandBuilder.WithName(Name);
            SlashCommandBuilder.WithDescription(Description);
        }

        virtual public int Run(SocketSlashCommand command)
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
