using System;

namespace meguca.IRC
{
  class IRCEventArgs : EventArgs
  {
    public DateTime Time { get; set; }
    public string Line { get; set; }
    public string[] Tokens { get; set; }
  }
}