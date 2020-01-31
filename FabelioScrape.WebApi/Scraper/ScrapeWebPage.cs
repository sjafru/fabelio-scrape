
using AngleSharp;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace FabelioScrape.Scraper
{
    public class ScrapeWebPage : IScrapeWebPage
    {
        private readonly HttpClient _httpClient;

        public ScrapeWebPage(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IReadHtmlContent ReadHtml(string pageUrl, bool enableUrlValidation = true)
        {
            return new ReadHtmlContent(pageUrl, _httpClient, enableUrlValidation);
        }
    }

    internal class ReadHtmlContent : IReadHtmlContent
    {
        private readonly HttpClient _httpClient;

        public string PageUrl { get; }
        public bool EnableUrlValidation { get; }
        public HtmlDocument Document { get; private set; }

        private HtmlNode _Html;
        public async Task<HtmlNode> GetHtml()
        {
            if (_Html == null)
            {
                if (EnableUrlValidation)
                {
                    var pageResponse = await _httpClient.GetAsync(PageUrl);
                    if (!AcceptedStatusCodes.Contains(pageResponse.StatusCode))
                        throw new NotFoundException($"{PageUrl} {pageResponse.StatusCode.ToString()} {pageResponse.ReasonPhrase}");
                }

                Document = new HtmlDocument();

                using (var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
                {
                    driver.Navigate().GoToUrl(PageUrl);
                    Thread.Sleep(TimeSpan.FromSeconds(20));
                    Document.LoadHtml(driver.PageSource);

                    driver.Close();
                }

                _Html = Document.DocumentNode.FirstChild;
            }

            return _Html;
        }

        public ReadHtmlContent(string pageUrl, HttpClient httpClient, bool enableUrlValidation)
        {
            this._httpClient = httpClient;
            this.PageUrl = pageUrl;
            this.EnableUrlValidation = enableUrlValidation;
        }

        public async Task<HtmlNode> SingleNodeAsync(string xpath)
        {
            return (await GetHtml()).SelectSingleNode(xpath);
        }

        public async Task<HtmlNodeCollection> SelectNodesAsync(string xpath)
        {
            return (await GetHtml()).SelectNodes(xpath);
        }

        private HttpStatusCode[] AcceptedStatusCodes => new HttpStatusCode[] { HttpStatusCode.OK, HttpStatusCode.Accepted };
    }

    public interface IReadHtmlContent
    {
        Task<HtmlNodeCollection> SelectNodesAsync(string xpath);

        Task<HtmlNode> SingleNodeAsync(string xpath);
    }
}