using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace meguca.Pixiv.Model {
  class SearchArtworkPage {
    public bool Error;
    [JsonProperty("body")]
    public SearchArtwork SearchArtwork;
  }
}
