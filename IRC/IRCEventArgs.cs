using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace meguca.IRC {
  public enum eMessayeType {
    Ping = 1,
    PrivMessage = 2,
    Notice = 3,
    Other = 1000,
  }
  public class IRCEventArgs : EventArgs {
    public DateTime TimeStamp { get; set; }
    public string Line { get; set; }
    public string[] Tokens { get; set; }
    public string Channel { get; private set; }// => Tokens.Length > 2 && Tokens[2].StartsWith('#') ? Tokens[2] : null;
    public eMessayeType MessageType { get; private set; }// => Tokens[0] == "PING" ? Tokens[0] : Tokens[1];
    public string Content { get; private set; } //=> MessageType == "PRIVMSG" ? string.Join(' ', Tokens, 3, Tokens.Length - 3) : null;
    public string Sender { get; private set; }
    public string Destination { get; private set; }
    public string SenderFullInfo { get; private set; }
    public bool IsChannelMessage => MessageType == eMessayeType.PrivMessage && !string.IsNullOrEmpty(Channel);
    public IEnumerable<string> ContentTokens => MessageType == eMessayeType.PrivMessage ? Tokens.Skip(3) : Enumerable.Empty<string>();

    public IRCEventArgs() { }
    public IRCEventArgs(DateTime dateTime, string line) : this() {
      TimeStamp = dateTime;
      Line = line;
      Tokens = line.Split(' ');
      Destination = Tokens.Length > 2 ? Tokens[2] : string.Empty;
      Channel = Destination.StartsWith('#') ? Destination : string.Empty;
      if (Tokens[0] == "PING")
        MessageType = eMessayeType.Ping;
      else {
        switch (Tokens[1]) {
          case "PRIVMSG":
            MessageType = eMessayeType.PrivMessage;
            break;
          case "NOTICE":
            MessageType = eMessayeType.Notice;
            break;
          default:
            MessageType = eMessayeType.Other;
            break;
        }
      }

      if(MessageType == eMessayeType.PrivMessage) {
        if (Tokens[3].StartsWith(':') && Tokens[3].Length > 1)
          Tokens[3] = Tokens[3].Substring(1);
        if (Tokens.Length > 4)
          Content = string.Join(' ', Tokens, 3, Tokens.Length - 3);
        else if (Tokens.Length == 4)
          Content = Tokens[3];
        else
          Content = string.Empty;
        SenderFullInfo = Tokens[0].StartsWith(':') ? Tokens[0].Substring(1) : Tokens[0];
        Sender = SenderFullInfo.Substring(0, SenderFullInfo.IndexOf('!'));
      }
      else {
        SenderFullInfo = Sender = string.Empty;
      }
    }
  }
}