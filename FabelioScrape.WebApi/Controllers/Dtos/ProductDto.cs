using FabelioScrape.Models;
using System;
using System.Linq;

namespace FabelioScrape.Controllers
{
    public class ProductDto
    {
        public ProductDto(Product entity)
        {
            this.Id = entity.Id;
            this.Title = entity.Title;
            this.SubTitle = entity.SubTitle;
            this.Description = System.Net.WebUtility.HtmlDecode(entity.Description);
            this.ImageUrls = entity.ImageUrls;
            this.PageUrl = entity.PageUrl;
            this.FinalPrice = entity.FinalPrice;
            this.OldPrice = entity.OldPrice;
            this.LastSyncAt = entity.LastSyncAt;
            this.LastSyncStatus = entity.LastSyncStatus.ToString();
            this.NextSyncAt = entity.NextSyncAt;
        }

        public Guid Id { get; }
        public string PageUrl { get; }
        public string Title { get; }
        public string SubTitle { get; }
        public string Description { get; private set; }
        public string[] ImageUrls { get; }
        public int FinalPrice { get; }
        public int OldPrice { get; }
        public DateTime LastSyncAt { get; }
        public string LastSyncStatus { get; }
        public DateTime NextSyncAt { get; }
        public string[] Images { get; private set; }

        internal ProductDto SetLocalImages(string currentHost)
        {
            var localImageDir = $"{currentHost}/images/";

            this.Images = this.ImageUrls.Select(i => i.Replace("https://m2fabelio.imgix.net/catalog/product/cache/", localImageDir)).ToArray();
            return this;
        }

        internal ProductDto ShowDescription(bool visible)
        {
            if(!visible)
                this.Description = string.Empty;
                
            return this;
        }
    }
}