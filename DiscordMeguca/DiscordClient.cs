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

namespace meguca.DiscordMeguca {
  class DiscordClient {

    private DiscordSettings Settings;

    public DiscordSocketClient Client;

    public IRCClient IRCClient;

    public PixivDownloader PixivDownloader;

    public DiscordClient(DiscordSettings settings, PixivDownloader downloader) {
      Client = new DiscordSocketClient();
      Settings = settings;
      PixivDownloader = downloader;

      SetupBot();
    }

    private void SetupBot() {
      Client.MessageReceived += async (messageParams) => {
        var message = messageParams;
        if (message == null) return;
        Console.WriteLine(MessageToString(message));
      };

      Client.MessageReceived += async (messageParams) => {
        var message = messageParams;
        if (!string.IsNullOrWhiteSpace(message.Content)) {

          if((message.Channel.Id == 337692280267997196 || message.Channel.Id == 131902567457357824) && message.Content.StartsWith(Pixiv.Utils.WorkPageURL)) {
            Dictionary<string, MemoryStream> downloadedImages = new Dictionary<string, MemoryStream>();
            try {
              long id = Pixiv.Utils.GetID(message.Content);
              var illust = PixivDownloader.GetIllustration(id);
              downloadedImages = PixivDownloader.DownloadIllustration(illust);
              string tags = illust.Tags.ToString();
              foreach (var result in downloadedImages) {
                var response = await message.Channel.SendFileAsync(result.Value, result.Key, $"Tags: {tags}");
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
        }
      };
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
