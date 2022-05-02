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
using meguca.Telegram;

namespace meguca {
  class Program {

    static void Main(string[] args) {

      PixivDownloader downloader = new PixivDownloader("pixiv.json");
      IRCClient ircClient = JsonConvert.DeserializeObject<IRCClient>(File.ReadAllText("irc.json"));
      ircClient.PixivDownloader = downloader;

      TelegramClient telegramClient = JsonConvert.DeserializeObject<TelegramClient>(File.ReadAllText("telegram.json"));
      telegramClient.PixivDownloader = downloader;
      telegramClient.Setup();

      DiscordClient discordClient = JsonConvert.DeserializeObject<DiscordClient>(File.ReadAllText("discord.json"));
      discordClient.PixivDownloader = downloader;
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

      var telegramRun = new Task(async () => await telegramClient.Run());
      telegramRun.Start();

      var tasks = new List<Task>();
      tasks.Add(ircRun);
      tasks.Add(discordRun);
      tasks.Add(telegramRun);
      Task.WaitAll(tasks.ToArray());

      try {
        telegramClient.CTS.Cancel();
      }
      catch (Exception ex) { }
    }

  }
}
