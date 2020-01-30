using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace FabelioScrape.Web.Models
{
    public class Product
    {
        public Product(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }

        [MaxLength(255)]
        public string PageUrl { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        [MaxLength(64)]
        public string Title { get; set; }

        [MaxLength(64)]
        public string SubTitle { get; set; }

        public int FinalPrice { get; set; }

        public int OldPrice { get; set; }

        [NotMapped]
        public string[] ImageUrls { get => JsonConvert.DeserializeObject<string[]>(ImageUrls_Json); set => ImageUrls_Json = JsonConvert.SerializeObject(value); }

        [MaxLength(1000)]
        public string ImageUrls_Json { get; set; }

        public DateTime LastSyncAt { get; set; }

        public DateTime NextSyncAt { get; set; }

        public HttpStatusCode LastSyncStatus { get; set; }
    }
}
