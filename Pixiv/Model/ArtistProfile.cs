using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace meguca.Pixiv.Model {
  class ArtistProfile {
    public long Id;
    [JsonProperty("illusts")]
    public Dictionary<long, IllustrationThumbnail> Illustrations;
    [JsonProperty("manga")]
    public JToken MangaToken;
    [JsonIgnore]
    public Dictionary<long, IllustrationThumbnail> Manga => MangaToken switch
    {
      JObject obj => obj.ToObject<Dictionary<long, IllustrationThumbnail>>(),
      _ => new Dictionary<long, IllustrationThumbnail>()
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

    public void SetReferer() { 
        if (Illustrations != null)
          foreach (var illust in Illustrations.Values)
            illust.Referer = Url;

        if (Manga != null)
          foreach (var manga in Manga.Values)
            manga.Referer = Url;

        //if (Novels != null)
    }
  }
}