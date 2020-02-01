using FabelioScrape.Scraper;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace FabelioScrape.Models.Products
{
    internal static class ProductExtensions
    {
        public static void GenNextSync(this Product product, int minutesInterval)
        {
            product.NextSyncAt = DateTime.Now.AddMinutes(minutesInterval);
        }
    }
    internal class ProductSync
    {
        private readonly string _productUrl;
        private readonly IReadHtmlContent _readHtml;
        private readonly Product _entity;

        public ProductSync(string productUrl, HttpClient httpClient, Product entity = null, bool enableUrlValidation = true)
        {
            _productUrl = productUrl;
            _readHtml = new ScrapeWebPage(httpClient).ReadHtml(productUrl, enableUrlValidation);
            _entity = entity ?? new Product(Guid.NewGuid());
        }

        public async Task<Product> StartSync(int minutesInterval = 5)
        {
            _entity.Title = await GetTitle();
            _entity.SubTitle = await GetSubTitle();
            _entity.ImageUrls =  await GetImageUrls();
            _entity.LastSyncAt = DateTime.Now;
            _entity.GenNextSync(minutesInterval);
            _entity.Description = await GetDescriptionHtmlEncode();
            _entity.FinalPrice = await GetFinalPrice();
            _entity.OldPrice = await GetOldPrice();
            _entity.PageUrl = _productUrl;

            return _entity;
        }

        private async Task<int> GetOldPrice()
        {
            var node = await _readHtml.SingleNodeAsync("(//span[contains(@data-price-type,'oldPrice')])[1]");
            return node.GetAttributeValue("data-price-amount", 0);
        }

        private async Task<int> GetFinalPrice()
        {
            var node = await _readHtml.SingleNodeAsync("(//span[contains(@data-price-type,'finalPrice')])[1]");
            return node.GetAttributeValue("data-price-amount", 0);
        }

        private async Task<string> GetDescriptionHtmlEncode()
        {
            var node = await _readHtml.SingleNodeAsync("(//div[contains(@id,'description')])[1]");

            return System.Net.WebUtility.HtmlEncode(node?.InnerHtml);
        }

        private async Task<string[]> GetImageUrls()
        {
            var node = await _readHtml.SelectNodesAsync("(//img[contains(@class,'fotorama__img')])");
            
            var images =  node.Any() ? node.Select(o => o.GetAttributeValue("src", "")).ToArray() : new string[]{};

            if (images.Any())
                await DownloadImages(images);

            return images;
        }

        private Task DownloadImages(string[] urls)
        {
            Parallel.ForEach(urls, imgUrl => DownloadImage(imgUrl).Wait());

            return Task.CompletedTask;
        }

        private async Task DownloadImage(string imageUrl)
        {
            var splits = imageUrl.Replace("https://", "").Split("/", 10);
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.IndexOf("bin\\")));
            var imageDir = Path.Combine(path, $"wwwroot/images/{splits[4]}/{splits[5]}/{splits[6]}/{splits[7]}/{splits[8]}");

            bool folderExists = Directory.Exists(imageDir);
            if (!folderExists)
                Directory.CreateDirectory(imageDir);

            string imageName = imageDir + "/" + splits[9];

            using WebClient client = new WebClient();
            if (File.Exists(imageName))
            {
                string tmpImageName = imageName + "-temp";
                await client.DownloadFileTaskAsync(new Uri(imageUrl), tmpImageName);
                File.Replace(tmpImageName, imageName, null);
            }
            else
                client.DownloadFileAsync(new Uri(imageUrl), imageName);
        }

        private async Task<string> GetSubTitle()
        {
            var node = await _readHtml.SingleNodeAsync("(//div[contains(@class,'page-title__secondary')])[1]");
            return node?.InnerText;
        }

        private async Task<string> GetTitle()
        {
            var node = await _readHtml.SingleNodeAsync("(//span[contains(@data-ui-id,'page-title-wrapper')])[1]");
            return node?.InnerText;
        }
    }
}
