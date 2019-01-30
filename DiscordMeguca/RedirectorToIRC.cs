using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using meguca.IRC;

namespace meguca.DiscordMeguca {
  class RedirectorToIRC {
    List<RedirectToIRC> Redirects;

    public RedirectorToIRC(string settingsPath, DiscordClient discordClient, IRCClient ircClient) {

      if(File.Exists(settingsPath)) {
        var json = File.ReadAllText(settingsPath);
        Redirects = JsonConvert.DeserializeObject<List<RedirectToIRC>>(json);
      }
      else {
        Redirects = new List<RedirectToIRC>();
      }

      #region Assign Redirects
      foreach (var red in Redirects) {
        discordClient.Client.MessageReceived += async (message) => {
          if(message.Channel.Id == red.DiscordChannel) {
            if(red.Attachments) {
              foreach (var att in message.Attachments) {
                await ircClient.SendToChannelAsync(red.IRCChannel, $"{red.PrefixForMessage}{att.Url}");
              }
            }
            if(red.ChannelMessages) {
              await ircClient.SendToChannelAsync(red.IRCChannel, $"{red.PrefixForMessage}{message.Content}");
            }
          }
        };
      }
      #endregion
    }
  }
}
