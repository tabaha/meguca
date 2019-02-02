using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord;
using meguca.IRC;
using System.Linq;
using System.Threading.Tasks;
using meguca.Pixiv;

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

          if(message.Channel.Id == 337692280267997196 && message.Content.StartsWith(Pixiv.Utils.WorkPageURL)) {
            try {
              long id = Pixiv.Utils.GetID(message.Content);
              foreach(var result in PixivDownloader.GetWork(message.Content)) {
                var response = await message.Channel.SendFileAsync(result.Value, result.Key);
                foreach (var attach in response.Attachments.Select(a => a.Url))
                  await IRCClient.SendToChannelAsync("#onioniichan", attach);
                result.Value.Dispose();
              }
            }
            catch {
              await message.Channel.SendMessageAsync("Error fetching the image");
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
