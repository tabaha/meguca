using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord;
using meguca.IRC;
using System.Linq;
using System.Threading.Tasks;

namespace meguca.Discord {
  class DiscordClient {

    private DiscordSettings Settings;

    public DiscordSocketClient Client;

    public IRCClient IRCClient;

    public DiscordClient(DiscordSettings settings) {
      Client = new DiscordSocketClient();
      Settings = settings;

      SetupBot();
    }

    private void SetupBot() {
      Client.MessageReceived += async (messageParams) => {
        var message = messageParams;
        if (message == null) return;
        Console.WriteLine("Discord::" + message.Content);
      };

      Client.MessageReceived += async (messageParams) => {
        var message = messageParams;
        if (!string.IsNullOrWhiteSpace(message.Content)) {
          if (message.Content.StartsWith("!pixiv")) {
            var res = await message.Channel.SendFileAsync(@"C:\pictures\wallpapers\72727155_p0 - 0117.jpg");
            if (res != null) {
              foreach (var image in res.Attachments.Select(attach => attach.Url)) {
                await IRCClient.SendAsync($"PRIVMSG #onioniichan :{image}");
              }
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

  }
}
