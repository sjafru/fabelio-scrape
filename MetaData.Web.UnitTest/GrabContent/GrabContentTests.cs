using FabelioScrape.Web.Scraper;
using FluentAssertions;
using HtmlAgilityPack;
using Moq;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FabelioScrape.Web.UnitTest.GrabContents
{
    public class GrabContentTests : IDisposable
    {
        private readonly MockRepository _mockRepository;
        readonly HttpClient _client = new HttpClient();


        public GrabContentTests()
        {
            this._mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private ScrapeWebPage CreateGrabContent(HttpClient httpClient)
        {
            return new ScrapeWebPage(httpClient);
        }

        [Fact]
        public async Task SeleniumWebDriver_Expected()
        {
            var document = new HtmlDocument();

            using (var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
            {
                driver.Navigate().GoToUrl(@"https://fabelio.com/ip/set-1-1-kursi-taylor.html");
                Thread.Sleep(TimeSpan.FromSeconds(10));

                document.LoadHtml(driver.PageSource);

                //var images = driver.FindElements(By.XPath("//*[@class='fotorama__img']"));

                //var jsToBeExecuted = $"window.scroll(0, {link.Location.Y});";
                //((OpenQA.Selenium.IJavaScriptExecutor)driver).ExecuteScript(jsToBeExecuted);
                //var wait = new WebDriverWait(driver, TimeSpan.FromMinutes(1));
                //var clickableElement = wait.Until(ExpectedConditions.ElementToBeClickable(By.PartialLinkText("TFS Test API")));
                //clickableElement.Click();

                driver.Close();
            }


            var productImages = document.DocumentNode.FirstChild.SelectNodes("(//img[contains(@class,'fotorama__img')])");
            productImages.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetProductFabelio_Expected()
        {
            // Prepare
            var grabContent = this.CreateGrabContent(_client);

            var node = await grabContent.ReadHtml("https://fabelio.com/ip/set-1-1-kursi-taylor.html").SingleNodeAsync("(//div[contains(@class,'price-box price-final_price')])[1]");

            // Act
            var finalPriceNode = node.SelectSingleNode("(//span[contains(@data-price-type,'finalPrice')])[1]");
            var oldPriceNode = node.SelectSingleNode("(//span[contains(@data-price-type,'oldPrice')])[1]");

            var productImages = node.SelectNodes("(//img[contains(@class,'fotorama__img')])");

            // Assert
            productImages.Should().NotBeEmpty();

            finalPriceNode.Should().NotBeNull();
            oldPriceNode.Should().NotBeNull();

            var priceText = finalPriceNode.GetAttributeValue("data-price-amount", "");
            priceText.Should().NotBeNullOrEmpty();
            decimal finalPrice = decimal.Parse(priceText);
            finalPrice.Should().BeGreaterThan(0);

            priceText = oldPriceNode.GetAttributeValue("data-price-amount", "");
            priceText.Should().NotBeNullOrEmpty();
            decimal oldPrice = decimal.Parse(priceText);
            oldPrice.Should().BeGreaterThan(0);
        }

        public void Dispose()
        {
            this._mockRepository.VerifyAll();
        }
    }
}
