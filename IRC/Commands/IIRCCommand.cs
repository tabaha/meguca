using System;
using System.Collections.Generic;
using System.Text;

namespace meguca.IRC.Commands {
  public interface IIRCCommand {
    string Name { get; }
    string Description { get; }
    Dictionary<string, List<string>> Triggers { get; set; }
    void Process(object sender, IRCEventArgs args);
  }
}
