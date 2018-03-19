using System;
using meguca.IRC;
using meguca.Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.Collections.Generic;

namespace meguca
{
  class Program
  {

    private DiscordSocketClient Client;
    private string _BotToken;
    static void Main(string[] args)
    {

      IRCSettings ircSettings = IRCSettings.Load("irc.json");
      IRCClient ircClient = new IRCClient(ircSettings);
      DiscordSettings discordSettings = DiscordSettings.Load("discord.json");
      ircClient.Connect();
      var ircRun = new Task(ircClient.Run);
      ircRun.Start();

      var discordRun = new Task(async () =>
      {
        await DiscordStart(discordSettings);
      });
      discordRun.Start();
      var tasks = new List<Task>();
      tasks.Add(ircRun);
      tasks.Add(discordRun);
      Task.WaitAll(tasks.ToArray());
    }


    public static async Task DiscordStart(DiscordSettings settings)
    {
      var client = new DiscordSocketClient();

      client.MessageReceived += async (messageParams) =>
      {
        var message = messageParams;
        if (message == null) return;
        Console.WriteLine("Discord::" + message.Content);
      };
      await client.LoginAsync(TokenType.Bot, settings.Token);
      await client.StartAsync();
      Console.WriteLine("Discord connected?");



      await Task.Delay(-1);
    }

  }
}
