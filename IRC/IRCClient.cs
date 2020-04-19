using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using meguca.DiscordMeguca;
using Discord;
using System.Linq;
using meguca.Pixiv;
using System.Collections.Generic;
using meguca.IRC.Commands;
using Newtonsoft.Json;

namespace meguca.IRC {
  class IRCClient {

    private TcpClient IRCTcpClient;
    private NetworkStream NetworkStream;
    private StreamReader Reader;
    private StreamWriter Writer;
    public IRCSettings Settings { get; private set; }
    public event EventHandler<IRCEventArgs> ReadLine;
    public event EventHandler<IRCEventArgs> WriteLine;
    [JsonIgnore()]
    public DiscordClient DiscordClient;
    [JsonIgnore()]
    public PixivDownloader PixivDownloader;
    public Decide DecideCommand { get; private set; }
    public UrlTitle UrlTitleCommand { get; private set; }

    public IRCClient() {
      ReadLine += Ping;
      ReadLine += Identify;
      ReadLine += DisplayLine;
      WriteLine += DisplayLine;

      Settings = new IRCSettings();
      DecideCommand = new Decide();
      UrlTitleCommand = new UrlTitle();

      ReadLine += DecideCommand.Process;
      ReadLine += UrlTitleCommand.Process;
    }

    private void DisplayLine(object sender, IRCEventArgs e) {
      Console.WriteLine(e.Line);
    }

    private async void Identify(object sender, IRCEventArgs e) {
      if (e.Tokens[1].Equals("251")) {
        if (!string.IsNullOrWhiteSpace(Settings.NickservPassword))
          await SendAsync($"PRIVMSG NickServ :Identify {Settings.NickservPassword}");
        System.Threading.Thread.Sleep(1000);
        foreach (string channel in Settings.AutoJoinChannels)
          await SendAsync($"JOIN {channel}");
      }
    }

    private void Ping(object sender, IRCEventArgs e) {
      if (e.Tokens[0].Equals("PING"))
        Send(string.Format("PONG {0}", e.Tokens[1].Substring(1)));
    }

    public void Setup() {
      ReadLine += GetPixivAsync;
    }

    public void Connect() {
      try {
        IRCTcpClient = new TcpClient(Settings.Hostname, Settings.Port);
        NetworkStream = IRCTcpClient.GetStream();
        Reader = new StreamReader(NetworkStream);
        Writer = new StreamWriter(NetworkStream) { NewLine = "\r\n", AutoFlush = true };
        Send("USER " + Settings.Username + " meg " + " uca" + " :" + Settings.Realname);
        Send("NICK " + Settings.Nickname);
      }
      catch {
        Console.WriteLine("Communication Error");
      }
    }

    public void Run() {
      try {
        string line;
        while (true) {
          while ((line = Reader.ReadLine()) != null) {
            OnReadLine(new IRCEventArgs(DateTime.Now, line));
          }
        }
      }
      catch (IOException ioe) {
        Console.WriteLine(ioe);
      }
    }


    public void Send(string text) {
      Writer.WriteLine(text);
      OnWriteLine(new IRCEventArgs(DateTime.Now, text));
    }

    public async Task SendAsync(string text) {
      await Writer.WriteLineAsync(text);
      OnWriteLine(new IRCEventArgs(DateTime.Now, text));
    }

    public async Task SendToChannelAsync(string channel, string text) {
      await SendAsync($"PRIVMSG {channel} :{text}");
    }

    private async void GetPixivAsync(object sender, IRCEventArgs args) {
      if (args.Tokens[1] == "PRIVMSG" && args.Tokens[2] == "#onioniichan") {
        var msgToken = args.Tokens[3];
        if (msgToken.StartsWith(":"))
          msgToken = msgToken.Substring(1);
        if (msgToken.Contains(Pixiv.Utils.WorkPageURL_EN) || msgToken.Contains(Pixiv.Utils.WorkPageURL)) {
          try {
            long id = Pixiv.Utils.GetID(msgToken);
            var channel = DiscordClient.Client.GetChannel(337692280267997196) as IMessageChannel;
            var illust = await PixivDownloader.GetIllustration(id);
            string tags = illust.Tags.ToString();
            foreach (var result in PixivDownloader.DownLoadIllistrationAsync(illust, 8388119).ToList()) {
              using (var image = await result) {
                string text = image.PageNumber == 0 ? $"Tags: {tags}" : string.Empty;
                if (!image.IsOriginal)
                  text += " (preview version)";
                var response = await channel.SendFileAsync(image.ImageData, image.Filename, string.IsNullOrEmpty(text) ? null : text.Trim());
                foreach (var attach in response.Attachments.Select(a => a.Url))
                  await SendToChannelAsync("#onioniichan", attach);
              }
            }
          }
          catch (Exception ex) {
            Console.WriteLine(ex.Message);
            await SendToChannelAsync("#onioniichan", "Error fetching the image");
          }
          finally {

          }
        }
      }
    }

    public virtual void OnReadLine(IRCEventArgs args) {
      ReadLine?.Invoke(this, args);
    }
    public virtual void OnWriteLine(IRCEventArgs args) {
      WriteLine?.Invoke(this, args);
    }
  }
}
