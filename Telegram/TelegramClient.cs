using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;

namespace meguca.Telegram {
  public class TelegramClient {

    public string Token { get; set; }

    [JsonIgnore()]
    private bool IsSetup = false;

    [JsonIgnore()]
    public TelegramBotClient Bot { get; set; }

    public TelegramClient() { }

    public bool Setup() {
      Bot = new TelegramBotClient(Token);

      return true;
    }
  }
}
