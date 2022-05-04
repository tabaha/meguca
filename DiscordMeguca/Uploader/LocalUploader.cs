using Discord.Rest;
using Discord.WebSocket;
using meguca.Pixiv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace meguca.DiscordMeguca.Uploader {
  public class LocalUploader : IUploader {
    public string BaseUrl { get; set; }
    public string Directory { get; set; }

    public int? MaxBytes => null;

    public async Task<RestUserMessage> SendImage(SocketMessage originalMessage, DownloadedImage image, string additionalText) {
      string filename = Directory + image.Filename;
      if (image.ImageData != null && !File.Exists(filename)) {
        using(FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
          await image.ImageData.CopyToAsync(fs);
        }
      }
      if (File.Exists(filename)) {
        return await originalMessage.Channel.SendMessageAsync(BaseUrl + image.Filename + (!string.IsNullOrEmpty(additionalText) ? $" [{additionalText}]" : string.Empty));
      }
      else
        return await new Task<RestUserMessage>(() => null);
    }

    public async Task<RestUserMessage> SendImage(SocketSlashCommand command, DownloadedImage image, string additionalText) {
      string filename = Directory + image.Filename;
      if (image.ImageData != null && !File.Exists(filename)) {
        using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
          await image.ImageData.CopyToAsync(fs);
        }
      }
      if (File.Exists(filename)) {
        return await command.Channel.SendMessageAsync(BaseUrl + image.Filename + (!string.IsNullOrEmpty(additionalText) ? $" [{additionalText}]" : string.Empty));
      }
      else
        return await new Task<RestUserMessage>(() => null);
    }

    public async Task<RestUserMessage> SendImage(ISocketMessageChannel channel, DownloadedImage image, string additionalText) {
      string filename = Directory + image.Filename;
      if (image.ImageData != null && !File.Exists(filename)) {
        using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
          await image.ImageData.CopyToAsync(fs);
        }
      }
      if (File.Exists(filename)) {
        return await channel.SendMessageAsync(BaseUrl + image.Filename + (!string.IsNullOrEmpty(additionalText) ? $" [{additionalText}]" : string.Empty));
      }
      else
        return await new Task<RestUserMessage>(() => null);
    }
  }
}
