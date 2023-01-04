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
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.IO;
using Telegram.Bot.Polling;
using Newtonsoft.Json;

namespace meguca.Telegram {

  public class InputMediaPhotoSpoiler : InputMediaPhoto {

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? HasSpoiler { get; set; }
    public InputMediaPhotoSpoiler(InputMedia media) : base(media) {
      HasSpoiler = true;
    }
  }

  class TelegramClient {

    public string Token { get; set; }

    [JsonIgnore()]
    private bool IsSetup = false;

    [JsonIgnore()]
    public PixivDownloader PixivDownloader { get; set; }

    [JsonIgnore()]
    public TelegramBotClient TelegramBot { get; set; }

    [JsonIgnore]
    public CancellationTokenSource CTS = new CancellationTokenSource();

    public TelegramClient() { }

    public bool Setup() {
      TelegramBot = new TelegramBotClient(Token);



      return true;
    }

    public async Task Run() {
      var receiverOptions = new ReceiverOptions {
        AllowedUpdates = { } // receive all update types
      };
      TelegramBot.StartReceiving(
          HandleUpdateAsync,
          HandleErrorAsync,
          receiverOptions,
          cancellationToken: CTS.Token);

      //Console.ReadLine();

      // Send cancellation request to stop bot
      await Task.Delay(-1);
      // CTS.Cancel();
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

      if (messageText.StartsWith("/pixiv") || messageText.StartsWith(Utils.WorkPageURL_EN) || messageText.StartsWith(Pixiv.Utils.WorkPageURL) ||
              messageText.StartsWith("<" + Pixiv.Utils.WorkPageURL_EN) || messageText.StartsWith("<" + Pixiv.Utils.WorkPageURL) || messageText.StartsWith("!pixiv")) {
        try {

          long id = Pixiv.Utils.GetWorkID(messageText);
          await SendIllustration(botClient, chatId, id);
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

    private async Task SendIllustration(ITelegramBotClient botClient, long chatId, long id) {
      var illust = await PixivDownloader.GetIllustration(id);

      List<IAlbumInputMedia> items = new List<IAlbumInputMedia>();

      bool isFirstSent = true;

      foreach (var imageTask in PixivDownloader.DownloadIllistrationAsync(illust, maxPages: 10, maxBytes: 8388119)) {
        using (var image = await imageTask) {
          Console.WriteLine($"Downloading page {image.PageNumber}");
          var ms = new MemoryStream();
          image.ImageData.CopyTo(ms);
          ms.Position = 0;
          items.Add(new InputMediaPhotoSpoiler(new InputMedia(ms, image.Filename)) { Caption = isFirstSent ? Pixiv.Utils.GetWorkUrl(id) + "   " + illust.ToString() : null, ParseMode = ParseMode.MarkdownV2});
          isFirstSent = false;
          //image.ImageData.Position = 0;
        }
      }

      await botClient.SendMediaGroupAsync(chatId, items, disableNotification: true);
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
