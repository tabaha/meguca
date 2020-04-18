using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace meguca.Pixiv.Model {
  class Illustration {
    public long IllustID;
    public string IllustTitle;
    public string IllustComment;
    public long ID;
    public string Title;
    public string Description;
    public int IllustType;
    public DateTime CreateDate;
    public DateTime UploadDate;
    public int Restrict;
    public int XRestrict;
    public int Sl;
    public IllustrationUrls Urls;
    public Tags Tags;
    public string Alt;
    public List<string> StorableTags;
    public long UserID;
    public string UserName;
    public string UserAccount;
    public List<Object> UserIlusts;
    public bool LikeData;
    public int Width;
    public int Height;
    public int PageCount;
    public int BookmarkCount;
    public int LikeCount;
    public int CommentCount;
    public int ResponseCount;
    public int ViewCount;
    public bool IsHowTo;
    public bool IsOriginal;
    public Object ImageResponseOutData;
    public Object ImageResponseData;
    public int ImageResponseCount;
    public Object PollData;
    public Object SeriesNavData;
    public Object DescriptionBoothId;
    public Object DescriptionYoutubeId;
    public Object ComicPromotion;
    public Object FanboxPromotion;
    public Object ContestBanners;
    public FactoryGoods FactoryGoods;
    [JsonProperty("isBookmarkable")]
    public bool Bookmarkable;
    public Object BookmarkData;
    public Object ContestData;
    public Object ZoneConfig;
    public Object ExtraData;

    public bool IsSFW => XRestrict == 0;
    public bool IsR18 => XRestrict > 0;
    public bool IsR18G => XRestrict == 2;
  }
}
