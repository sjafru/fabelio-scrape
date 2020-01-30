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

        public ProductsController(ILogger<ProductsController> logger, IProductRepository productRepo, IHttpClientFactory clientFactory, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> Get(int page = 0, int size = 25)
        {
            List<Product> data = await _productRepo.SearchAsync(c => true, page, size);

            return Ok(data.Select(o => new ProductDto(o)).ToList());
        }

        private HttpStatusCode[] AcceptedStatusCodes => new HttpStatusCode[] { HttpStatusCode.OK, HttpStatusCode.Accepted };

        [HttpPost]
        public async Task<ActionResult> Post(string fabelioProductURL)
        {
            if (_productRepo.ProductSet.AsNoTracking().Any(o => o.PageUrl == fabelioProductURL))
                ModelState.AddModelError("fabelioProductURL", "Already Exists");

            if (!ModelState.IsValid)
                return BadRequest();

            var httpClient = _clientFactory.CreateClient();

            var pageResponse = await httpClient.GetAsync(fabelioProductURL);
            if (!AcceptedStatusCodes.Contains(pageResponse.StatusCode))
            {
                ModelState.AddModelError("fabelioProductURL", pageResponse.ReasonPhrase);
                return BadRequest();
            }

            var product = await new ProductSync(fabelioProductURL, httpClient).StartSync();
            product.LastSyncStatus = pageResponse.StatusCode;

            _productRepo.ProductSet.Add(product);

            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }
    }
}
