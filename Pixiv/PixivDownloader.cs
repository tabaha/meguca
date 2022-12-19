using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using meguca.Pixiv.Model;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using meguca.DiscordMeguca;

namespace meguca.Pixiv {

  public class DownloadedImage : IDisposable {
    public string Filename { get; }
    public int PageNumber { get; }
    public Stream ImageData{ get; }
    public bool IsOriginal { get; }

    public string Descritpion { get; }

    public DownloadedImage(string filename, int pageNumber, Stream imageData, bool isOriginal, string description = null) {
      Filename = filename;
      PageNumber = pageNumber;
      ImageData = imageData;
      IsOriginal = isOriginal;
      Descritpion = description;
    }

    public void Dispose() {
      ImageData.Dispose();
    }
  }

  class PixivDownloader {
    Settings Settings;

    private Cookie AuthCookie;
    private CookieContainer Cookies;

    private HttpClient HttpClient;
    private HttpClientHandler HttpClientHandler;

    public PixivDownloader(string settingsPath) {

      if(File.Exists(settingsPath)) {
        var json = File.ReadAllText(settingsPath);
        Settings = JsonConvert.DeserializeObject<Settings>(json);
      }
      else {
        Settings = new Settings() { Cookie = string.Empty, UserAgent = string.Empty, Path = "pixiv/" };
        File.WriteAllText(settingsPath, JsonConvert.SerializeObject(Settings));
      }

      AuthCookie = new Cookie("PHPSESSID", Settings.Cookie, "/", ".pixiv.net");
      Cookies = new CookieContainer();
      Cookies.Add(AuthCookie);
      HttpClientHandler = new HttpClientHandler() { CookieContainer = Cookies };
      HttpClient = new HttpClient(HttpClientHandler);
      HttpClient.DefaultRequestHeaders.Add("User-Agent", Settings.UserAgent);
    }

    public async Task<Illustration> GetIllustration(long id) {
      if (id <= 0)
        return null;

      Submission header;
      var page = await GetPage(Utils.GetWorkUrl(id), @"https://pixiv.net");
      try {
        var startJson = page.IndexOf("{\"token\":");
        var endJson = page.IndexOf("}'>", startJson) + 1;
        string headerText = page.Substring(startJson, endJson - startJson);
        header = JsonConvert.DeserializeObject<Submission>(headerText);

        startJson = page.IndexOf("content='", endJson) + "content='".Length;
        endJson = page.IndexOf("}'>", startJson) + 1;
        string preloadText = page.Substring(startJson, endJson - startJson);

        var preload = JsonConvert.DeserializeObject<Preload>(preloadText);
        header.Preload = preload;
      }
      catch (Exception ex) {
        Console.WriteLine(page);
        throw;
      }



      return header.Illustration;
    }

    public async Task<ArtistProfile> GetArtistProfile(long id) {
      if (id <= 0)
        return null;

      var page = GetPage(Utils.GetArtistProfileServiceURL(id), Utils.GetArtistUrl(id));
      var result = JsonConvert.DeserializeObject<ArtistProfilePage>(await page);

      if(result?.ArtistProfile != null) {
        result.ArtistProfile.Id = id;
        result.ArtistProfile.SetReferer(); 
      }

      return result?.ArtistProfile;
    }

    public async Task<SearchArtwork> GetSearchPage(IEnumerable<string> tags, int pageNumber) {
      if (!tags.Any() || pageNumber <= 0)
        return null;

      string serviceUrl = Utils.GetSearchImageServiceUrl(string.Join(" ", tags), pageNumber);
      string searchPageUrl = Utils.GetSearchImageURL(string.Join(" ", tags), pageNumber);
      var page = GetPage(serviceUrl, searchPageUrl);
      var result = JsonConvert.DeserializeObject<SearchArtworkPage>(await page);
      if (result?.SearchArtwork != null)
        result.SearchArtwork.Referer = searchPageUrl;

      return result?.SearchArtwork;
    }

