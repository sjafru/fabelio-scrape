using System;
using System.Collections.Generic;
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
            var result = await $"{_apiOption.Value.ServiceUrl}/products?page={page}&size={size}".GetJsonAsync<Dictionary<string, object>>();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Post(string fabelioProductURL)
        {

            var result = await $"{_apiOption.Value.ServiceUrl}/products?fabelioProductURL={fabelioProductURL}".PostJsonAsync(new { })
                .ReceiveJson<Dictionary<string, object>>();

            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult> Put(string fabelioProductURL)
        {

            var result = await $"{_apiOption.Value.ServiceUrl}/products?fabelioProductURL={fabelioProductURL}".PutJsonAsync(new { })
                .ReceiveJson<Dictionary<string, object>>();

            return Ok(result);
        }
    }
}