using FabelioScrape.Web.Scraper;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FabelioScrape.Web.Models.Products
{
    internal class ProductSync
    {
        private readonly string _productUrl;
        private readonly IReadHtmlContent _readHtml;
        private readonly Product _entity;

        public ProductSync(string productUrl, HttpClient httpClient, Product entity = null)
        {
            _productUrl = productUrl;
            _readHtml = new ScrapeWebPage(httpClient).ReadHtml(productUrl);
            _entity = entity ?? new Product(Guid.NewGuid());
        }

        public async Task<Product> StartSync()
        {
            _entity.Title = await GetTitle();
            _entity.SubTitle = await GetSubTitle();
            _entity.ImageUrls =  await GetImageUrls();
            _entity.LastSyncAt = DateTime.Now;
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

            return System.Web.HttpUtility.HtmlEncode(node.InnerHtml);
        }

        private async Task<string[]> GetImageUrls()
        {
            var node = await _readHtml.SelectNodesAsync("(//img[contains(@class,'fotorama__img')])");
            return node.Select(o => o.GetAttributeValue("src", "")).ToArray();
        }

        private async Task<string> GetSubTitle()
        {
            var node = await _readHtml.SingleNodeAsync("(//div[contains(@class,'page-title__secondary')])[1]");
            return node.InnerText;
        }

        private async Task<string> GetTitle()
        {
            var node = await _readHtml.SingleNodeAsync("(//span[contains(@data-ui-id,'page-title-wrapper')])[1]");
            return node.InnerText;
        }
    }
}