    public IEnumerable<Task<DownloadedImage>> DownloadIllistrationAsync(Illustration illust, IEnumerable<int> pageNumbers = null, int? maxPages = null, int ? maxBytes = null ) {
      if (pageNumbers == null)
        pageNumbers = Enumerable.Range(0, illust.PageCount);
      if (maxPages.HasValue)
        pageNumbers = pageNumbers.Take(maxPages.Value);

      var workUrl = Utils.GetWorkUrl(illust.IllustID);
      if (string.IsNullOrWhiteSpace(illust.Urls.Original) || illust.IllustType == 2)
        return Enumerable.Empty<Task<DownloadedImage>>();
      return pageNumbers
        .Select((page) => {
          return DownloadToMemoryAsync(illust, workUrl, page, maxBytes);
        });
    }

    public IEnumerable<Task<DownloadedImage>> DownloadThumbnailsArtistProfileAsync(ArtistProfile artistProfile, Func<IllustrationThumbnail, string> description, IEnumerable<int> thumbnailNumbers = null, int? maxPages = null) {
      if (thumbnailNumbers == null)
        thumbnailNumbers = Enumerable.Range(0, artistProfile.Illustrations.Count);
      if (maxPages.HasValue)
        thumbnailNumbers = thumbnailNumbers.Take(maxPages.Value);

      var artistUrl = artistProfile.Url;
      if (!thumbnailNumbers.Any())
        return Enumerable.Empty<Task<DownloadedImage>>();

      return thumbnailNumbers
        .Select((page) => {
          return DownloadToMemoryAsync(artistProfile.Illustrations.ElementAt(page).Value, artistUrl, description); //please fix this code
        });
    }

    public IEnumerable<Task<DownloadedImage>> DownloadThumbnailsAsync(IEnumerable<IllustrationThumbnail> thumbnails, Func<IllustrationThumbnail, string> description) {
      return thumbnails
        .Select(thumbnail => {
          return DownloadToMemoryAsync(thumbnail, thumbnail.Referer, description); //please fix this code
        });
    }

    private async Task<string> GetPage(string url, string referer) {
      var requestMessage = CreatePixivRequestMessage(url, referer);
      var response = await HttpClient.SendAsync(requestMessage);
      return await response.Content.ReadAsStringAsync();
    }

    private HttpRequestMessage CreatePixivRequestMessage(string url, string referer) {
      var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
      httpRequestMessage.Version = new Version(2, 0);
      if (!string.IsNullOrEmpty(referer))
        httpRequestMessage.Headers.Referrer = new Uri(referer);
      return httpRequestMessage;
    }

    private void DownloadFile(string url, string referer, string filename) {

    }

    private async Task<DownloadedImage> DownloadToMemoryAsync(Illustration illust, string referer, int pageNumber, int? maxBytes = null) {

      #region Commented Ugoira code
      //if (illust.IllustType == 2) {
      //  //https://www.pixiv.net/ajax/illust/{id}/ugoira_meta
      //  string ext = Path.GetExtension(illust.Urls.Original);
      //  string pathUgoira = illust.Urls.Original.Replace("img-original", "img-zip-ugoira").Replace($"_ugoira0{ext}", "_ugoira1920x1080.zip");
      //  Task t = new Task(() => action(Path.GetFileName(pathUgoira), 0, DownloadToMemory(pathUgoira, workUrl)));
      //}
      #endregion

      string url = illust.Urls.Original.Replace("_p0", $"_p{pageNumber}");
      bool isOriginal = true;
      var message = CreatePixivRequestMessage(url, referer);
      var response = await HttpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
      if(maxBytes.HasValue && response.Content.Headers.ContentLength > maxBytes) {
        response.Dispose();
        url = illust.Urls.Regular.Replace("_p0", $"_p{pageNumber}");
        message = CreatePixivRequestMessage(url, referer);
        response = await HttpClient.SendAsync(message);
        isOriginal = false;
      }
      string fileName = Path.GetFileName(url);
      var stream = await response.Content.ReadAsStreamAsync();
      return new DownloadedImage(fileName, pageNumber, stream, isOriginal);
    }

    private async Task<DownloadedImage> DownloadToMemoryAsync(IllustrationThumbnail illust, string referer, Func<IllustrationThumbnail, string> description) {

      string url = illust.Url;
      bool isOriginal = false;
      var message = CreatePixivRequestMessage(url, referer);
      var response = await HttpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
      string fileName = Path.GetFileName(url);
      var stream = await response.Content.ReadAsStreamAsync();
      return new DownloadedImage(fileName, 0, stream, isOriginal, description(illust));
    }
  }
}
