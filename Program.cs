


using Discord_Bot.Bot;

public class Program
{
    

    private static async Task Main()
    {
        Console.WriteLine("Starting Bot");
        DiscordClient Client = new DiscordClient();

        while(Client.isRunning) { } //replace with Event?
        Console.WriteLine("Shutting down...");
    }

}