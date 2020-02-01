using System;
using Xunit;
using FluentAssertions;
using System.IO;
using System.Reflection;
using System.Net;
using System.Threading.Tasks;

namespace FabelioScrape.WebApi.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task FabelioProduct_ImageParsing()
        {
            await DownloadImages(new string[] {
            "https://m2fabelio.imgix.net/catalog/product/cache/image/700x350/e9c3970ab036de70892d86c6d221abfe/k/a/karpet_maru_1.jpg",
            "https://m2fabelio.imgix.net/catalog/product/cache/image/700x350/e9c3970ab036de70892d86c6d221abfe/m/a/marru_carpet_small.jpg",
            "https://m2fabelio.imgix.net/catalog/product/cache/image/700x350/e9c3970ab036de70892d86c6d221abfe/m/a/marru_carpet_4.jpg",
            "https://m2fabelio.imgix.net/catalog/product/cache/thumbnail/88x110/beff4985b56e3afdbeabfc89641a4582/k/a/karpet_maru_1.jpg",
            "https://m2fabelio.imgix.net/catalog/product/cache/thumbnail/88x110/beff4985b56e3afdbeabfc89641a4582/m/a/marru_carpet_4.jpg",
            "https://m2fabelio.imgix.net/catalog/product/cache/thumbnail/88x110/beff4985b56e3afdbeabfc89641a4582/m/a/marru_carpet_2.jpg",
            "https://m2fabelio.imgix.net/catalog/product/cache/thumbnail/88x110/beff4985b56e3afdbeabfc89641a4582/m/a/marru_carpet_3.jpg",
            "https://m2fabelio.imgix.net/catalog/product/cache/thumbnail/88x110/beff4985b56e3afdbeabfc89641a4582/m/a/marru_carpet_small.jpg"
            });
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
            var imageDir = Path.Combine(path, $"wwwroot/images/{splits[4]}/{splits[5]}/{splits[6]}");

            bool folderExists = Directory.Exists(imageDir);
            if (!folderExists)
                Directory.CreateDirectory(imageDir);

            string imageName = imageDir + "/" + splits[9];

            using (WebClient client = new WebClient())
            {
                if (File.Exists(imageName))
                {
                    string tmpImageName = imageName + "-temp";
                    await client.DownloadFileTaskAsync(new Uri(imageUrl), tmpImageName);
                    File.Replace(tmpImageName, imageName, null);
                }
                else
                    client.DownloadFileAsync(new Uri(imageUrl), imageName);
            }
        }
    }
}
