using Discord.Rest;
using Discord.WebSocket;
using meguca.Pixiv;
using meguca.Pixiv.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace meguca.DiscordMeguca.Uploader {
  public interface IUploader {
    int? MaxBytes { get; }
    Task<RestUserMessage> SendImage(SocketMessage originalMessage, DownloadedImage image, string additionalText);
    Task<RestUserMessage> SendImage(SocketSlashCommand command, DownloadedImage image, string additionalText);
    Task<RestUserMessage> SendImage(ISocketMessageChannel channel, DownloadedImage image, string additionalText);
  }
}
