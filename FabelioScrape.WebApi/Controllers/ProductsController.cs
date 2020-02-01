using FabelioScrape.Infrastructure;
using FabelioScrape.Models;
using FabelioScrape.Models.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FabelioScrape.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IProductRepository _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _clientFactory;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _appSettings;
        const int MAX_RUNNER_INSTANCE = 7;

        public ProductsController(ILogger<ProductsController> logger, IProductRepository productRepo, IHttpClientFactory clientFactory, IUnitOfWork unitOfWork, IMemoryCache cache, IServiceProvider provider, IConfiguration appSettings)
        {
            _logger = logger;
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _clientFactory = clientFactory;
            _httpClient = _clientFactory.CreateClient();
            _cache = cache;
            _provider = provider;
            _appSettings = appSettings;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> Get(int page = 0, int size = 25)
        {
            List<Product> data = await _productRepo.SearchAsync(c => true, page, size);

            return Reply("Product List", data.Select(o => new ProductDto(o)).ToList());
        }

        private HttpStatusCode[] AcceptedStatusCodes => new HttpStatusCode[] { HttpStatusCode.OK, HttpStatusCode.Accepted };

        [HttpPost]
        public async Task<ActionResult> Post(string fabelioProductURL)
        {
            var (validateResult, pageResponse) = await Validate(fabelioProductURL);

            if (validateResult is BadRequestObjectResult)
                return validateResult;

            var product = await new ProductSync(fabelioProductURL, _httpClient, enableUrlValidation: false).StartSync(IntervalProductRecordedInMinutes);
            product.LastSyncStatus = pageResponse.StatusCode;

            _productRepo.ProductSet.Add(product);

            await _unitOfWork.SaveChangesAsync();

            return Reply($"Product has been created");
        }

        [HttpPut]
        public async Task<ActionResult> Put(string fabelioProductURL)
        {
            var (validateResult, pageResponse) = await Validate(fabelioProductURL, scenario: "update-product");

            if (validateResult is BadRequestObjectResult)
                return validateResult;

            var product = _productRepo.ProductSet.First(o => o.PageUrl == fabelioProductURL);

            if (ProductInRunners.Contains(product.Id.ToString()))
                return Reply($"Product in running for updated");
            else
                await AddProductToRunner(product.Id.ToString());

            await new ProductSync(fabelioProductURL, _httpClient, entity: product, enableUrlValidation: false).StartSync(IntervalProductRecordedInMinutes);

            product.LastSyncStatus = pageResponse.StatusCode;

            _productRepo.ProductSet.Update(product);

            await _unitOfWork.SaveChangesAsync();

            await RemoveProductFromRunner(product.Id.ToString());

            return Reply($"Product has been updated");
        }

        [HttpPost("sync")]
        public async Task<ActionResult> Sync(string syncTime)
        {
            if(ProductInRunners.Count >= MAX_RUNNER_INSTANCE)
                return Reply("Waiting available runner");

            var productsToUpdate = _productRepo.ProductSet.AsNoTracking().Where(c => c.NextSyncAt < DateTime.Now).Select(s => new { id = s.Id, url = s.PageUrl }).ToList();

            if (productsToUpdate.Any())
            {
                var httpClient = _clientFactory.CreateClient();
                Parallel.ForEach(productsToUpdate, async p =>
                {
                    using (var scope = _provider.CreateScope())
                    {
                        var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        if (ProductInRunners.Contains(p.id.ToString()))
                            return;
                        else
                            await AddProductToRunner(p.id.ToString());

                        var product = productRepo.ProductSet.Find(p.id);
                        try
                        {
                            await new ProductSync(p.url, httpClient, product).StartSync(IntervalProductRecordedInMinutes);
                        }
                        catch (System.Net.Http.HttpRequestException)
                        {
                            product.LastSyncStatus = HttpStatusCode.RequestTimeout;
                            product.LastSyncAt = DateTime.Now;
                            product.GenNextSync(IntervalProductRecordedInMinutes);
                        }
                        catch (OpenQA.Selenium.NotFoundException)
                        {
                            product.LastSyncStatus = HttpStatusCode.NotFound;
                            product.LastSyncAt = DateTime.Now;
                            product.GenNextSync(IntervalProductRecordedInMinutes);
                        }
                        catch (OpenQA.Selenium.WebDriverException)
                        {
                            product.LastSyncStatus = HttpStatusCode.RequestTimeout;
                            product.LastSyncAt = DateTime.Now;
                            product.GenNextSync(IntervalProductRecordedInMinutes);
                        }
                        catch (Exception)
                        {
                            product.LastSyncStatus = HttpStatusCode.RequestTimeout;
                            product.LastSyncAt = DateTime.Now;
                            product.NextSyncAt = DateTime.Now.AddMinutes(IntervalProductRecordedInMinutes);
                        }

                        productRepo.ProductSet.Update(product);

                        await unitOfWork.SaveChangesAsync();

                        await RemoveProductFromRunner(product.Id.ToString());
                    }
                });
            }

            return await Task.FromResult(Reply("Successfully Sync"));
        }

        const string SYNC_RUNNER_KEY = "SYNC_RUNNER";

        int IntervalProductRecordedInMinutes => _appSettings.GetValue<int>("Fabelio:IntervalProductRecordedInMinutes");

        IReadOnlyList<string> ProductInRunners => _cache.GetOrCreate(SYNC_RUNNER_KEY, c =>
        {
            return new List<string>();
        });

        private Task AddProductToRunner(string productId)
        {
            var runners = ProductInRunners.ToList();
            if (!runners.Contains(productId))
            {
                runners.Add(productId);
            }

            _cache.Set(SYNC_RUNNER_KEY, runners);

            return Task.CompletedTask;
        }

        private Task RemoveProductFromRunner(string productId)
        {
            var runners = ProductInRunners.ToList();
            if (runners.Contains(productId))
            {
                runners.Remove(productId);
            }

            _cache.Set(SYNC_RUNNER_KEY, runners);

            return Task.CompletedTask;
        }
        private async Task<Tuple<ActionResult, HttpResponseMessage>> Validate(string fabelioProductURL, string scenario = "new-product")
        {
            Uri uriResult;
            bool isUriValid = Uri.TryCreate(fabelioProductURL, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == Uri.UriSchemeHttp);

            bool isFabelioSite = uriResult.Host.Contains("fabelio");

            if (string.IsNullOrEmpty(fabelioProductURL) || !isUriValid || !isFabelioSite)
                return new Tuple<ActionResult, HttpResponseMessage>(ReplyError("Invalid Product Url"), null);

            if (scenario == "new-product")
            {
                if (_productRepo.ProductSet.AsNoTracking().Any(o => o.PageUrl == fabelioProductURL))
                    return new Tuple<ActionResult, HttpResponseMessage>(ReplyError("Product Already Exists"), null);
            }
            else if (scenario == "update-product")
            {
                if (!_productRepo.ProductSet.AsNoTracking().Any(o => o.PageUrl == fabelioProductURL))
                    return new Tuple<ActionResult, HttpResponseMessage>(ReplyError("Product Not Exists"), null);
            }

            var pageResponse = await _httpClient.GetAsync(fabelioProductURL);
            
            
            if (!AcceptedStatusCodes.Contains(pageResponse.StatusCode))
            {
                return new Tuple<ActionResult, HttpResponseMessage>(ReplyError(pageResponse.ReasonPhrase), null);
            }

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(await pageResponse.Content.ReadAsStringAsync());

            if(html.DocumentNode.SelectSingleNode("(//div[contains(@class,'product media')])[1]") == null)
            {
                return new Tuple<ActionResult, HttpResponseMessage>(ReplyError("Its not product page"), null);
            }

            return new Tuple<ActionResult, HttpResponseMessage>(Ok(), pageResponse);
        }

        private ActionResult ReplyError(string message)
        {
            return BadRequest(new
            {
                message,
                success = false,
                timestamp = string.Format("{0:s}{0:zzz}", DateTime.Now)
            });
        }

        private ActionResult Reply(string message, object data = null)
        {
            return Ok(new
            {
                success = true,
                message,
                timestamp = string.Format("{0:s}{0:zzz}", DateTime.Now),
                data
            });
        }

    }
}
