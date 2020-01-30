using FabelioScrape.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace FabelioScrape.Infrastructure
{
    public class MetaDataDbContext : DbContext, IUnitOfWork
    {
        public MetaDataDbContext(DbContextOptions<MetaDataDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(etb => {
                etb.HasKey(k => k.Id);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
