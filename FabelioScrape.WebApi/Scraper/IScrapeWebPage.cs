namespace FabelioScrape.Scraper
{
    public interface IScrapeWebPage
    {
        IReadHtmlContent ReadHtml(string pageUrl, bool enableUrlValidation = true);
    }
}