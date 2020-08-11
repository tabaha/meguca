using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace meguca.Pixiv.Model {
  class ArtistProfilePage {
    public bool Error;
    public string Message;
    [JsonProperty("body")]
    public ArtistProfile ArtistProfile;
  }
}
