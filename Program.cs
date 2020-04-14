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
using meguca.Pixiv;

namespace meguca {
  class Program {

    static void Main(string[] args) {

      //Pixiv.Downloader downloader = new Pixiv.Downloader("pixiv.json");
      //downloader.GetWork(@"https://www.pixiv.net/member_illust.php?mode=medium&illust_id=72911254");
      //return;


      PixivDownloader downloader = new PixivDownloader("pixiv.json");
      IRCClient ircClient = JsonConvert.DeserializeObject<IRCClient>(File.ReadAllText("irc.json"));
      ircClient.PixivDownloader = downloader;

      DiscordSettings discordSettings = DiscordSettings.Load("discord.json");
      DiscordClient discordClient = new DiscordClient(discordSettings, downloader);
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
