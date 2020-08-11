using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace meguca.Pixiv.Model {
  class ArtistProfile {
    public long Id;
    [JsonProperty("illusts")]
    public Dictionary<long, MiniIllustInfo> Illustrations;
    [JsonProperty("manga")]
    public JToken MangaToken;
    [JsonIgnore]
    public Dictionary<long, MiniMangaInfo> Manga => MangaToken switch
    {
      JObject obj => obj.ToObject<Dictionary<long, MiniMangaInfo>>(),
      _ => new Dictionary<long, MiniMangaInfo>()
    };
    [JsonProperty("novels")]
    public JToken NovelsToken;
    [JsonIgnore]
    public Dictionary<long, object> Novels => NovelsToken switch
    {
      JObject obj => obj.ToObject<Dictionary<long, object>>(),
      _ => new Dictionary<long, object>()
    };
    public object RequestPostWorks;
    public object ZoneConfig;
    public object ExtraData;
    [JsonIgnore]
    public string Url => Utils.GetArtistUrl(Id);
  }
}