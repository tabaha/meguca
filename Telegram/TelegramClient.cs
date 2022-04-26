using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using meguca.Pixiv;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.IO;

namespace meguca.Telegram {
  class TelegramClient {

    public string Token { get; set; }

    [JsonIgnore()]
    private bool IsSetup = false;

    [JsonIgnore()]
    public PixivDownloader PixivDownloader { get; set; }

    [JsonIgnore()]
    public TelegramBotClient TelegramBot { get; set; }

    [JsonIgnore]
    CancellationTokenSource CTS = new CancellationTokenSource();

    public TelegramClient() { }

    public bool Setup() {
      TelegramBot = new TelegramBotClient(Token);

      

      return true;
    }

    public void Run() {
      var receiverOptions = new ReceiverOptions {
        AllowedUpdates = { } // receive all update types
      };
      TelegramBot.StartReceiving(
          HandleUpdateAsync,
          HandleErrorAsync,
          receiverOptions,
          cancellationToken: CTS.Token);

      Console.ReadLine();

      // Send cancellation request to stop bot
      CTS.Cancel();
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
      // Only process Message updates: https://core.telegram.org/bots/api#message
      if (update.Type != UpdateType.Message)
        return;
      // Only process text messages
      if (update.Message!.Type != MessageType.Text)
        return;

      var chatId = update.Message.Chat.Id;
      var messageText = update.Message.Text;

      int maxPages = 4;

      if (messageText.StartsWith("/pixiv")) {
        try {
          long id = Pixiv.Utils.GetWorkID(messageText);
          var illust = await PixivDownloader.GetIllustration(id);

          List<IAlbumInputMedia> items = new List<IAlbumInputMedia>();

          bool isFirstSent = true;

          foreach (var imageTask in PixivDownloader.DownloadIllistrationAsync(illust, maxPages: 10, maxBytes: 8388119)) {
            using (var image = await imageTask) {
              var ms = new MemoryStream();
              image.ImageData.CopyTo(ms);
              ms.Position = 0;
              items.Add(new InputMediaPhoto(new InputMedia(ms, image.Filename)) { Caption = isFirstSent? illust.ToString() : null});
              isFirstSent = false;
              //image.ImageData.Position = 0;
            }
          }

          await botClient.SendMediaGroupAsync(chatId, items);

          
          //foreach (var imageTask in PixivDownloader.DownloadIllistrationAsync(illust, maxPages: 4, maxBytes: 8388119).ToList()) {
          //  using (var image = await imageTask) {
          //    string text = isFirstSent ? illust.ToString() : string.Empty;
          //    if (isFirstSent && maxPages > 0 && illust.PageCount > maxPages) ;
          //      text += $" [Showing {maxPages} images out of {illust.PageCount}]";
          //    if (!image.IsOriginal)
          //      text += " (preview version)";
          //    text = text.Trim();
          //    isFirstSent = false;
          //    Console.WriteLine($"Sending page {image.PageNumber}");
          //    await botClient.SendPhotoAsync(chatId, new InputOnlineFile(image.ImageData, image.Filename), string.IsNullOrEmpty(text) ? null : text);
          //  }
          //}
          return;
        }
        catch (Exception ex) { 
        }
      }

      Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

      // Echo received message text
      //Message sentMessage = await botClient.SendTextMessageAsync(
      //    chatId: chatId,
      //    text: "You said:\n" + messageText,
      //    cancellationToken: cancellationToken);

    }

    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
      var ErrorMessage = exception switch {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
      };

      Console.WriteLine(ErrorMessage);
      return Task.CompletedTask;
    }
  }
}
