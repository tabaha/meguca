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

        if (PixivChannels.TryGetValue(msg.Channel.Id, out var channelPixivSettings)) {
          if (msg.Content.StartsWith("<" + Pixiv.Utils.WorkPageURL_EN) || msg.Content.StartsWith("<" + Pixiv.Utils.WorkPageURL) || msg.Content.StartsWith("!pixiv")) {
            try {
              long id = Pixiv.Utils.GetWorkID(msg.Content);
              var illust = await PixivDownloader.GetIllustration(id);
              if ((illust.IsR18 && !channelPixivSettings.AllowR18) || (illust.IsR18G && !channelPixivSettings.AllowR18G)) {
                Console.WriteLine($"Channel does not allow {(illust.IsR18G ? "R18G" : "R18")} images");
                return;
              }

              if (illust.Tags.TagsCollection.Any(t => t.Translation?.EN.ToLower() == "loli") && !illust.IsSFW)
                return;

              IEnumerable<int> pagesToDownload = null;
              #region -p parameter (specify pages to download)
              int flagStart = msg.Content.IndexOf("-p ");
              int flagEnd = -1;
              if (flagStart != -1) {
                flagStart += 3;
                flagEnd = msg.Content.IndexOf("-", flagStart);
                if (flagEnd == -1)
                  flagEnd = msg.Content.Length;
                pagesToDownload = msg.Content.Substring(flagStart, flagEnd - flagStart).Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(p => int.TryParse(p.Trim(), out var pageNumber) && pageNumber >= 0 && pageNumber < illust.PageCount ? pageNumber : -1).Where(p => p > -1);
              }
              else
                pagesToDownload = Enumerable.Range(0, illust.PageCount);
              #endregion

              //Maybe consider getting the Page object and doing Page.User.Name instead of this
              //string submissionInfo = $"**Title:** {illust.IllustTitle ?? string.Empty}    //    **Artist:** {illust.UserName}    //    **Tags:** {illust.Tags.ToString()}";
              bool isFirstSent = true;
              foreach (var imageTask in PixivDownloader.DownloadIllistrationAsync(illust, pageNumbers: pagesToDownload, maxPages: channelPixivSettings.MaxPages, maxBytes: Uploader.MaxBytes).ToList()) {
                using (var image = await imageTask) {
                  string text = isFirstSent ? illust.ToString() : string.Empty;
                  if (isFirstSent && channelPixivSettings.MaxPages.HasValue && pagesToDownload.Count() > channelPixivSettings.MaxPages)
                    text += $" [Showing {channelPixivSettings.MaxPages} images out of {pagesToDownload.Count()}]";
                  if (!image.IsOriginal)
                    text += " (preview version)";
                  text = text.Trim();
                  isFirstSent = false;
                  Console.WriteLine($"Sending page {image.PageNumber}");
                  var response = await Uploader.SendImage(msg, image, string.IsNullOrEmpty(text) ? null : text);
                }
              }
            }
            catch (Exception ex) {
              Console.WriteLine(ex);
              Console.WriteLine("------------------------");
              Console.WriteLine(ex.StackTrace);
            }
            finally {

            }
          }
          else if ((msg.Content.StartsWith("<" + Pixiv.Utils.ArtistPageURL_EN) || msg.Content.StartsWith("<" + Pixiv.Utils.ArtistPageURL)) && channelPixivSettings.AllowR18) {
            try {
              long id = Pixiv.Utils.GetArtistID(msg.Content);
              var info = await PixivDownloader.GetArtistProfile(id);
              Func<IllustrationThumbnail, string> description = illust => illust.Alt + (illust.PageCount > 1 ? "   " + illust.PageCount + " pages" : string.Empty) + "   (<" + Utils.GetWorkUrl(illust.Id) + ">)";
              foreach (var imageTask in PixivDownloader.DownloadThumbnailsArtistProfileAsync(info, description, maxPages: 5)) {
                using (var image = await imageTask) {
                  var response = await Uploader.SendImage(msg, image, image.Descritpion);
                }
              }
            }
            catch (Exception ex) {
              Console.WriteLine(ex);
              Console.WriteLine("------------------------");
              Console.WriteLine(ex.StackTrace);
            }
            finally {

            }
          }
          else if (msg.Content.StartsWith(".pixiv search") && channelPixivSettings.AllowR18) {
            var tags = msg.Content.Replace(".pixiv search", string.Empty).Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (tags.Any()) {
              try {
                var results = await PixivDownloader.GetSearchPage(tags, 1);
                Func<IllustrationThumbnail, string> description = illust => illust.Alt + (illust.PageCount > 1 ? "   " + illust.PageCount + " pages" : string.Empty) + "   (<" + Utils.GetWorkUrl(illust.Id) + ">)";
                foreach (var imageTask in PixivDownloader.DownloadThumbnailsAsync(results.Results.Illustrations.Take(5), description)) {
                  using (var image = await imageTask) {
                    var response = await Uploader.SendImage(msg, image, image.Descritpion);
                  }
                }
              }
              catch (Exception ex) {
                Console.WriteLine(ex);
                Console.WriteLine("------------------------");
                Console.WriteLine(ex.StackTrace);
              }
              finally {

              }
            }
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
