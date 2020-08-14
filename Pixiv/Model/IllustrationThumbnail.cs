using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace meguca.Pixiv.Model {
  class IllustrationThumbnail {
    public long IllustId;
    public string illustTitle;
    public long Id;
    public string Title;
    public int IllustType;
    public int Restrict;
    public int XRestrict;
    public int Sl;
    public string Url;
    public string Description;
    public List<string> Tags;
    public long UserId;
    public string UserName;
    public int Width;
    public int Height;
    public int PageCount;
    public bool IsBookmarkable;
    public object BookmarkData;
    public string Alt;
    public bool IsAdContainer;
    public object TitleCaptionTranslation; //change!!
    public DateTime CreateDate;
    public DateTime UpdateDate;
    public string ProfileImageUrl;

    [JsonIgnore]
    public string Referer;
  }
}
