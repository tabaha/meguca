using System;
using System.Collections.Generic;
using System.Text;

namespace meguca.DiscordMeguca {
  class RedirectToIRC {
    public ulong DiscordChannel { get; private set; }
    public string IRCChannel { get; private set; }
    public bool Attachments { get; private set; }
    public bool ChannelMessages { get; private set; }
    public string Prefix { get; private set; }
    public string Filter { get; private set; } //not implemented

    public string PrefixForMessage {
      get {
        if (!string.IsNullOrWhiteSpace(Prefix))
          return $"[{Prefix}] ";
        else
          return string.Empty;
      }
    }
  }
}
