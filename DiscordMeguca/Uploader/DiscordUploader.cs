using Discord.Rest;
using Discord.WebSocket;
using meguca.Pixiv;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace meguca.DiscordMeguca.Uploader {
  public class DiscordUploader : IUploader {
    public async Task<RestUserMessage> SendImage(SocketMessage originalMessage, DownloadedImage image, string additionalText) {
      return await originalMessage.Channel.SendFileAsync(image.ImageData, image.Filename, string.IsNullOrEmpty(additionalText) ? null : additionalText.Trim());
    }
  }
}
