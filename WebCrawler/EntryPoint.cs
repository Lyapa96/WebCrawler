using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace WebCrawler
{
    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(Run);
        }

        private static void Run(Options opts)
        {
            var startUri = new Uri(opts.Uri);
            var pathToFolder = opts.Path ?? GetPathToFolderWithResults();
            var maxPagesCount = opts.MaxPagesCount;
            var maxDepth = opts.MaxDepth;

            var fileSaver = new FileSaver(pathToFolder);
            var uriExtractor = new UriExtractor();
            var queue = new Queue<Uri>();
            queue.Enqueue(startUri);
            var hashSet = new HashSet<Uri> {startUri};

            var currentDepth = 0;
            var currentPageCount = 0;

            while (queue.Count != 0
                   && currentDepth < maxDepth
                   && currentPageCount < maxPagesCount)
            {
                var tasks = GetAllUriListFromQueue(queue, maxPagesCount - currentPageCount)
                    .Select(url => GetUriListFromPage(url, fileSaver, uriExtractor, hashSet))
                    .ToArray();

                currentDepth++;
                currentPageCount += tasks.Length;

                Task.WaitAll(tasks);

                foreach (var uri in tasks.SelectMany(task => task.Result))
                    if (!hashSet.Contains(uri))
                    {
                        hashSet.Add(uri);
                        queue.Enqueue(uri);
                    }
            }

            PrintStats(hashSet, currentPageCount, queue, currentDepth);
        }

        private static void PrintStats(HashSet<Uri> hashSet, int currentPageCount, Queue<Uri> queue, int currentDepth)
        {
            Console.WriteLine();
            Console.WriteLine("==================================");
            Console.WriteLine($"Текущий уровень вложенности: {currentDepth}");
            Console.WriteLine($"Текущий размер очереди: {queue.Count}");
            Console.WriteLine($"Всего найдено ссылок: {hashSet.Count}");
            Console.WriteLine($"Всего сохранено ссылок: {currentPageCount}");
        }

        private static string GetPathToFolderWithResults()
        {
            const string resultsFolder = "Results";

            var pathToFolderWithResults = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resultsFolder);

            if (Directory.Exists(pathToFolderWithResults))
                Directory.Delete(pathToFolderWithResults, true);
            Directory.CreateDirectory(pathToFolderWithResults);

            return pathToFolderWithResults;
        }

        private static IEnumerable<Uri> GetAllUriListFromQueue(Queue<Uri> queue, int batchSize)
        {
            while (queue.Count != 0 && batchSize != 0)
            {
                batchSize--;
                yield return queue.Dequeue();
            }
        }

        private static Task<Uri[]> GetUriListFromPage(Uri baseUrl,
            FileSaver fileSaver,
            UriExtractor uriExtractor,
            HashSet<Uri> hashSet)
        {
            return new WebClient()
                .DownloadStringTaskAsync(baseUrl)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                        return new Uri[0];

                    Console.WriteLine(
                        $"Thread: {Thread.CurrentThread.ManagedThreadId} downloaded page: {baseUrl.AbsoluteUri}");

                    var html = task.Result;

                    fileSaver.SaveHtmlFile(baseUrl, html);

                    return uriExtractor
                        .GetUriList(html, baseUrl)
                        .Where(x => !hashSet.Contains(x))
                        .ToArray();
                });
        }
    }
}