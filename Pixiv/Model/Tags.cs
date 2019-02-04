using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace meguca.Pixiv.Model {
  class Tags {
    public long AuthorId;
    public bool IsLocked;
    [JsonProperty("tags")]
    public List<Tag> TagsCollection;
    public bool Writable;

    public override string ToString() {
      List<string> tags = new List<string>();
      foreach (var tag in TagsCollection) {
        if (tag.Translation != null && !string.IsNullOrWhiteSpace(tag.Translation.EN))
          tags.Add(tag.Translation.EN);
        else
          tags.Add($"{tag.Name}"); //fix later to add romaji properly if it exists
      }
      return string.Join(", ", tags);
    }
  }
}
