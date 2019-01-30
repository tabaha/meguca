using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using meguca.Pixiv.Model;

namespace meguca.Pixiv {
  class Downloader {
    Settings Settings;

    private Cookie AuthCookie;
    private CookieContainer Cookies;
    

    public Downloader(string settingsPath) {

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


      var workTypeMatch = Utils.WorkTypeRegex.Match(page);
      if(workTypeMatch.Success) {
        switch (workTypeMatch.Groups["type"].Value) {
          case "manga":
            break;
          default:
            break;
        }
      }
      else {
        var singeWorkMatch = Utils.SingleWorkLocationRegex.Match(page);
        using (StreamWriter w = new StreamWriter("test.html")) {
          w.Write(page);
        }
        if (singeWorkMatch.Success && singeWorkMatch.Groups["extension"].Success) {
          DownloadFile(singeWorkMatch.Value, url, $"{id}_p0.{singeWorkMatch.Groups["extension"].Value}");
        }
      }
    }

    private void DownloadSinglePageWork() {

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
  }
}
