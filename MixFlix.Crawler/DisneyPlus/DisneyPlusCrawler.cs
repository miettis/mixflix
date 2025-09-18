using Microsoft.Playwright;
using StreamBuddy.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StreamBuddy.Crawler.DisneyPlus
{
    public class DisneyPlusCrawler
    {
        private readonly IBrowserContext _context;
        private bool _isCrawling = false;
        private bool _allLoaded = false;
        public DisneyPlusCrawler(IBrowserContext context)
        {
            _context = context;
            _context.RequestFinished += RequestFinished;
        }

        private async void RequestFinished(object? sender, IRequest e)
        {
            if (e.Url.StartsWith("https://disney.api.edge.bamgrid.com/explore") && e.Url.Contains("/set/"))
            {
                var url = new Uri(e.Url);
                var query = System.Web.HttpUtility.ParseQueryString(url.Query);
                var offset = query["offset"];

                var filename = $"disneyplus";
                var frame = e.Frame;
                if (frame.Url.Contains("/movies"))
                {
                    filename += "_movies" + (offset != null ? $"_{offset}" : string.Empty);
                }
                else if(frame.Url.Contains("/series"))
                {
                    filename += "_series" + (offset != null ? $"_{offset}" : string.Empty);
                }

                filename += ".json";


                //  https://disney.api.edge.bamgrid.com/explore/v1.10/set/e20ce99f-838b-4cb8-9b50-46b035f72b87?layoutId=2eb4e790-5ac9-4781-b281-de9b31eb6716&limit=48&offset=0&pageId=c44952c4-c788-44c3-bdf7-e99fca172f36&pageResolutionId=3b19e9cc-bba0-4e08-b2b5-e293992b2531&pageStyle=standard_emphasis_with_navigation&setResolutionId=943728cc-48b1-451c-ae31-509469049a60&setStyle=standard_art_dense&skipEligibilityCheck=false

                try
                {
                    var response = await e.ResponseAsync();
                    var json = await response.TextAsync();
                    File.WriteAllText(filename, json);

                    var explore = JsonSerializer.Deserialize<ExploreResponse>(json);

                    var streams = new List<Content>();

                    if (explore?.Data?.Set?.Items != null)
                    {
                        foreach (var item in explore.Data.Set.Items)
                        {
                            var stream = new Content
                            {
                                //ServiceId = "disneyplus",
                                ExternalId = item.Id,
                                Title = item.Visuals?.Title,
                                Description = item.Visuals?.Description?.Full,
                                StartYear = item.Visuals?.MetastringParts?.ReleaseYearRange?.StartYear != null ? int.Parse(item.Visuals?.MetastringParts?.ReleaseYearRange?.StartYear) : null,
                                //EndYear = item.Visuals.MetastringParts.ReleaseYearRange.e,
                            };
                            if (item.Visuals?.MetastringParts?.Runtime?.RuntimeMs != null)
                            {
                                stream.Runtime = (int)item.Visuals.MetastringParts.Runtime.RuntimeMs.Value / (60 * 1000);
                            }

                            streams.Add(stream);

                        }
                    }

                    if (explore?.Data?.Set?.Pagination?.HasMore ?? false)
                    {
                        _allLoaded = false;
                    }
                    else
                    {
                        _allLoaded = true;
                    }

                    ;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing request: {ex.Message}");
                    ;
                }

            }
        }

        public async Task Crawl()
        {
            await CrawlMovies();
            await CrawlSeries();
        }
        public async Task<IPage> GetTab()
        {
            var page = _context.Pages.FirstOrDefault(x => x.Url.Contains("disneyplus.com"));
            if (page == null)
            {
                page = await _context.NewPageAsync();
            }
            await page.GotoAsync("https://www.disneyplus.com/fi-fi/home");

            return page;
        }
        public async Task DisplayAll(string url)
        {
            var tab = await GetTab();
            await tab.GotoAsync(url);
            await tab.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var tabMenu = await tab.QuerySelectorAsync(@"div[data-testid=""tab-menu""]");
            if (tabMenu != null)
            {
                var allButton = await tabMenu.QuerySelectorAsync(@"button[aria-label^=""Kaikki""]");
                if (allButton != null)
                {
                    _allLoaded = false;
                    await allButton.ClickAsync();

                    var scrollCount = 0;
                    while (!_allLoaded && scrollCount < 100)
                    {
                        await Task.Delay(2000); // Wait for the page to load
                        await tab.Keyboard.PressAsync("PageDown");
                        scrollCount++;
                    }
                }
            }
        }
        public async Task CrawlMovies()
        {
            var url = "https://www.disneyplus.com/browse/movies";
            await DisplayAll(url);
            
        }
        public async Task CrawlSeries()
        {
            var url = "https://www.disneyplus.com/browse/series";
            await DisplayAll(url);

        }
    }
}
