using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;


namespace modul9_bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new TelegramBotClient(""); //токен бота
            client.StartReceiving(Update, Error); //обработка методов обновления и ошибки

            if (!Directory.Exists(@"download")) //проверка папки для загруженных файлов
            {
                Directory.CreateDirectory(@"download");
            }


            Console.ReadLine();

        }

        private static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3) //метод обработки ошибки
        {
            throw new NotImplementedException(); //действие по ошибке
        }

        async static Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery) //метод обработки данных от кнопки
        {
            Console.WriteLine(callbackQuery.Data); //данные кнопки пишем в консоль
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"скачиваем {callbackQuery.Data.Substring(9)}");
            await using Stream stream = System.IO.File.OpenRead(callbackQuery.Data);
            await botClient.SendDocumentAsync(callbackQuery.Message.Chat.Id, new InputOnlineFile(stream, callbackQuery.Data.Substring(9))); //отделяем имя файла от пути
            return;
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token) //метод обновления бота
        {
            var message = update.Message; //сокращение


            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery) //проверка нажатия кнопки
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery); //вызов метода бработки данных кнопки
                return;
            }

            if (message.Text != null) //если сообщение не пустое
            {
                Console.WriteLine($"{message.Chat.FirstName}   |   {message.Text}"); //имя пользователя и сообщение в консоль
                if (message.Text.ToLower().Contains("привет")) //проверка приветствия 
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "добрый день");
                    return;
                }

                if (message.Text == "/start") //команда начала работы
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вас приветствует Super Bot! Список доступных команд: \n/filelist - вывод скачанных на диск файлов \n/pcchoise - помощь в подборе комплектующих для компьютера");
                    return;
                }

                if (message.Text == "/pcchoise")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите сферу применения компьютеры: \n/home - для дома и офиса \n/game - игровой \n/ultra - игровой с запасом мощности");
                    return;
                }

                if (message.Text == "/home")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Процессор: AMD Ryzen 3 4100\r\nВидеокарта: Palit GeForce GTX 1050 Ti\r\nМатеринская плата: GIGABYTE B450M DS3H\r\nОперативня память: Kingston FURY Beast Black 8 ГБ\r\nБлок питания: DEEPCOOL PF750\r\nСтоимость: 36100 руб.");
                }
                if (message.Text == "/game")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Процессор: AMD Ryzen 5 5600X\r\nВидеокарта: PowerColor AMD Radeon RX 6600\r\nМатеринская плата: GIGABYTE B550M DS3H\r\nОперативня память: A-Data XPG SPECTRIX D41 RGB 16 ГБ\r\nБлок питания: Cooler Master MWE Bronze 750 V2\r\nСтоимость: 50100 руб.");
                }
                if (message.Text == "/ultra")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Процессор: AMD Ryzen 7 5800X\r\nВидеокарта: GIGABYTE GeForce RTX 3060 Ti\r\nМатеринская плата: MSI MAG B550 TOMAHAWK MAX WIFI\r\nОперативня память: Patriot Viper Steel 32 ГБ\r\nБлок питания: MSI MPG A750GF\r\nСтоимость: 83300 руб.");
                }


                if (message.Text == "/filelist") //вывод кнопок
                {

                    string[] allfiles = Directory.GetFiles(@"download"); //создаём массив из всех скачанных файлов
                    foreach (string filename in allfiles)
                    {
                        InlineKeyboardMarkup keyboard = new(new[] //создаём кнопки в цикле
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(filename,filename),
                            },
                        });
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Нажмите для скачивания {filename} ↓", replyMarkup: keyboard); //вывод кнопки в чат
                        Console.WriteLine(filename);
                    }
                    return;
                }

            }

            if (message.Document != null) //проверка сообщения на документ
            {
                Random rand = new Random(); //создаём генератор случайных чисел
                int fileName = rand.Next(10000, 99999); //выбираем диапазон случайных чисел
                var fileId = update.Message.Document.FileId; //считываем файл из сообщения
                var fileInfo = await botClient.GetFileAsync(fileId); //собираем инфо по файлу
                var filePath = fileInfo.FilePath; //берём имя файла
                var extension = Path.GetExtension(filePath); //берём расширение файла
                string destinationFilePath = $@"download\{fileName}{extension}"; //задаём путь сохранения файла
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath); //запускаем файловый стрим
                await botClient.DownloadFileAsync(filePath, fileStream); //сохраняем файл
                fileStream.Close(); //закрываем стрим
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Файл загружен, переименован в {fileName}{extension} и сохранён в папку download"); //уведомление в чат о скачивании
                return;
            }
            //ниже по аналогии с аудио и фото

            if (message.Audio != null) //проверка сообщения на аудио
            {
                Random rand = new Random();
                int fileName = rand.Next(10000, 99999);
                var fileId = update.Message.Audio.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePath = fileInfo.FilePath;
                var extension = Path.GetExtension(filePath);
                string destinationFilePath = $@"download\{fileName}{extension}";
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                await botClient.DownloadFileAsync(filePath, fileStream);
                fileStream.Close();
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Файл загружен, переименован в {fileName}{extension} и сохранён в папку download");
                return;
            }

            if (message.Photo != null) //проверка сообщения на фото
            {
                Random rand = new Random();
                int fileName = rand.Next(10000, 99999);
                var fileId = update.Message.Photo[message.Photo.Count() - 1].FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePath = fileInfo.FilePath;
                var extension = Path.GetExtension(filePath);
                string destinationFilePath = $@"download\{fileName}{extension}";
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                await botClient.DownloadFileAsync(filePath, fileStream);
                fileStream.Close();
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Файл загружен, переименован в {fileName}{extension} и сохранён в папку download");
                return;
            }
        }
    }
}
