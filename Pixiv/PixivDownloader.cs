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

    public DownloadedImage(string filename, int pageNumber, Stream imageData, bool isOriginal) {
      Filename = filename;
      PageNumber = pageNumber;
      ImageData = imageData;
      IsOriginal = isOriginal;
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
      }

      AuthCookie = new Cookie("PHPSESSID", Settings.Cookie, "/", ".pixiv.net");
      Cookies = new CookieContainer();
      Cookies.Add(AuthCookie);
      HttpClientHandler = new HttpClientHandler() { CookieContainer = Cookies };
      HttpClient = new HttpClient(HttpClientHandler);
    }

    public async Task<Illustration> GetIllustration(long id) {
      if (id <= 0)
        return null;

      var page = await GetPage(Utils.GetWorkURL(id), @"https://pixiv.net");
      var startJson = page.IndexOf("{\"token\":");
      var endJson = page.IndexOf("}'>", startJson) + 1;
      string headerText = page.Substring(startJson, endJson - startJson);
      var header = JsonConvert.DeserializeObject<Page>(headerText);

      startJson = page.IndexOf("content='", endJson) + "content='".Length;
      endJson = page.IndexOf("}'>", startJson) + 1;
      string preloadText = page.Substring(startJson, endJson - startJson);

      var preload = JsonConvert.DeserializeObject<Preload>(preloadText);
      header.Preload = preload;



      return header.Illustration;
    }

    public async Task<IEnumerable<DownloadedImage>> DownLoadIllistrationAsyncAlternate(Illustration illust, int? maxBytes = null) {
      string workUrl = Utils.GetWorkURL(illust.IllustID);
      if (!string.IsNullOrWhiteSpace(illust.Urls.Original) || illust.IllustType == 2) {
        var tasks = new List<Task<DownloadedImage>>();
        for (int page = 0; page < illust.PageCount; page++) {
          string pageUrl = illust.Urls.Original.Replace("_p0", $"_p{page}");
          tasks.Add(DownloadToMemoryAsync(illust, workUrl, page, maxBytes));
        }
        return await Task.WhenAll(tasks);
      }
      else
        return await Task.FromResult(Enumerable.Empty<DownloadedImage>());
    }

    public IEnumerable<Task<DownloadedImage>> DownLoadIllistrationAsync(Illustration illust, int? maxBytes = null) {
      var workUrl = Utils.GetWorkURL(illust.IllustID);
      if (string.IsNullOrWhiteSpace(illust.Urls.Original) || illust.IllustType == 2)
        return Enumerable.Empty<Task<DownloadedImage>>();
      return Enumerable.Range(0, illust.PageCount)
        .Select((page) => {
          var pageUrl = illust.Urls.Original.Replace("_p0", $"_p{page}");
          return DownloadToMemoryAsync(illust, workUrl, page, maxBytes);
        });
    }

    private async Task<string> GetPage(string url, string referer) {
      var requestMessage = CreatePixivRequestMessage(url, referer);
      var response = await HttpClient.SendAsync(requestMessage);
      return await response.Content.ReadAsStringAsync();
    }

    private HttpRequestMessage CreatePixivRequestMessage(string url, string referer) {
      var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
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
  }
}
