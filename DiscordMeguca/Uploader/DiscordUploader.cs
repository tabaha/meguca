using Discord.Rest;
using Discord.WebSocket;
using meguca.Pixiv;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace meguca.DiscordMeguca.Uploader {
  public class DiscordUploader : IUploader {
    public int? MaxBytes => 8388119;

    public async Task<RestUserMessage> SendImage(SocketMessage originalMessage, DownloadedImage image, string additionalText) {
      return await originalMessage.Channel.SendFileAsync(image.ImageData, image.Filename, string.IsNullOrEmpty(additionalText) ? null : additionalText.Trim());
    }

    public async Task<RestUserMessage> SendImage(SocketSlashCommand command, DownloadedImage image, string additionalText) {
      return await command.Channel.SendFileAsync(image.ImageData, image.Filename, string.IsNullOrEmpty(additionalText) ? null : additionalText.Trim());
    }
    public async Task<RestUserMessage> SendImage(ISocketMessageChannel channel, DownloadedImage image, string additionalText) {
      return await channel.SendFileAsync(image.ImageData, image.Filename, string.IsNullOrEmpty(additionalText) ? null : additionalText.Trim());
    }
  }
}
