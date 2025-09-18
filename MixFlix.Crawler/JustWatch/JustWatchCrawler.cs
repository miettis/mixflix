using Microsoft.Playwright;
using MixFlix.Data;
using System.Text.Json;

namespace MixFlix.Crawler.JustWatch
{
    public class JustWatchCrawler
    {
        private readonly string _baseUrl = "https://www.justwatch.com/fi";
        private readonly IBrowserContext _context;
        private bool _isCrawling = false;
        private bool _allLoaded = false;
        private readonly bool _downloadImages;
        private readonly string _language = "fi";
        public JustWatchCrawler(IBrowserContext context, bool downloadImages, string language)
        {
            _context = context;
            _context.RequestFinished += RequestFinished;
            _downloadImages = downloadImages;
            _language = language;
        }

        private async void RequestFinished(object? sender, IRequest e)
        {
            if (e.Url.StartsWith("https://apis.justwatch.com/graphql"))
            {
                var request = e.PostData;
                if(request == null)
                {
                    return;
                }

                var graphqlRequest = JsonSerializer.Deserialize<GraphQLRequest>(request);
                if(graphqlRequest?.OperationName != "GetPopularTitles")
                {
                    return;
                }

                var types = string.Join("-", graphqlRequest.Variables.PopularTitlesFilter.ObjectTypes);
                var providers = string.Join("-",graphqlRequest.Variables.PopularTitlesFilter.Packages);
                var order = graphqlRequest.Variables.PopularTitlesSortBy.ToLower();
                var after = graphqlRequest.Variables.After;
                var filename = $"justwatch\\graphql_{_language}\\popular_titles_{types}_{providers}_{order}_{after}.json";

                var response = await e.ResponseAsync();
                var json = await response.TextAsync();

                File.WriteAllText(filename, json);

                var titles = JsonSerializer.Deserialize<GetPopularTitlesResponse>(json);

                if(titles?.Data?.PopularTitles?.Edges == null)
                {
                    return;
                }
                foreach(var edge in titles.Data.PopularTitles.Edges)
                {

                }

                if (!(titles?.Data?.PopularTitles?.PageInfo?.HasNextPage ?? false))
                {
                    _allLoaded = true;
                }
            }
            if (_downloadImages && e.Url.StartsWith("https://images.justwatch.com/poster/"))
            {
                var path = e.Url.Replace("https://images.justwatch.com/poster/", string.Empty);
                var filename = $"justwatch\\posters\\{path.Replace("/", "_")}.jpg";
                if(File.Exists(filename))
                {
                    return; // Skip if already exists
                }
                try
                {
                    var response = await e.ResponseAsync();
                    var buffer = await response.BodyAsync();
                    File.WriteAllBytes(filename, buffer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving image {filename}: {ex.Message}");
                }
            }
        }


        public async Task<IPage> GetTab()
        {
            var page = _context.Pages.FirstOrDefault(x => x.Url.Contains(_baseUrl));
            if (page == null)
            {
                page = await _context.NewPageAsync();
                await page.GotoAsync("https://www.disneyplus.com/fi-fi/home");
            }

            return page;
        }
        public async Task Crawl(string providerId, ContentType contentType)
        {
            var url = $"{_baseUrl}?monetization_types=flatrate&providers={providerId}";

            var tab = await GetTab();
            await tab.GotoAsync(url);
            await tab.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            var linkSuffix = contentType switch
            {
                ContentType.Movie => "/elokuvat",
                ContentType.Series => "/sarjat",
                _ => ""
            };
            var filterBar = await tab.QuerySelectorAsync(".filter-bar");
            //var filterBar = await tab.QuerySelectorAsync(".row-filter-bar");
            var link = await filterBar.QuerySelectorAsync(@$"a[href$=""{linkSuffix}""]");
            
            if(link == null)
            {
                return;
            }

            _allLoaded = false;

            await link.ClickAsync();
            await Task.Delay(10000);

            
            var count = 0;

            while (!_allLoaded && count < 100)
            {
                await Task.Delay(10000);
                await tab.Keyboard.PressAsync("PageDown");

                count++;
            }
        }
    }
}
