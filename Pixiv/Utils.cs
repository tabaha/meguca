﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace meguca.Pixiv {
  public class Utils {
    public static string WorkPageURL = @"https://www.pixiv.net/artworks/";
    public static string WorkPageURL_EN = @"https://www.pixiv.net/en/artworks/";
    //private static string BookmarkNewIllustURL = @"https://www.pixiv.net/bookmark_new_illust.php?p=";
    //private static string BigMangaURL = @"https://www.pixiv.net/member_illust.php?mode=manga_big&illust_id=";
    //private static string MangaURL = @"https://www.pixiv.net/member_illust.php?mode=manga&illust_id=";

    #region REGEX
    private static string SingleWorkLocationExpression = @"https?://i.pximg.net/img-original/img/\d+/\d+/\d+/\d+/\d+/\d+/\d+_p\d+.(?<extension>\w+)";
    private static string WorkThumbnailExpression = @"https?://i.pximg.net/c/150x150/img-master/img/\d+/\d+/\d+/\d+/\d+/\d+/(?<pixivid>\d+)(_p0)?_master1200.(?<extension>\w+)";
    private static string WorkPageExpression = "class=\"image-item\"><a href=\"/member_illust.php\\?mode=medium&amp;illust_id=(?<pixivid>\\d+)";
    private static string WorkTypeExpression = @"member_illust.php\?mode=(?<type>(big|manga|ugoira_view))&amp;illust_id=";
    private static string MultiplePagesNumberExpression = @"<li>Multiple images: (?<numPages>\d+)P</li>";

    public static string WorkIDExpression = @"\d+";

    //public static Regex SingleWorkLocationRegex = new Regex(SingleWorkLocationExpression);
    //public static Regex WorkThumbnailRegex = new Regex(WorkThumbnailExpression);
    //public static Regex WorkPageRegex = new Regex(WorkPageExpression);
    //public static Regex WorkTypeRegex = new Regex(WorkTypeExpression);
    //public static Regex MultiplePagesNumberRegex = new Regex(MultiplePagesNumberExpression);

    public static Regex WorkIDRegex = new Regex(WorkIDExpression);
    #endregion

    public static string GetWorkURL(long id) {
      return WorkPageURL_EN + id;
    }

    //public static string GetBookmarkNewIllustURL(int page) {
    //  return BookmarkNewIllustURL + page;
    //}

    //public static string GetBigMangaURL(long id, int pageNumber) {
    //  return BigMangaURL + id + "&page=" + pageNumber;
    //}

    //public static string GetMangaURL(long id) {
    //  return MangaURL + id;
    //}

    public static long GetID(string url) {
      var m = WorkIDRegex.Match(url);
      return (m.Success && long.TryParse(m.Value, out var result)) ? result : -1;
    }
  }
}
