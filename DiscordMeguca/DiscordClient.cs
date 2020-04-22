using System;
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
using meguca.Pixiv.Model;
using meguca.DiscordMeguca.Uploader;

namespace meguca.DiscordMeguca {
  class DiscordClient {
    [JsonIgnore]
    public static int MaxUploadBytes = 8388119;

    public DiscordSettings Settings;

    [JsonIgnore()]
    public DiscordSocketClient Client;
    [JsonIgnore()]
    public IRCClient IRCClient;
    [JsonIgnore()]
    public PixivDownloader PixivDownloader;
    [JsonIgnore()]
    public Dictionary<ulong, PixivChannelSettings> PixivChannels => Settings.PixivChannels;
    public IUploader Uploader;
    public LocalUploader LocalUploader;

    public DiscordClient() {
      Settings = new DiscordSettings();
      Client = new DiscordSocketClient();
      Uploader = new DiscordUploader();
      LocalUploader = new LocalUploader();
      SetupBot();
    }

    private void SetupBot() {
      Client.MessageReceived += DisplayMessage;
      Client.MessageReceived += PixivCommand;
    }

    private async Task PixivCommand(SocketMessage msg) {
      if (!string.IsNullOrWhiteSpace(msg.Content)) {

        if (PixivChannels.TryGetValue(msg.Channel.Id, out var channelPixivSettings) && (msg.Content.StartsWith("<" + Pixiv.Utils.WorkPageURL_EN) || msg.Content.StartsWith("<" + Pixiv.Utils.WorkPageURL) || msg.Content.StartsWith("!pixiv"))) {
          try {
            long id = Pixiv.Utils.GetID(msg.Content);
            var illust = await PixivDownloader.GetIllustration(id);
            Console.WriteLine("Finished getting illust data");
            if((illust.IsR18 && !channelPixivSettings.AllowR18) || (illust.IsR18G && !channelPixivSettings.AllowR18G )) {
              Console.WriteLine($"Channel does not allow {(illust.IsR18G ? "R18G" : "R18")} images");
              return;
            }
            string tags = illust.Tags.ToString();
            foreach (var imageTask in PixivDownloader.DownLoadIllistrationAsync(illust, Uploader.MaxBytes).ToList()) {
              using (var image = await imageTask) {
                Console.WriteLine($"Got page {image.PageNumber}");
                string text = image.PageNumber == 0 ? $"Tags: {tags}" : string.Empty;
                if (!image.IsOriginal)
                  text += " (preview version)";
                Console.WriteLine($"Sending page {image.PageNumber}");
                var response = await Uploader.SendImage(msg, image, string.IsNullOrEmpty(text) ? null : text.Trim());
              }
            }
          }
          catch (Exception ex) {
            Console.WriteLine(ex);
            Console.WriteLine("------------------------");
            Console.WriteLine(ex.Message);
            Console.WriteLine("------------------------");
            Console.WriteLine(ex.StackTrace);
          }
          finally {

          }
        }
        else if (PixivChannels.TryGetValue(msg.Channel.Id, out var channelPixivSettingsPP) && msg.Content.StartsWith("!ppixiv")) {
          try {
            long id = Pixiv.Utils.GetID(msg.Content);
            var illust = await PixivDownloader.GetIllustration(id);
            if ((illust.IsR18 && !channelPixivSettings.AllowR18) || (illust.IsR18G && !channelPixivSettings.AllowR18G)) {
              Console.WriteLine($"Channel does not allow {(illust.IsR18G ? "R18G" : "R18")} images");
              return;
            }
            string tags = illust.Tags.ToString();
            foreach (var image in await PixivDownloader.DownLoadIllistrationAsyncAlternate(illust)) {
              try {
                string text = image.PageNumber == 0 ? $"Tags: {tags}" : string.Empty;
                if (!image.IsOriginal)
                  text += " (preview version)";
                var response = await Uploader.SendImage(msg, image, string.IsNullOrEmpty(text) ? null : text.Trim());
              }
              catch (Exception ex) {
                Console.WriteLine(ex.Message);
              }
              finally {
                image.Dispose();
              }
            }
          }
          catch (Exception ex) {
            Console.WriteLine(ex.Message);
          }
          finally {

          }
        }
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
