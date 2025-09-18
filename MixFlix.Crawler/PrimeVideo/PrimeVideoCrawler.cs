using Microsoft.Playwright;
using StreamBuddy.Crawler.DisneyPlus;
using StreamBuddy.Data;
using System.Text.Json;

namespace StreamBuddy.Crawler.PrimeVideo
{
    public class PrimeVideoCrawler
    {
        private readonly string _baseUrl = "https://www.primevideo.com";
        private readonly IBrowserContext _context;
        private bool _landingPagesLoaded = false;
        private bool _categoryPagesLoaded = false;
        private Queue<string> _categoryPages = new();

        public PrimeVideoCrawler(IBrowserContext context)
        {
            _context = context;
            _context.RequestFinished += RequestFinished;
        }

        private async void RequestFinished(object? sender, IRequest e)
        {
            if (e.Url.StartsWith($"{_baseUrl}/region/eu/tv") || e.Url.StartsWith($"{_baseUrl}/region/eu/movie") || e.Url.StartsWith($"{_baseUrl}/region/eu/browse") || e.Url.StartsWith($"{_baseUrl}/region/eu/genre"))
            {
                var response = await e.ResponseAsync();
                var contentType = await response.HeaderValueAsync("Content-Type");
                if(contentType == null || !contentType.Contains("application/json"))
                {
                    var links = e.Frame.QuerySelectorAllAsync(@"a[data-testid=""see-more""]");
                    foreach (var link in links.Result)
                    {
                        var href = await link.GetAttributeAsync("href");
                        if (href != null && !string.IsNullOrEmpty(href))
                        {
                            _categoryPages.Enqueue($"{_baseUrl}{href}");
                        }
                    }
                    return; // Not a JSON response, skip processing
                }

                var url = new Uri(e.Url);
                var path = url.PathAndQuery.Split('?')[0];
                var filename = $"primevideo_{path.TrimStart('/').Replace('/', '_')}.json";

                try
                {
                    
                    var json = await response.TextAsync();
                    File.WriteAllText(filename, json);

                    var browse = JsonSerializer.Deserialize<BrowseResponse>(json);

                    if(browse?.Page != null)
                    {
                        foreach (var page in browse.Page)
                        {
                            if (page.Assembly?.Body != null)
                            {
                                foreach (var body in page.Assembly.Body)
                                {
                                    var containers = new List<Container>();
                                    if (body.Props?.LandingPage?.Containers != null) 
                                    {
                                        containers.AddRange(body.Props.LandingPage.Containers);
                                    }
                                    if (body.Props?.Browse?.Containers != null)
                                    {
                                        containers.AddRange(body.Props.Browse.Containers);
                                    }
                                    foreach (var container in containers)
                                    {
                                        if (container.Entities != null)
                                        {
                                            foreach (var entity in container.Entities)
                                            {
                                                if (entity.EntitlementCues?.EntitlementType != "Entitled")
                                                {
                                                    continue;
                                                }

                                                var entityFile = $"primevideo\\{entity.TitleId}.json";
                                                if (!File.Exists(entityFile))
                                                {
                                                    var entityJson = JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true });
                                                    File.WriteAllText(entityFile, entityJson);
                                                }

                                                var stream = new Content
                                                {
                                                    //ServiceId = "primevideo",
                                                    ExternalId = entity.TitleId,
                                                    Type = entity.EntityType == "Movie" ? ContentType.Movie : ContentType.Series,
                                                    Title = entity.Title,
                                                    Description = entity.Synopsis,
                                                    StartYear = entity.ReleaseYear != null ? int.Parse(entity.ReleaseYear) : null,
                                                    ImageUrl = entity.Images?.Cover?.Url
                                                };
                                            }
                                        }

                                        if (container.SeeMore?.Link?.Url != null)
                                        {
                                            var seeMoreUrl = container.SeeMore.Link.Url.TrimStart(['/']);
                                            _categoryPages.Enqueue($"{_baseUrl}/{seeMoreUrl}");
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing request: {ex.Message}");
                }
            }
            if (e.Url.StartsWith($"{_baseUrl}/region/eu/api/getLandingPage"))
            {
                var frame = e.Frame;
                var url = new Uri(frame.Url);
                var path = url.PathAndQuery.Split('?')[0];

                try
                {
                    var response = await e.ResponseAsync();
                    var json = await response.TextAsync();
                    var landingPage = JsonSerializer.Deserialize<LandingPageResponse>(json);
                    var offset = landingPage?.Pagination?.StartIndex ?? 0;
                    var filename = $"primevideo_{path.TrimStart('/').Replace('/', '_')}_paginate_{offset}.json";
                    File.WriteAllText(filename, json);

                    if (landingPage?.Containers != null) 
                    {
                        foreach(var container in landingPage.Containers)
                        {
                            if(container.Entities != null)
                            {
                                foreach (var entity in container.Entities)
                                {
                                    if(entity.EntitlementCues?.EntitlementType != "Entitled")
                                    {
                                        continue;
                                    }

                                    var entityFile = $"primevideo\\{entity.TitleId}.json";
                                    if (!File.Exists(entityFile))
                                    {
                                        var entityJson = JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true });
                                        File.WriteAllText(entityFile, entityJson);
                                    }

                                    var stream = new Content
                                    {
                                        //ServiceId = "primevideo",
                                        ExternalId = entity.TitleId,
                                        Type = entity.EntityType == "Movie" ? ContentType.Movie : ContentType.Series,
                                        Title = entity.Title,
                                        Description = entity.Synopsis,
                                        StartYear = entity.ReleaseYear != null ? int.Parse(entity.ReleaseYear) : null,
                                        ImageUrl = entity.Images?.Cover?.Url
                                    };
                                }
                            }
                            if(container.SeeMore?.Link?.Url != null)
                            {
                                var seeMoreUrl = container.SeeMore.Link.Url.TrimStart(['/']);
                                _categoryPages.Enqueue($"{_baseUrl}/{seeMoreUrl}");
                            }
                        }
                    }
                    if(landingPage?.Pagination == null)
                    {
                        _landingPagesLoaded = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing request: {ex.Message}");
                }
            }
            if (e.Url.StartsWith($"{_baseUrl}/region/eu/api/paginateCollection"))
            {
                var frame = e.Frame;
                var url = new Uri(frame.Url);
                var path = url.PathAndQuery.Split('?')[0];
                
                try
                {
                    var response = await e.ResponseAsync();
                    var json = await response.TextAsync();
                    var paginate = JsonSerializer.Deserialize<PaginateResponse>(json);
                    var offset = paginate?.Pagination?.StartIndex ?? 0;
                    var filename = $"primevideo_{path.TrimStart('/').Replace('/', '_')}_paginate_{offset}.json";
                    File.WriteAllText(filename, json);

                    if (paginate?.Entities != null)
                    {

                        foreach (var entity in paginate.Entities)
                        {
                            if (entity.EntitlementCues?.EntitlementType != "Entitled")
                            {
                                continue;
                            }
                            var stream = new Content
                            {
                                //ServiceId = "primevideo",
                                ExternalId = entity.TitleId,
                                Type = entity.EntityType == "Movie" ? ContentType.Movie : ContentType.Series,
                                Title = entity.Title,
                                Description = entity.Synopsis,
                                StartYear = entity.ReleaseYear != null ? int.Parse(entity.ReleaseYear) : null,
                                ImageUrl = entity.Images?.Cover?.Url
                            };
                        }

                    }
                    if (!(paginate?.HasMoreItems ?? false))
                    {
                        _categoryPagesLoaded = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing request: {ex.Message}");
                }
            }
        }

        public async Task Crawl()
        {
            //await CrawlMovies();
            //await CrawlSeries();
        }
        public async Task<IPage> GetTab()
        {
            var page = _context.Pages.FirstOrDefault(x => x.Url.Contains("primevideo.com"));
            if (page == null)
            {
                page = await _context.NewPageAsync();
            }
            await page.GotoAsync("https://www.primevideo.com/region/eu/storefront");

            return page;
        }
        public async Task DisplayMainPage(string linkTestId)
        {
            var tab = await GetTab();
            await tab.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var link = await tab.QuerySelectorAsync(@$"a[data-testid=""{linkTestId}""]");
            await link.ClickAsync();
            //await tab.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(2000);

            _landingPagesLoaded = false;

            var scrollCount = 0;
            while (!_landingPagesLoaded && scrollCount < 100)
            {
                await Task.Delay(2000); // Wait for the page to load
                await tab.Keyboard.PressAsync("PageDown");
                scrollCount++;
            }

            if(_categoryPages.Count > 0)
            {
                 _categoryPagesLoaded = false;
                while (_categoryPages.Count > 0)
                {
                    var categoryUrl = _categoryPages.Dequeue();

                    await tab.GotoAsync(categoryUrl);
                    await tab.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    scrollCount = 0;
                    while (!_categoryPagesLoaded && scrollCount < 100)
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
            await DisplayMainPage("pv-nav-home-movies");
            
        }
        public async Task CrawlSeries()
        {
            await DisplayMainPage("pv-nav-home-tv-shows");
        }
    }
}
