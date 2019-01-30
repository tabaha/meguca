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
  }
}
