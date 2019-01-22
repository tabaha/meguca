using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace meguca.IRC
{
  class IRCClient
  {

    private TcpClient IRCTcpClient;
    private NetworkStream NetworkStream;
    private StreamReader Reader;
    private StreamWriter Writer;
    public IRCSettings Settings { get; private set; }
    public event EventHandler<IRCEventArgs> ReadLine;
    public event EventHandler<IRCEventArgs> WriteLine;

    public IRCClient(IRCSettings settings)
    {
      Settings = settings;
      ReadLine += (sender, args) =>
      {
        if (args.Tokens[0].Equals("PING"))
          Send(string.Format("PONG {0}", args.Tokens[1].Substring(1)));
      };
      ReadLine += async (sender, args) =>
      {
        if (args.Tokens[1].Equals("251")) {
          if (!string.IsNullOrWhiteSpace(Settings.NickservPassword))
            await SendAsync($"PRIVMSG NickServ :Identify {Settings.NickservPassword}");
          System.Threading.Thread.Sleep(1000);
          foreach (string channel in Settings.AutoJoinChannels)
            await SendAsync($"JOIN {channel}");

        }

      };
      ReadLine += (sender, args) =>
      {
        Console.WriteLine(args.Line);
      };
      WriteLine += (sender, args) =>
      {
        Console.WriteLine(args.Line);
      };
    }

    public void Connect()
    {
      try
      {
        IRCTcpClient = new TcpClient(Settings.Hostname, Settings.Port);
        NetworkStream = IRCTcpClient.GetStream();
        Reader = new StreamReader(NetworkStream);
        Writer = new StreamWriter(NetworkStream) { NewLine = "\r\n", AutoFlush = true };
        Send("USER " + Settings.Username + " meg " + " uca" + " :" + Settings.Realname);
        Send("NICK " + Settings.Nickname);
      }
      catch
      {
        Console.WriteLine("Communication Error");
      }
    }

    public void Run()
    {
      try
      {
        string line;
        while (true)
        {
          while ((line = Reader.ReadLine()) != null)
          {
            OnReadLine(new IRCEventArgs() { Time = DateTime.Now, Line = line, Tokens = line.Split(' ') });
          }
        }
      }
      catch (IOException ioe)
      {
        Console.WriteLine(ioe);
      }
    }


    public void Send(string text)
    {
      Writer.WriteLine(text);
      OnWriteLine(new IRCEventArgs() { Time = DateTime.Now, Line = text });
    }

    public async Task SendAsync(string text)
    {
      await Writer.WriteLineAsync(text);
      OnWriteLine(new IRCEventArgs() { Time = DateTime.Now, Line = text });
    }

    public virtual void OnReadLine(IRCEventArgs args)
    {
      ReadLine?.Invoke(this, args);
    }
    public virtual void OnWriteLine(IRCEventArgs args)
    {
      WriteLine?.Invoke(this, args);
    }
  }
}
