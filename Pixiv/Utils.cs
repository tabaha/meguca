using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;

namespace meguca.Pixiv {
  public class Utils {
    public static string WorkPageURL = @"https://www.pixiv.net/artworks/";
    public static string WorkPageURL_EN = @"https://www.pixiv.net/en/artworks/";

    public static string ArtistPageURL = @"https://www.pixiv.net/users/";
    public static string ArtistPageURL_EN = @"https://www.pixiv.net/en/users/";

    private static string UserProfileService = "https://www.pixiv.net/ajax/user/{0}/profile/top?lang=en";
    private static string UserAllImagesService = "https://www.pixiv.net/ajax/user/{0}/profile/all?lang=en";


    /// <summary>
    /// {0} - search term
    /// {1} - Order. Default = date_d
    /// {2} - Mode. Default = 2
    /// {3} - Page. Default = 1. Starts at 1
    /// {4} - Search Mode? Default = s_tag_full
    /// {5} - Type. Default = all
    /// </summary>
    private static string SearchImageService = "https://www.pixiv.net/ajax/search/artworks/{0}?word={0}&order={1}&mode={2}&p={3}&s_mode={4}&type={5}&lang=en";

    #region REGEX
    public static string WorkIDExpression = @"\d+";
    public static string ArtistIDExpression = @"\d+";

    public static Regex WorkIDRegex = new Regex(WorkIDExpression);
    public static Regex ArtistIDRegex = new Regex(ArtistIDExpression);
    #endregion

    public static string GetWorkUrl(long id) => WorkPageURL_EN + id;

    public static string GetArtistUrl(long id) => ArtistPageURL_EN + id;

    public static string GetArtistProfileServiceURL(long id) => string.Format(UserProfileService, id);

    public static string GetSearchImageServiceUrl(string tag, int page) => string.Format(SearchImageService, tag, "date_d", 2, page, "s_tag_full", "all");

    public static long GetID(string url) {
      var m = WorkIDRegex.Match(url);
      return (m.Success && long.TryParse(m.Value, out var result)) ? result : -1;
    }

    public static long GetArtistID(string url) {
      var m = ArtistIDRegex.Match(url);
      return (m.Success && long.TryParse(m.Value, out var result)) ? result : -1;
    }
  }
}
