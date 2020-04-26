using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace meguca.IRC.Commands {
  public class UrlTitle : IIRCCommand {
    public string Name => "Url Title";

    public string Description => "";

    public Dictionary<string, List<string>> Triggers { get; set ; }

    public List<string> UrlBlacklist { get; set; }
    public UrlTitle() {
      Triggers = new Dictionary<string, List<string>>();
      UrlBlacklist = new List<string>();
    }
    public void Process(object sender, IRCEventArgs e) {
      if(e.IsChannelMessage && Triggers.TryGetValue(e.Channel, out var triggers)) {
        var urls = e.ContentTokens.Where(s => s.StartsWith("http:", StringComparison.InvariantCultureIgnoreCase) || s.StartsWith("https:", StringComparison.InvariantCultureIgnoreCase));
        if(urls.Any() && (triggers.Contains(string.Empty) || triggers.Contains(e.ContentTokens.First()))) {
          foreach(var s in urls) {
            if (UrlBlacklist.Any(b => s.Contains(b)))
              continue;
            Task t = new Task(async () => {
              string title = GetUrlTitle(s);
              if (!string.IsNullOrWhiteSpace(title))
                await (sender as IRCClient).SendToChannelAsync(e.Channel, title);
            });
            t.Start();
          }
        }
      }
    }

    private string GetUrlTitle(string url) {
      try {
        var request = WebRequest.CreateHttp(url);
        string html;
        using (var reader = new StreamReader(request.GetResponse().GetResponseStream())) {
          html = reader.ReadToEnd();
        }
        int start = html.IndexOf("<title>");
        int end = html.IndexOf("</title>", start == -1 ? 0 : start);

        string title = null;
        if (start != -1 && end != -1)
          title = html.Substring(start + 7, (end) - (start + 7));

        if (!string.IsNullOrEmpty(title))
          title = WebUtility.HtmlDecode(title).Trim().Replace(Environment.NewLine, "; ").Replace("\n", "; ");

        return title;
      }
      catch (Exception) {
        return "Error retrieving site info";
      }
    }

  }
}
