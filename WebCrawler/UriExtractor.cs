using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace WebCrawler
{
    public class UriExtractor
    {
        private readonly string[] filteringPrefix = {"http", "mail", "news"};

        public List<Uri> GetUriList(string html, Uri url)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var allNodesWithHref = doc.DocumentNode.Descendants("a")
                .Concat(doc.DocumentNode.Descendants("link"));

            return allNodesWithHref
                .Select(htmlNode => htmlNode.GetAttributeValue("href", null))
                .Where(href => !string.IsNullOrEmpty(href))
                .Where(href => !filteringPrefix.Any(href.StartsWith))
                .Select(href => GetAbsoluteUri(url, href))
                .ToList();
        }

        private static Uri GetAbsoluteUri(Uri url, string href)
        {
            if (href.StartsWith("."))
                href = $"{href.Substring(1, href.Length - 1)}";

            var uri = new Uri(url, href);

            return uri;
        }
    }
}