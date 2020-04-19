using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using meguca.Pixiv.Model;
using System.Threading.Tasks;
using System.Linq;

namespace meguca.Pixiv {

  public class DownloadedImage : IDisposable {
    public string Filename { get; }
    public int PageNumber { get; }
    public MemoryStream ImageData { get; }

    public DownloadedImage(string filename, int pageNumber, MemoryStream imageData) {
      Filename = filename;
      PageNumber = pageNumber;
      ImageData = imageData;
    }

    public void Dispose() {
      ImageData.Dispose();
    }
  }

  public class DownloadedImageVoldy : IDisposable {
    public string Filename { get; }
    public int PageNumber { get; }
    public Stream ImageData{ get; }

    public DownloadedImageVoldy(string filename, int pageNumber, Stream imageData) {
      Filename = filename;
      PageNumber = pageNumber;
      ImageData = imageData;
    }

    public void Dispose() {
      ImageData.Dispose();
    }
  }

  class PixivDownloader {
    Settings Settings;

    private Cookie AuthCookie;
    private CookieContainer Cookies;
    

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

    }


    public Dictionary<string, MemoryStream> GetWork(string url) {
      return GetWork(url, Utils.GetID(url));
    }

    public Dictionary<string, MemoryStream> GetWork(long id) {
      return GetWork(Utils.GetWorkURL(id));
    }

    private Dictionary<string, MemoryStream> GetWork(string url, long id) {
      if (id <= 0)
        return new Dictionary<string, MemoryStream>();

      var page = GetPage(url, @"https://pixiv.net");
      var startJson = page.IndexOf("({token:");
      var endJson = page.IndexOf("});", startJson);
      page = page.Substring(startJson + 1, endJson - startJson);
      var jsonObj = JsonConvert.DeserializeObject<Page>(page);

      if(jsonObj.Preload.Illustration[id].PageCount == 1) {
        var imageUrl = jsonObj.Preload.Illustration[id].Urls.Original;
        if(!string.IsNullOrWhiteSpace(imageUrl)) {
          var dic = new Dictionary<string, MemoryStream>();
          dic.Add(Path.GetFileName(imageUrl), DownloadToMemory(imageUrl, url));
          return dic;
        }
      }
      return new Dictionary<string, MemoryStream>();
    }

