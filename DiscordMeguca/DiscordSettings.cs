using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace meguca.DiscordMeguca {
  public class PixivChannelSettings {
    public ulong ChannelID { get; set; } //Redundant
    public string Description { get; set; }
    public bool AllowR18 { get; set; }
    public bool AllowR18G { get; set; }
  }

  class DiscordSettings {
    public string Token { get; set; }
    public Dictionary<ulong, PixivChannelSettings> PixivChannels { get; set; }

    public DiscordSettings() {
      PixivChannels = new Dictionary<ulong, PixivChannelSettings>();
    }

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
      settings.PixivChannels = Console.ReadLine().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(c => ulong.TryParse(c, out var cid) ? cid : 0).Where(id => id != 0).ToDictionary(id => id, id => new PixivChannelSettings());
      settings.Save(path);
      return settings;
    }

    public void Save(string path) {
      File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
  }
}