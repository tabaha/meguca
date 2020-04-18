﻿using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord;
using meguca.IRC;
using System.Linq;
using System.Threading.Tasks;
using meguca.Pixiv;
using System.IO;
using Newtonsoft.Json;

namespace meguca.DiscordMeguca {
  class DiscordClient {

    public DiscordSettings Settings;

    [JsonIgnore()]
    public DiscordSocketClient Client;
    [JsonIgnore()]
    public IRCClient IRCClient;
    [JsonIgnore()]
    public PixivDownloader PixivDownloader;
    [JsonIgnore()]
    public Dictionary<ulong, PixivChannelSettings> PixivChannels => Settings.PixivChannels;

    public DiscordClient() {
      Settings = new DiscordSettings();
      Client = new DiscordSocketClient();

      SetupBot();
    }

    private void SetupBot() {
      Client.MessageReceived += DisplayMessage;
      Client.MessageReceived += PixivCommand;
    }

    private async Task PixivCommand(SocketMessage msg) {
      if (!string.IsNullOrWhiteSpace(msg.Content)) {

        if (PixivChannels.TryGetValue(msg.Channel.Id, out var channelPixivSettings) && (msg.Content.StartsWith("<" + Pixiv.Utils.WorkPageURL_EN) || msg.Content.StartsWith("<" + Pixiv.Utils.WorkPageURL) || msg.Content.StartsWith("!pixiv"))) {
          Dictionary<string, MemoryStream> downloadedImages = new Dictionary<string, MemoryStream>();
          try {
            long id = Pixiv.Utils.GetID(msg.Content);
            var illust = PixivDownloader.GetIllustration(id);
            if((illust.IsR18 && !channelPixivSettings.AllowR18) || (illust.IsR18G && !channelPixivSettings.AllowR18G )) {
              Console.WriteLine($"Channel does not allow {(illust.IsR18G ? "R18G" : "R18")} images");
              return;
            }
            string tags = illust.Tags.ToString();
            downloadedImages = PixivDownloader.DownloadIllustration(illust);
            bool tagsSent = false;
            foreach (var result in downloadedImages) {
              var response = await msg.Channel.SendFileAsync(result.Value, result.Key, !tagsSent ? $"Tags: {tags}" : null);
              tagsSent = true;
            }
          }
          catch (Exception ex) {
            Console.WriteLine(ex.Message);
          }
          finally {
            foreach (var ms in downloadedImages.Values)
              ms.Dispose();
            downloadedImages.Clear();
          }
        }
        //else if (PixivChannels.TryGetValue(msg.Channel.Id, out var channelPixivSettingsP) && msg.Content.StartsWith("!ppixiv")) {
        //  try {
        //    long id = Pixiv.Utils.GetID(msg.Content);
        //    var illust = PixivDownloader.GetIllustration(id);
        //    string tags = illust.Tags.ToString();
        //    PixivDownloader.DownloadIllustration(illust, async (s, i, ms) => { await msg.Channel.SendFileAsync(ms, s, i == 0 ? $"Tags: {tags}" : null); ms.Dispose(); });
        //  }
        //  catch (Exception ex) {
        //    Console.WriteLine(ex.Message);
        //  }
        //  finally {

        //  }
        //}
      }
    }

    private Task DisplayMessage(SocketMessage msg) {
      if (msg != null)
        Console.WriteLine(MessageToString(msg));
      return Task.CompletedTask;
    }

    public async Task Run() {
      await Client.LoginAsync(TokenType.Bot, Settings.Token);
      await Client.StartAsync();
      Console.WriteLine("Discord connected?");

      await Task.Delay(-1);
    }

    private string MessageToString(SocketMessage message) {
      string attachments = (string.Join(" ; ", message.Attachments.Select(a => $"{a.Filename} ({a.Width}x{a.Height}) {a.Url}")));
      return $"[Discord] #{message.Channel.Name} {message.Author.Username}: {attachments}{message.Content}";
    }

  }
}