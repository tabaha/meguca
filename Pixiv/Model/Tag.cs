using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace meguca.Pixiv.Model {
  class Tag {
    [JsonProperty("tag")]
    public string Name;
    public bool Locked;
    public bool Deletable;
    public long UserId;
    public string Romaji;
    [JsonProperty("translation")]
    public Translation Translation;
    public string Username;
  }
}
