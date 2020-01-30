using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace FabelioScrape.Web.Models.Products
{
    public class ProductUseCase : IProductUseCase
    {
        private readonly IProductRepository _repo;
        private readonly HttpClient _httpClient;

        public ProductUseCase(IProductRepository repository, HttpClient httpClient)
        {
            _repo = repository;
            _httpClient = httpClient;
        }
    }
}
