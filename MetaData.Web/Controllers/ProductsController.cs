using FabelioScrape.Web.Infrastructure;
using FabelioScrape.Web.Models;
using FabelioScrape.Web.Models.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FabelioScrape.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IProductRepository _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _clientFactory;
        private readonly HttpClient _httpClient;

        public ProductsController(ILogger<ProductsController> logger, IProductRepository productRepo, IHttpClientFactory clientFactory, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _clientFactory = clientFactory;
            _httpClient = _clientFactory.CreateClient();
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

            var product = await new ProductSync(fabelioProductURL, _httpClient, enableUrlValidation: false).StartSync();
            product.LastSyncStatus = pageResponse.StatusCode;

            _productRepo.ProductSet.Add(product);

            await _unitOfWork.SaveChangesAsync();

            return Reply($"Product has been created");
        }

        private async Task<Tuple<ActionResult,HttpResponseMessage>> Validate(string fabelioProductURL, string scenario = "new-product")
        {
            Uri uriResult;
            bool isUriValid = Uri.TryCreate(fabelioProductURL, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == Uri.UriSchemeHttp);

            bool isFabelioSite = uriResult.Host.Contains("fabelio");

            if (string.IsNullOrEmpty(fabelioProductURL) || !isUriValid || !isFabelioSite)
                return new Tuple<ActionResult, HttpResponseMessage> (ReplyError("Invalid Product Url"), null);

            if(scenario == "new-product")
            {
                if (_productRepo.ProductSet.AsNoTracking().Any(o => o.PageUrl == fabelioProductURL))
                    return new Tuple<ActionResult, HttpResponseMessage>(ReplyError("Product Already Exists"), null);
            }
            else if(scenario == "update-product")
            {
                if (!_productRepo.ProductSet.AsNoTracking().Any(o => o.PageUrl == fabelioProductURL))
                    return new Tuple<ActionResult, HttpResponseMessage>(ReplyError("Product Not Exists"), null);
            }
            
            var pageResponse = await _httpClient.GetAsync(fabelioProductURL);
            if (!AcceptedStatusCodes.Contains(pageResponse.StatusCode))
            {
                return new Tuple<ActionResult, HttpResponseMessage>(ReplyError(pageResponse.ReasonPhrase), null);
            }

            return new Tuple<ActionResult, HttpResponseMessage>(Ok(), pageResponse);
        }

        private ActionResult ReplyError(string message)
        {
            return BadRequest(new
            {
                errorMessage = message,
                success = false,
                timestamp = string.Format("{0:s}{0:zzz}", DateTime.Now)
            });
        }

        private ActionResult Reply(string message, object data = null)
        {
            return Ok(new { 
                success = true,
                message,
                timestamp = string.Format("{0:s}{0:zzz}", DateTime.Now),
                data
            });
        }

        [HttpPut]
        public async Task<ActionResult> Put(string fabelioProductURL)
        {
            var (validateResult, pageResponse) = await Validate(fabelioProductURL, scenario: "update-product");

            if (validateResult is BadRequestObjectResult)
                return validateResult;

            var product = _productRepo.ProductSet.First(o=>o.PageUrl == fabelioProductURL);

            await new ProductSync(fabelioProductURL, _httpClient, entity: product, enableUrlValidation: false).StartSync();

            product.LastSyncStatus = pageResponse.StatusCode;

            _productRepo.ProductSet.Update(product);

            await _unitOfWork.SaveChangesAsync();

            return Reply($"Product has been updated");
        }
    }
}
