using System;
using meguca.IRC;
using meguca.Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.Collections.Generic;
using System.Linq;

namespace meguca {
  class Program {

    private DiscordSocketClient Client;
    private string _BotToken;
    static void Main(string[] args) {

      IRCSettings ircSettings = IRCSettings.Load("irc.json");
      IRCClient ircClient = new IRCClient(ircSettings);
      DiscordSettings discordSettings = DiscordSettings.Load("discord.json");
      ircClient.Connect();
      var ircRun = new Task(ircClient.Run);
      ircRun.Start();

      var discordRun = new Task(async () => {
        await DiscordStart(discordSettings, ircClient);
      });
      discordRun.Start();
      var tasks = new List<Task>();
      tasks.Add(ircRun);
      tasks.Add(discordRun);
      Task.WaitAll(tasks.ToArray());
    }


    public static async Task DiscordStart(DiscordSettings settings, IRCClient ircClient) {
      var client = new DiscordSocketClient();

      client.MessageReceived += async (messageParams) => {
        var message = messageParams;
        if (message == null) return;
        Console.WriteLine("Discord::" + message.Content);
      };

      client.MessageReceived += async (messageParams) => {
        var message = messageParams;
        if (!string.IsNullOrWhiteSpace(message.Content)) {
          if (message.Content.StartsWith("!pixiv")) {
            var res = await message.Channel.SendFileAsync(@"C:\pictures\wallpapers\72727155_p0 - 0117.jpg");
            if(res != null) {
              foreach(var image in res.Attachments.Select(attach => attach.Url)) {
                await ircClient.SendAsync($"PRIVMSG #onioniichan :{image}");
              }
            }
          }
        }
      };
      await client.LoginAsync(TokenType.Bot, settings.Token);
      await client.StartAsync();
      Console.WriteLine("Discord connected?");



      await Task.Delay(-1);
    }

  }
}
