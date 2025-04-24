using Discord;
using Discord.WebSocket;
using Discord_Bot.Bot.Commands;
using Discord_Bot.Bot.Special;
using Discord_Bot.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Discord_Bot.Bot
{


    public class DiscordClient
    {
        public bool isRunning = true;
        public DiscordSocketClient rawClient;
        private string currentStatus = string.Empty;

        private CommandHandler Commands = new CommandHandler();
        private HandleEasterEvent EasterEvent = new HandleEasterEvent();

        private static DateTime EventStartTime = new DateTime(2025, 04, 17, 17, 0, 0, DateTimeKind.Utc);
        private static DateTime EventEndTime   = new DateTime(2025, 04, 22, 23, 0, 0, DateTimeKind.Utc);
        private bool EasterEventIsActive = false;

        public DiscordClient()
        {
            rawClient = new DiscordSocketClient();
            //rawClient.Log += Log;

            Console.WriteLine("[Discord] Logging in...");
            rawClient.LoginAsync(TokenType.Bot, Tokens.Discord);
            rawClient.StartAsync();
            Console.WriteLine("[Discord] Fully Ready!");

            Observable
                .Interval(TimeSpan.FromMinutes(5))
                .Subscribe(x => PeriodicCheck());
            
            PeriodicCheck();
            rawClient.MessageReceived += HandleMessageReceived;
            //rawClient.SlashCommandExecuted += HandleSlashCommand;
            //rawClient.MessageCommandExecuted += HandleMessageCommand;
        }

        public void PeriodicCheck()
        {
            EasterEventIsActive = (DateTime.Now > EventStartTime && DateTime.Now < EventEndTime);
            if (EasterEventIsActive) SetStatus($"Hosting the Easter Event for {EasterEvent.Players.Count()} Players!");
            else SetStatus("Waiting for things to happen...");
        }

        public void Shutdown() { isRunning = false; }

        public void SetStatus(string status)
        {
            if (status.Equals(currentStatus)) return;
            currentStatus = status;
            rawClient.SetCustomStatusAsync(status);
        }

        private async Task HandleMessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            if (message.Channel.Name[0] == '@') //if first letter is @, we are in a DM channel
            {
                Console.WriteLine(message.Author.Username + ": " + message.Content);
                if (message.Author.Id == 150552916732805120 /*my id*/ && message.Content.StartsWith("DEBUG::Results"))
                {
                    String GoldenEgg = "", ColorfulEggs = "", RedEggs = "", BlueEggs = "", GreenEggs = "", FreshEggs = "";

                    EasterEvent.Players.Sort((p1, p2) => { return p2.Points - p1.Points; });
                    foreach (var player in EasterEvent.Players) {
                        Console.WriteLine($"{player.Points} - {player.Name}");
                        bool found = false;
                        if (player.Points >= 137 && !found) { GoldenEgg =     $"``{player.Name}`` - {player.Points} Points."; found = true; }
                        if (player.Points >= 136 && !found) { ColorfulEggs += $"``{player.Name}`` - {player.Points} Points.\n"; found = true; }
                        if (player.Points >= 102 && !found) { RedEggs +=      $"``{player.Name}`` - {player.Points} Points.\n"; found = true; }
                        if (player.Points >= 68 && !found) {  BlueEggs +=     $"``{player.Name}`` - {player.Points} Points.\n"; found = true; }
                        if (player.Points >= 34 && !found) {  GreenEggs +=    $"``{player.Name}`` - {player.Points} Points.\n"; found = true; }
                        if (!found) {                         FreshEggs +=    $"``{player.Name}`` - {player.Points} Points.\n"; }
                    }
                    
                    var embed = new EmbedBuilder
                    {
                        Title = "Easter Event Results",
                        Description = "These are the Results of all participants.\nWe will get in touch regarding your Rewards soon.",
                        Color = Color.Green,
                        Footer = new EmbedFooterBuilder { IconUrl = "https://cdn.discordapp.com/attachments/1363483342586253372/1363520963211169872/Niko_Main_-_Low_Poly.jpg?ex=6806555a&is=680503da&hm=b1e83b42d247997251fb00169aff1529e7d70a8e37a21f7f2fe0696f142abcc2&", Text = "\"Thank you for playing!\"" }
                    };

                    if (GoldenEgg.Length == 0) GoldenEgg = "[None]";
                    if (ColorfulEggs.Length == 0) ColorfulEggs = "[None]";
                    if (RedEggs.Length == 0) RedEggs = "[None]";
                    if (BlueEggs.Length == 0) BlueEggs = "[None]";
                    if (GreenEggs.Length == 0) GreenEggs = "[None]";
                    if (FreshEggs.Length == 0) FreshEggs = "[None]";

                    embed.AddField(new EmbedFieldBuilder { Name = "**Golden Egg - First Completion**", Value = GoldenEgg });
                    embed.AddField(new EmbedFieldBuilder { Name = "**Colorful Eggs - All Points**", Value = ColorfulEggs });
                    embed.AddField(new EmbedFieldBuilder { Name = "**Red Eggs - 75% of all Points**", Value = RedEggs });
                    embed.AddField(new EmbedFieldBuilder { Name = "**Blue Eggs - 50% of all Points**", Value = BlueEggs });
                    embed.AddField(new EmbedFieldBuilder { Name = "**Green Eggs - 25% of all Points**", Value = GreenEggs });
                    embed.AddField(new EmbedFieldBuilder { Name = "**Fresh Eggs - Participants**", Value = FreshEggs });

                    //await rawClient.GetGuild(1321827807143395358).GetTextChannel(1363483342586253372).SendMessageAsync(embed: embed.Build()); //Debug Guild
                    await rawClient.GetGuild(1110250059469172817).GetTextChannel(1110250060169621576).SendMessageAsync(text: rawClient.GetGuild(1110250059469172817).GetRole(1359146914062401577).Mention, embed: embed.Build()); // FC
                    
                    return;
                }


                String output;
                IDisposable disposable = message.Channel.EnterTypingState();

                if (EasterEventIsActive)
                {
                    output = EasterEvent.CheckLocation(message);
                    await message.Channel.SendMessageAsync(output);
                }
                else
                {
                    output = "Sorry, but currently there is no Event active!";
                    await message.Channel.SendMessageAsync(output);
                }
                disposable.Dispose();
                Console.WriteLine("Answer: " + output);
            }
        }

        private async Task HandleSlashCommand(SocketSlashCommand command)
        {
            //Commands.RunSlash(command.CommandName, command);
        }

        private async Task HandleMessageCommand(SocketMessageCommand arg)
        {
            //Commands.RunMessage(arg.CommandName, arg);
        }


        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
