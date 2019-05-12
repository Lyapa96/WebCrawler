using CommandLine;

namespace WebCrawler
{
    public class Options
    {
        [Option('u', "uri", Required = true, HelpText = "Uri Web 1.0")]
        public string Uri { get; set; }

        [Option('d', "depth", Required = false, Default = 5, HelpText = "Max depth")]
        public int MaxDepth { get; set; }

        [Option('c', "count", Required = false, Default = 5000, HelpText = "Max pages count")]
        public int MaxPagesCount { get; set; }

        [Option('p', "path", Required = false, HelpText = "Path to folder with pages")]
        public string Path { get; set; }
    }
}