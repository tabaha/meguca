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

namespace meguca.DiscordMeguca {
  class DiscordClient {

    private DiscordSettings Settings;

    public DiscordSocketClient Client;

    public IRCClient IRCClient;

    public PixivDownloader PixivDownloader;

    public List<ulong> PixivChannels => Settings.PixivChannels;

    public DiscordClient(DiscordSettings settings, PixivDownloader downloader) {
      Client = new DiscordSocketClient();
      Settings = settings;
      PixivDownloader = downloader;

      SetupBot();
    }

    private void SetupBot() {
      Client.MessageReceived += DisplayMessage;
      Client.MessageReceived += PixivCommand;
    }

    private async Task PixivCommand(SocketMessage msg) {
      if (!string.IsNullOrWhiteSpace(msg.Content)) {

        if (PixivChannels.Contains(msg.Channel.Id) && (msg.Content.StartsWith("<" + Pixiv.Utils.WorkPageURL_EN) || msg.Content.StartsWith("<" + Pixiv.Utils.WorkPageURL) || msg.Content.StartsWith("!pixiv"))) {
          Dictionary<string, MemoryStream> downloadedImages = new Dictionary<string, MemoryStream>();
          try {
            long id = Pixiv.Utils.GetID(msg.Content);
            var illust = PixivDownloader.GetIllustration(id);
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
        if (PixivChannels.Contains(msg.Channel.Id) && msg.Content.StartsWith("!ppixiv")) {
          try {
            long id = Pixiv.Utils.GetID(msg.Content);
            var illust = PixivDownloader.GetIllustration(id);
            string tags = illust.Tags.ToString();
            PixivDownloader.DownloadIllustration(illust, async (s, i, ms) => { await msg.Channel.SendFileAsync(ms, s, i == 0 ? $"Tags: {tags}" : null); ms.Dispose(); });
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
