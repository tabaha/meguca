using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace meguca.Discord {
  class DiscordSettings {
    public string Token { get; set; }

    public static DiscordSettings Load(string path) {
      if (File.Exists(path)) {
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<DiscordSettings>(json);
      }
      else {
        return New(path);
      }
    }

    public static DiscordSettings New(string path) {
      var settings = new DiscordSettings();
      Console.WriteLine("Discord token?");
      settings.Token = Console.ReadLine();
      settings.Save(path);
      return settings;
    }

    public void Save(string path) {
      File.WriteAllText(path, JsonConvert.SerializeObject(this));
    }
  }
}