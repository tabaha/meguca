using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace meguca.IRC.Commands {
  public class Decide : IIRCCommand {
    public string Name => "Decide";
    public string Description => "";

    public Dictionary<string, List<string>> Triggers { get; set ; }
    private RandomNumberGenerator RNG;
    public Decide() {
      Triggers = new Dictionary<string, List<string>>();
      RNG = RandomNumberGenerator.Create();
    }

    public async void Process(object sender, IRCEventArgs args) {
      if(args.IsChannelMessage && Triggers.TryGetValue(args.Channel, out var triggers) && triggers.Any(t => args.Tokens[3] == t)) {
        var client = sender as IRCClient;
        //assume trigger is token[3]
        var contentTokens = args.Tokens.Skip(4);
        if(!contentTokens.Any()) {
          await client.SendToChannelAsync(args.Channel, $"{args.Sender}: There are no choices to choose from!");
          return;
        }
        var choices = string.Join(" ", contentTokens).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (!choices.Any()) {
          await client.SendToChannelAsync(args.Channel, $"{args.Sender}: There are no choices to choose from!");
          return;
        }
        var result = choices.OrderBy(c => GetRandomByte()).FirstOrDefault();
        await client.SendToChannelAsync(args.Channel, $"{args.Sender}: {result.Trim()}");
      }
    }

    private byte GetRandomByte() {
      byte[] b = new byte[1];
      RNG.GetBytes(b);
      return b[0];
    }
  }
}
