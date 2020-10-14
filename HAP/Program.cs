using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HAP
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Book> books = new List<Book>();

            int j = 0;
            Parallel.For(1, 3, i =>
            {
                var url = $"https://avidreaders.ru/books/{i}";
                var web = new HtmlWeb();
                var doc = web.Load(url);
                var hrefs = doc.DocumentNode.SelectNodes(@"//a").Where(a => a.InnerText == "Подробнее").Select(a => a.GetAttributeValue("href", ""));

                Parallel.ForEach(hrefs, (href) =>
                {
                    WebClient webClient = new WebClient();

                    var book = web.Load(href);

                    var title = book.DocumentNode.SelectSingleNode(@"//h1[@itemprop='name']")?.InnerText;
                    var author = book.DocumentNode.SelectSingleNode(@"//a[@itemprop='item']/span[@itemprop='name']")?.InnerText;
                    var genre = book.DocumentNode.SelectSingleNode("//p[@itemprop='genre']/a")?.InnerText;

                    var imagePath = book.DocumentNode.SelectSingleNode("//img[@itemprop='image']")?.GetAttributeValue("src","");
                    string imageName = $"{++j}" + Path.GetExtension(imagePath);
                    webClient.DownloadFile(imagePath, Path.Combine(Environment.CurrentDirectory, "images", imageName));
                    books.Add(new Book() { Author = author, Genre = genre, Title = title, ImageName = imageName });
                    Console.WriteLine($"Название: {title}, Автор: {author} , Жанр: {genre}", imageName);
                });
            });
            Console.WriteLine("Запись в файл");
            var json = JsonConvert.SerializeObject(books);
            using(StreamWriter sw = new StreamWriter(Path.Combine(Environment.CurrentDirectory,"Books-p5.json"),false))
            {
                sw.Write(json);
                sw.Close();
            }
            Console.WriteLine("Все готово");
        }
    }
}
