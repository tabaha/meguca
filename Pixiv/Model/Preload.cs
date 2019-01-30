using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace meguca.Pixiv.Model {
  class Preload {
    public DateTime TimeStamp;
    [JsonProperty("illust")]
    public Dictionary<long, Illustration> Illustration;
    public Object User;
  }
}
