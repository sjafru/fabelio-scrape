using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FabelioScrape.Web.Infrastructure;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FabelioScrape.Web.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController : ControllerBase
    {
        private readonly IOptions<ApiServiceOption> _apiOption;

        public ProductsController(IOptions<ApiServiceOption> apiOption)
        {
            _apiOption = apiOption;
        }

        [HttpGet]
        public async Task<ActionResult> Get(int page = 0, int size = 25)
        {
            var result = await $"{_apiOption.Value.ServiceUrl}/products?page={page}&size={size}"
                .AllowHttpStatus(HttpStatusCode.BadRequest)
                .GetStringAsync();
            return Content(result, "application/json");
        }

        [HttpGet("detail")]
        public async Task<ActionResult> Detail(string id)
        {
            var result = await $"{_apiOption.Value.ServiceUrl}/products/{id}"
                .AllowHttpStatus(HttpStatusCode.BadRequest)
                .GetStringAsync();
            return Content(result, "application/json");
        }

        [HttpPost]
        public async Task<ActionResult> Post(string fabelioProductURL)
        {
            var result = await $"{_apiOption.Value.ServiceUrl}/products?fabelioProductURL={fabelioProductURL}"
                .AllowHttpStatus(HttpStatusCode.BadRequest)
                .PostJsonAsync(new { })
                .ReceiveString();

            return Content(result, "application/json");
        }

        [HttpPut]
        public async Task<ActionResult> Put(string fabelioProductURL)
        {
            var result = await $"{_apiOption.Value.ServiceUrl}/products?fabelioProductURL={fabelioProductURL}"
                .AllowHttpStatus(HttpStatusCode.BadRequest)
                .PutJsonAsync(new { })
                .ReceiveString();

            return Content(result, "application/json");
        }
    }
}