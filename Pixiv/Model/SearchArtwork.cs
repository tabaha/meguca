using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace meguca.Pixiv.Model {
  class SearchArtworkResults {
    [JsonProperty("data")]
    public List<IllustrationThumbnail> Illustrations;
    public int Total;
    [JsonProperty("bookmarkRanges")]
    public object[] Ranges;
  }

  class PopularPreview {
    public List<IllustrationThumbnail> Recent;
    public List<IllustrationThumbnail> Permanent;
  }

  class SearchArtwork {
    [JsonProperty("illustManga")]
    public SearchArtworkResults Results;
    [JsonProperty("popular")]
    public PopularPreview PopularPreview;
    public List<string> RelatedTags;
    [JsonProperty("tagTranslation")]
    public Dictionary<string, TagTranslation> TagTranslations;

    public object ZoneConfig;
    public object ExtraData;

    private string _referer;
    public string Referer {
      get {
        return _referer;
      }
      set {
        _referer = value;
        Results?.Illustrations?.ForEach(t => t.Referer = _referer);
        PopularPreview?.Permanent?.ForEach(t => t.Referer = _referer);
        PopularPreview?.Recent?.ForEach(t => t.Referer = _referer);
      }
    }
  }
}
