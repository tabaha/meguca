using System;
using meguca.IRC;
using meguca.DiscordMeguca;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace meguca {
  class Program {

    private DiscordSocketClient Client;
    private string _BotToken;
    static void Main(string[] args) {

      //Pixiv.Downloader downloader = new Pixiv.Downloader("pixiv.json");
      //downloader.GetWork(@"https://www.pixiv.net/member_illust.php?mode=medium&illust_id=72911254");
      //return;

      //return;

      IRCSettings ircSettings = IRCSettings.Load("irc.json");
      IRCClient ircClient = new IRCClient(ircSettings);
      DiscordSettings discordSettings = DiscordSettings.Load("discord.json");
      DiscordClient discordClient = new DiscordClient(discordSettings);
      discordClient.IRCClient = ircClient;
      ircClient.DiscordClient = discordClient;
      ircClient.Setup();
      ircClient.Connect();
      var ircRun = new Task(ircClient.Run);
      ircRun.Start();

      discordClient.Client.MessageReceived += async (msgArgs) => {
        if(msgArgs.Channel.Id == 337692280267997196 && msgArgs.Author.Id != discordClient.Client.CurrentUser.Id) {
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
