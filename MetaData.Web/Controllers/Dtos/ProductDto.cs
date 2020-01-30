using FabelioScrape.Web.Models;
using System;

namespace FabelioScrape.Web.Controllers
{
    public class ProductDto
    {
        public ProductDto(Product entity)
        {
            this.Id = entity.Id;
            this.Title = entity.Title;
            this.SubTitle = entity.SubTitle;
            this.Description = entity.Description;
            this.ImageUrls = entity.ImageUrls;
            this.PageUrl = entity.PageUrl;
            this.FinalPrice = entity.FinalPrice;
            this.OldPrice = entity.OldPrice;
            this.LastSyncAt = entity.LastSyncAt;
            this.NextSyncAt = entity.NextSyncAt;
        }

        public Guid Id { get; }
        public string PageUrl { get; }
        public string Title { get; }
        public string SubTitle { get; }
        public string Description { get; }
        public string[] ImageUrls { get; }
        public int FinalPrice { get; }
        public int OldPrice { get; }
        public DateTime LastSyncAt { get; }
        public DateTime NextSyncAt { get; }
    }
}