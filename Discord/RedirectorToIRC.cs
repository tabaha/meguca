using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using meguca.IRC;

namespace meguca.Discord {
  class RedirectorToIRC {
    List<RedirectToIRC> RedirectToIRC;

    public RedirectorToIRC(string settingsPath, DiscordClient discordClient, IRCClient ircClient) {

      if(File.Exists(settingsPath)) {
        var json = File.ReadAllText(settingsPath);
        RedirectToIRC = JsonConvert.DeserializeObject<List<RedirectToIRC>>(json);
      }
      else {
        RedirectToIRC = new List<RedirectToIRC>();
      }
    }
  }
}
