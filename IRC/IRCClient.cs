using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using meguca.DiscordMeguca;
using Discord;
using System.Linq;
using meguca.Pixiv;
using System.Collections.Generic;

namespace meguca.IRC {
  class IRCClient {

    private TcpClient IRCTcpClient;
    private NetworkStream NetworkStream;
    private StreamReader Reader;
    private StreamWriter Writer;
    public IRCSettings Settings { get; private set; }
    public event EventHandler<IRCEventArgs> ReadLine;
    public event EventHandler<IRCEventArgs> WriteLine;
    public DiscordClient DiscordClient;
    public PixivDownloader PixivDownloader;

    public IRCClient(IRCSettings settings, PixivDownloader downloader) {
      Settings = settings;
      PixivDownloader = downloader;

      ReadLine += (sender, args) => {
        if (args.Tokens[0].Equals("PING"))
          Send(string.Format("PONG {0}", args.Tokens[1].Substring(1)));
      };
      ReadLine += async (sender, args) => {
        if (args.Tokens[1].Equals("251")) {
          if (!string.IsNullOrWhiteSpace(Settings.NickservPassword))
            await SendAsync($"PRIVMSG NickServ :Identify {Settings.NickservPassword}");
          System.Threading.Thread.Sleep(1000);
          foreach (string channel in Settings.AutoJoinChannels)
            await SendAsync($"JOIN {channel}");

        }
      };
      
      ReadLine += (sender, args) => {
        Console.WriteLine(args.Line);
      };
      WriteLine += (sender, args) => {
        Console.WriteLine(args.Line);
      };
    }

    public void Setup() {
      ReadLine += async (sender, args) => {
        if (args.Tokens[1] == "PRIVMSG" && args.Tokens[2] == "#onioniichan") {
          var msgToken = args.Tokens[3];
          if (msgToken.StartsWith(":"))
            msgToken = msgToken.Substring(1);
          if (msgToken.StartsWith(Pixiv.Utils.WorkPageURL)) {
            Dictionary<string, MemoryStream> downloadedImages = new Dictionary<string, MemoryStream>();
            try {
              long id = Pixiv.Utils.GetID(msgToken);
              var channel = DiscordClient.Client.GetChannel(337692280267997196) as IMessageChannel;
              var illust = PixivDownloader.GetIllustration(id);
              downloadedImages = PixivDownloader.DownloadIllustration(illust);
              foreach (var result in downloadedImages) {
                var response = await channel.SendFileAsync(result.Value, result.Key);
                foreach (var attach in response.Attachments.Select(a => a.Url))
                  await SendToChannelAsync("#onioniichan", attach);
              }
            }
            catch (Exception ex){
              Console.WriteLine(ex.Message);
              await SendToChannelAsync("#onioniichan", "Error fetching the image");
            }
            finally {
              foreach (var ms in downloadedImages.Values)
                ms.Dispose();
              downloadedImages.Clear();
            }
          }
        }
      };
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
            OnReadLine(new IRCEventArgs() { Time = DateTime.Now, Line = line, Tokens = line.Split(' ') });
          }
        }
      }
      catch (IOException ioe) {
        Console.WriteLine(ioe);
      }
    }


    public void Send(string text) {
      Writer.WriteLine(text);
      OnWriteLine(new IRCEventArgs() { Time = DateTime.Now, Line = text });
    }

    public async Task SendAsync(string text) {
      await Writer.WriteLineAsync(text);
      OnWriteLine(new IRCEventArgs() { Time = DateTime.Now, Line = text });
    }

    public async Task SendToChannelAsync(string channel, string text) {
      await SendAsync($"PRIVMSG {channel} :{text}");
    }

    public virtual void OnReadLine(IRCEventArgs args) {
      ReadLine?.Invoke(this, args);
    }
    public virtual void OnWriteLine(IRCEventArgs args) {
      WriteLine?.Invoke(this, args);
    }
  }
}
