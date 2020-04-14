using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace meguca.IRC
{
  class IRCSettings
  {
    public string Hostname { get; set; }
    public int Port { get; set; }
    public string Nickname { get; set; }
    public string NickservPassword { get; set; }
    public string Username { get; set; }
    public string Realname { get; set; }
    public List<string> AutoJoinChannels { get; set; }

    public IRCSettings()
    {
      AutoJoinChannels = new List<string>();
    }

    public static IRCSettings Load(string path)
    {
      if (File.Exists(path))
      {
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<IRCSettings>(json);
      }
      else
      {
        return New(path);
      }
    }

    public static IRCSettings New(string path)
    {
      var settings = new IRCSettings();
      Console.WriteLine("Hostname?");
      settings.Hostname = Console.ReadLine();
      Console.WriteLine("Port?");
      var port_str = Console.ReadLine();
      int port = 6667;
      int.TryParse(port_str, out port);
      settings.Port = port;
      Console.WriteLine("Nickname?");
      settings.Nickname = Console.ReadLine();
      Console.WriteLine("Nickserv password?");
      settings.NickservPassword = Console.ReadLine();
      Console.WriteLine("Username?");
      settings.Username = Console.ReadLine();
      Console.WriteLine("Realname?");
      settings.Realname = Console.ReadLine();
      Console.WriteLine("Channels? (comma separated)");
      var chans = Console.ReadLine();
      settings.AutoJoinChannels.AddRange(chans.Split(','));
      settings.Save(path);
      return settings;
    }

    public void Save(string path)
    {
      File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
  }
}