    public Illustration GetIllustration(long id) {
      if (id <= 0)
        return null;

      var page = GetPage(Utils.GetWorkURL(id), @"https://pixiv.net");
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

    public Dictionary<string, MemoryStream> DownloadIllustration(Illustration illust, int? maxBytes = null) {
      var ret = new Dictionary<string, MemoryStream>();
      string workUrl = Utils.GetWorkURL(illust.IllustID);
      if(illust.IllustType == 2) {
        //https://www.pixiv.net/ajax/illust/{id}/ugoira_meta
        string ext = Path.GetExtension(illust.Urls.Original);
        string pathUgoira = illust.Urls.Original.Replace("img-original", "img-zip-ugoira").Replace($"_ugoira0{ext}", "_ugoira1920x1080.zip");
        ret.Add(Path.GetFileName(pathUgoira), DownloadToMemory(pathUgoira, workUrl));
      }
      else if (illust.PageCount == 1 && !string.IsNullOrWhiteSpace(illust.Urls.Original)) {
        ret.Add(Path.GetFileName(illust.Urls.Original), DownloadToMemory(illust.Urls.Original, workUrl));
      }
      else if(illust.PageCount > 1) {
        for(int page = 0; page < illust.PageCount; page++) {
          string pageUrl = illust.Urls.Original.Replace("_p0", $"_p{page}");
          ret.Add(Path.GetFileName(pageUrl), DownloadToMemory(pageUrl, workUrl));
        }
      }

      return ret;
    }

    public void DownloadIllustration(Illustration illust, Action<string, int, MemoryStream> action) {
      //var ret = new Dictionary<string, MemoryStream>();
      string workUrl = Utils.GetWorkURL(illust.IllustID);
      if (illust.IllustType == 2) {
        //https://www.pixiv.net/ajax/illust/{id}/ugoira_meta
        string ext = Path.GetExtension(illust.Urls.Original);
        string pathUgoira = illust.Urls.Original.Replace("img-original", "img-zip-ugoira").Replace($"_ugoira0{ext}", "_ugoira1920x1080.zip");
        Task t = new Task(() => action(Path.GetFileName(pathUgoira), 0, DownloadToMemory(pathUgoira, workUrl)));
        t.Start();
        //ret.Add(Path.GetFileName(pathUgoira), DownloadToMemory(pathUgoira, workUrl));
      }
      else if (illust.PageCount == 1 && !string.IsNullOrWhiteSpace(illust.Urls.Original)) {
        Task t = new Task(() => action(Path.GetFileName(illust.Urls.Original), 0, DownloadToMemory(illust.Urls.Original, workUrl)));
        t.Start();
        //ret.Add(Path.GetFileName(illust.Urls.Original), DownloadToMemory(illust.Urls.Original, workUrl));
      }
      else if (illust.PageCount > 1) {
        for (int page = 0; page < illust.PageCount; page++) {
          string pageUrl = illust.Urls.Original.Replace("_p0", $"_p{page}");
          Task t = new Task(() => action(Path.GetFileName(pageUrl), page, DownloadToMemory(pageUrl, workUrl)));
          t.Start();
        }
      }
      //return ret;
    }

    public async Task<IEnumerable<DownloadedImage>> DownLoadIllistrationTestAsync(Illustration illust) {
      string workUrl = Utils.GetWorkURL(illust.IllustID);
      if (!string.IsNullOrWhiteSpace(illust.Urls.Original)) {
        var tasks = new List<Task<DownloadedImage>>();
        for (int page = 0; page < illust.PageCount; page++) {
          string pageUrl = illust.Urls.Original.Replace("_p0", $"_p{page}");
          tasks.Add(DownloadToMemoryTest(pageUrl, workUrl, Path.GetFileName(pageUrl), page));
        }
        return await Task.WhenAll(tasks);
      }
      else
        return null;
    }

    public async Task<IEnumerable<DownloadedImageVoldy>> DownLoadIllistrationVoldyAsync(Illustration illust) {
      string workUrl = Utils.GetWorkURL(illust.IllustID);
      if (!string.IsNullOrWhiteSpace(illust.Urls.Original)) {
        var tasks = new List<Task<DownloadedImageVoldy>>();
        for (int page = 0; page < illust.PageCount; page++) {
          string pageUrl = illust.Urls.Original.Replace("_p0", $"_p{page}");
          tasks.Add(DownloadToMemoryVoldy(pageUrl, workUrl, Path.GetFileName(pageUrl), page));
        }
        return await Task.WhenAll(tasks);
      }
      else
        return await Task.FromResult(Enumerable.Empty<DownloadedImageVoldy>());
    }

    public IEnumerable<Task<DownloadedImageVoldy>> DownLoadIllistrationVoldyAsyncImproved(Illustration illust) {
      var workUrl = Utils.GetWorkURL(illust.IllustID);
      if (string.IsNullOrWhiteSpace(illust.Urls.Original))
        return Enumerable.Empty<Task<DownloadedImageVoldy>>();
      return Enumerable.Range(0, illust.PageCount)
        .Select((page) => {
          var pageUrl = illust.Urls.Original.Replace("_p0", $"_p{page}");
          return DownloadToMemoryVoldy(pageUrl, workUrl, Path.GetFileName(pageUrl), page);
        });
    }

    private string GetPage(string url, string referer) {
      var webRequest = CreatePixivWebRequest(url, referer);
      string html;
      using (var reader = new StreamReader(webRequest.GetResponse().GetResponseStream())) {
        html = reader.ReadToEnd();
      }
      return html;
    }

    private WebRequest CreatePixivWebRequest(string url, string referer) {
      var webRequest = WebRequest.CreateHttp(url);
      webRequest.UserAgent = Settings.UserAgent;
      webRequest.CookieContainer = Cookies;
      if (referer != null) webRequest.Referer = referer;
      return webRequest;
    }

    private void DownloadFile(string url, string referer, string filename) {
      var webRequest = CreatePixivWebRequest(url, referer);
      using (var imageResponse = webRequest.GetResponse()) {
        using (var fstream = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
          imageResponse.GetResponseStream().CopyTo(fstream);
        }
      }
    }

    private MemoryStream DownloadToMemory(string url, string referer) {
      var webRequest = CreatePixivWebRequest(url, referer);
      MemoryStream ms = new MemoryStream();
      using (var imageResponse = webRequest.GetResponse()) {
        imageResponse.GetResponseStream().CopyTo(ms);
      }
      ms.Position = 0;
      return ms;
    }

    private async Task<DownloadedImage> DownloadToMemoryTest(string url, string referer, string fileName, int pageNumber) {
      var webRequest = CreatePixivWebRequest(url, referer);
      MemoryStream ms = new MemoryStream();
      using (var imageResponse = await webRequest.GetResponseAsync()) {
        imageResponse.GetResponseStream().CopyTo(ms);
      }
      ms.Position = 0;
      return new DownloadedImage(fileName, pageNumber, ms);
    }

    private async Task<DownloadedImageVoldy> DownloadToMemoryVoldy(string url, string referer, string fileName, int pageNumber) {
      var webRequest = CreatePixivWebRequest(url, referer);
      var response = await webRequest.GetResponseAsync();
      var stream = response.GetResponseStream();
      return new DownloadedImageVoldy(fileName, pageNumber, stream);
    }
  }
}
