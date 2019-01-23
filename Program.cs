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
      DiscordClient discordClient = new DiscordClient(discordSettings);
      discordClient.IRCClient = ircClient;
      ircClient.Connect();
      var ircRun = new Task(ircClient.Run);
      ircRun.Start();

      discordClient.Client.MessageReceived += async (msgArgs) => {
        if(msgArgs.Channel.Id == 0000/*channel id*/) {
          foreach(var attach in msgArgs.Attachments) {
            await ircClient.SendAsync($"PRIVMSG #onioniichan :{attach.Url}");
          }
        }
      };

      var discordRun = new Task( async () => await discordClient.Run());
      discordRun.Start();
      var tasks = new List<Task>();
      tasks.Add(ircRun);
      tasks.Add(discordRun);
      Task.WaitAll(tasks.ToArray());
    }

  }
}
