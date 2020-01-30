using FabelioScrape.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FabelioScrape.Models.Products
{
    public interface IProductRepository
    {
        DbSet<Product> ProductSet { get; }

        Task<List<Product>> SearchAsync(Expression<Func<Product, bool>> criteria, int page, int size);
    }

    internal class ProductRepository : IProductRepository
    {
        private readonly IUnitOfWork _work;

        public DbSet<Product> ProductSet { get; }

        public ProductRepository(IUnitOfWork work)
        {
            _work = work;

            ProductSet = _work.Set<Product>();
        }

        public async Task<List<Product>> SearchAsync(Expression<Func<Product, bool>> criteria, int page, int size)
        {
            return await ProductSet.AsNoTracking().Where(criteria).Skip(page * size).Take(size).ToListAsync();
        }
    }
}
