using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WebCrawler
{
    public class FileSaver
    {
        private Regex pattern = new Regex("[;:?,/]");
        private string pathToFolder;

        public FileSaver(string pathToFolder)
        {
            this.pathToFolder = pathToFolder;
        }
        public void SaveHtmlFile(Uri url, string html)
        {
            var replace = pattern.Replace(url.AbsoluteUri, "_");
            var newPath = Path.Combine(pathToFolder, replace);

            try
            {
                File.WriteAllText(newPath, html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}