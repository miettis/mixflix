using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using MixFlix.Crawler;
using MixFlix.Crawler.JustWatch;
using MixFlix.Crawler.TMDB;
using MixFlix.Data;
using System.Text.Json;


//await FetchMissingImages();

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .Build();

string devConnection = config.GetConnectionString("Development");
string qaConnection = config.GetConnectionString("QA");
string prodConnection = config.GetConnectionString("Production");

var syncSettings = config.GetSection("Sync").Get<DataSyncSettings>();
syncSettings.SourceConnection = devConnection;
syncSettings.TargetConnection = qaConnection;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});
var logger = loggerFactory.CreateLogger<DataSync>();

var syncer = new DataSync(syncSettings, logger);
await syncer.TestConnections();
//await syncer.SyncServices();
//await syncer.SyncCategories();
//await syncer.SyncContents();
//await syncer.SyncContentAvailabilities();
syncer.SyncImages();
;


//using var playwright = await Playwright.CreateAsync();
//await using var browser = await playwright.Chromium.ConnectOverCDPAsync("http://localhost:9222");
//var context = browser.Contexts[0];
//var crawler = new JustWatchCrawler(context, true, "en");

var providers = new[] 
{
    //"dnp",
    //"nfx",
    //"prv",
    //"sst",
    "mxx",
    //"hbo-max",
    //"atp",
    //"tlp",
    //"bbo",
    //"mtk"
};

//await crawler.Crawl("mxx", ContentType.Series);

//foreach (var provider in providers)
//{
//    await crawler.Crawl(provider, ContentType.Movie);
//    await crawler.Crawl(provider, ContentType.Series);
//}



//var crawler = new DisneyPlusCrawler(context);
//await crawler.Crawl();

//var crawler = new PrimeVideoCrawler(context);
//await crawler.Crawl();

//Console.ReadLine();


//ImportDisneyPlus();
//ImportPrimeVideo();

//InsertTitles();

//UpdateTitlesEn();

//PopulateTmdbMeta();

//await CrawlTmdb();

//PopulateTmdbContent();
//InsertJustWatchContent();
//PopulateTmdbAvailabilities();
//CompareJustWatchTmdb();
//await PopulateCast();
//FixAvailabilities();

//await DownloadTmdbPosters();

/*
void ImportDisneyPlus()
{
    foreach (var file in Directory.GetFiles("disneyplus"))
    {
        var context = ContextFactory.CreateDbContextPostgreSQL();

        var service = context.Services.FirstOrDefault(x => x.Name == "Disney Plus");

        if(service == null)
        {
            service = new Service
            {
                Name = "Disney Plus"
            };
            context.Services.Add(service);
            context.SaveChanges();
        }

        var existingIds = context.Contents
            .Where(x => x.Service == service)
            .Select(x => x.ExternalId)
            .ToHashSet();

        var categories = context.Categories
            .Where(x => x.Service == service)
            .ToDictionary(x => x.Name, x => x);

        var json = File.ReadAllText(file);
        var explore = JsonSerializer.Deserialize<ExploreResponse>(json);

        foreach(var item in explore.Data.Set.Items)
        {
            if (existingIds.Contains(item.Id))
            {
                continue; // Skip if already exists
            }

            var content = new Content
            {
                ServiceId = service.Id,
                ExternalId = item.Id,
                Type = file.Contains("movie") ? ContentType.Movie : file.Contains("series") ? ContentType.Series : ContentType.None,
                Title = item.Visuals.Title,
                Description = item.Visuals.Description?.Full ?? item.Visuals.Description?.Medium ?? item.Visuals.Description?.Brief,
                StartYear = item.Visuals.MetastringParts?.ReleaseYearRange?.StartYear != null ? int.Parse(item.Visuals.MetastringParts.ReleaseYearRange.StartYear) : null,
                //EndYear = item.Visuals.MetastringParts.ReleaseYearRange.
                // https://disney.images.edge.bamgrid.com/ripcut-delivery/v2/variant/disney/b8e61408-1423-4d06-81f4-858f3b509a2e/compose?format=webp&label=standard_art_178&width=800
                ImageUrl = item.Visuals.Artwork.Tile?.Background?.The178?.ImageId,
                RawData = JsonSerializer.Serialize(item),
                Categories = new List<Category>()
            };

            foreach (var categoryName in item.Visuals.MetastringParts?.Genres?.Values ?? Enumerable.Empty<string>())
            {
                if (!categories.TryGetValue(categoryName, out var category))
                {
                    category = new Category
                    {
                        Service = service,
                        ExternalId = categoryName,
                        Name = categoryName
                    };
                    context.Categories.Add(category);
                    categories[categoryName] = category;
                }

                content.Categories.Add(category);
            }

            context.Contents.Add(content);


        }

        context.SaveChanges();
    }
}
void ImportPrimeVideo()
{
    foreach (var file in Directory.GetFiles("primevideo"))
    {
        var context = ContextFactory.CreateDbContextPostgreSQL();
        var service = context.Services.FirstOrDefault(x => x.Name == "Prime Video");
        if (service == null)
        {
            service = new Service
            {
                Name = "Prime Video"
            };
            context.Services.Add(service);
            context.SaveChanges();
        }
        var existingIds = context.Contents
            .Where(x => x.Service == service)
            .Select(x => x.ExternalId)
            .ToHashSet();

        var json = File.ReadAllText(file);

        var entities = new List<MixFlix.Crawler.PrimeVideo.Entity>();
        if (json.Contains("atv.wps#GetLandingPageOutput"))
        {
            var landingPage = JsonSerializer.Deserialize<LandingPageResponse>(json);
            entities = landingPage.Containers.SelectMany(x => x.Entities).ToList();
        }
        else if (json.Contains("atv.wps#PaginateCollectionOutput"))
        {
            var paginate = JsonSerializer.Deserialize<PaginateResponse>(json);
            entities = paginate.Entities.ToList();
        }
        else
        {
            var browse = JsonSerializer.Deserialize<BrowseResponse>(json);
            entities.AddRange(browse.Page.SelectMany(x => x.Assembly.Body.Where(b => b.Props.LandingPage?.Containers != null).SelectMany(a => a.Props.LandingPage.Containers.SelectMany(x => x.Entities))));
            entities.AddRange(browse.Page.SelectMany(x => x.Assembly.Body.Where(b => b.Props.Browse?.Containers != null).SelectMany(a => a.Props.Browse.Containers.SelectMany(x => x.Entities))));
        }

        foreach (var entity in entities)
        {
            if (existingIds.Contains(entity.TitleId))
            {
                continue; // Skip if already exists
            }
            if(entity.EntitlementCues?.EntitlementType != "Entitled")
            {
                continue; // Skip if not entitled
            }
            var content = new Content
            {
                ServiceId = service.Id,
                ExternalId = entity.TitleId,
                Type = entity.EntityType switch
                {
                    "Movie" => ContentType.Movie,
                    "TV Show" => ContentType.Series,
                    _ => ContentType.None
                },
                Title = entity.Title,
                Description = entity.Synopsis,
                StartYear = entity.ReleaseYear != null ? int.Parse(entity.ReleaseYear) : null,
                //EndYear = item.Visuals.MetastringParts.ReleaseYearRange.
                ImageUrl = entity.Images?.Cover?.Url,
                RawData = JsonSerializer.Serialize(entity)
            };

            context.Contents.Add(content);
        }

        context.SaveChanges();
    }
    
}

*/

void InsertServices()
{
    var services = new Dictionary<string, string>
    {
        { "dnp", "Disney Plus" },
        { "nfx", "Netflix"     },
        { "prv", "Prime Video" },
        { "sst", "SkyShowtime" },
        { "mxx", "HBO Max"     },
        { "atp", "Apple TV+"   },
        { "tlp", "Telia Play"  },
        { "bbo", "BritBox"     },
        { "mtk", "MTV Katsomo" },
    };
    var context = ContextFactory.CreateDbContextPostgreSQL();

    foreach(var service in services)
    {
        var newService = new Service
        {
            JustWatchId = service.Key,
            Name = service.Value
        };
        context.Services.Add(newService);
    }

    context.SaveChanges();
}
void InsertCategories()
{
    var categories = new Dictionary<string, string>();

    foreach(var file in Directory.GetFiles("justwatch\\graphql"))
    {
        var json = File.ReadAllText(file);
        var response = JsonSerializer.Deserialize<GetPopularTitlesResponse>(json);
        
        foreach(var edge in response?.Data?.PopularTitles?.Edges ?? Enumerable.Empty<Edge>())
        {
            if(edge.Node?.Content?.Genres == null)
            {
                continue;
            }
            foreach(var genre in edge.Node.Content.Genres)
            {
                if(!categories.ContainsKey(genre.ShortName))
                {
                    categories[genre.ShortName] = genre.Translation;
                }
            }
        }
    }

    var context = ContextFactory.CreateDbContextPostgreSQL();
    foreach (var category in categories)
    {
        var newCategory = new Category
        {
            JustWatchId = category.Key,
            Name = category.Value
        };
        context.Categories.Add(newCategory);
    }

    context.SaveChanges();
}

void InsertJustWatchContent()
{
    foreach (var file in Directory.GetFiles("justwatch\\graphql_fi"))
    {
        var json = File.ReadAllText(file);
        var response = JsonSerializer.Deserialize<GetPopularTitlesResponse>(json);

        var context = ContextFactory.CreateDbContextPostgreSQL();
        var externalIds = context.Contents
            .Select(x => x.JustWatchId)
            .ToHashSet();

        var categories = context.Categories
            .ToDictionary(x => x.JustWatchId, x => x);

        var services = context.Services
            .ToDictionary(x => x.JustWatchId, x => x);

        var filename = Path.GetFileName(file);
        var parts = filename.Split('_');
        var serviceId = parts[3];
        var service = services.GetValueOrDefault(serviceId);

        foreach (var edge in response?.Data?.PopularTitles?.Edges ?? Enumerable.Empty<Edge>())
        {
            if (externalIds.Contains(edge.Node.Id))
            {
                var existingContent = context.Contents.Include(x => x.Availabilities).FirstOrDefault(x => x.JustWatchId == edge.Node.Id);
                var existingAvailability = existingContent?.Availabilities?.FirstOrDefault(x => x.ServiceId == service.Id);
                if (existingAvailability != null)
                {
                    existingAvailability.JustWatchLastSeen = DateTime.UtcNow;
                    existingAvailability.JustWatchRanking = int.Parse(Helpers.Base64Decode(edge.Cursor));

                    context.ContentAvailabilities.Update(existingAvailability);
                    continue; // Skip if already exists for this service
                }
                else
                {

                    var availability = new ContentAvailability
                    {
                        ServiceId = service.Id,
                        ContentId = existingContent.Id,
                        JustWatchLastSeen = DateTime.UtcNow,
                        JustWatchRanking = int.Parse(Helpers.Base64Decode(edge.Cursor))
                    };

                    context.ContentAvailabilities.Add(availability);
                }
            }
            else
            {
                if (edge.Node.Content == null)
                {
                    continue; // Skip if content is null
                }

                var content = new MixFlix.Data.Content
                {
                    JustWatchId = edge.Node.Id,
                    Title = edge.Node.Content.Title,
                    ShortDescription = edge.Node.Content.ShortDescription,
                    ReleaseYear = edge.Node.Content.OriginalReleaseYear,
                    Type = edge.Node.ObjectType switch
                    {
                        "MOVIE" => ContentType.Movie,
                        "SHOW" => ContentType.Series,
                        _ => ContentType.None
                    },
                    ImageUrl = edge.Node.Content.PosterUrl,
                    //RawData = JsonSerializer.Serialize(edge.Node.Content)
                };
                /*
                foreach (var genre in edge.Node.Content.Genres ?? Enumerable.Empty<Genre>())
                {
                    if (categories.TryGetValue(genre.ShortName, out var category))
                    {
                        content.Categories ??= new List<Category>();
                        content.Categories.Add(category);
                    }
                }
                */

                content.Availabilities ??= new List<ContentAvailability>();
                content.Availabilities.Add(new ContentAvailability
                {
                    ServiceId = service.Id,
                    Content = content,
                    JustWatchLastSeen = DateTime.UtcNow,
                    JustWatchRanking = int.Parse(Helpers.Base64Decode(edge.Cursor))
                });

                context.Contents.Add(content);

                Console.WriteLine($"Added content: {content.Title} ({content.JustWatchId}) - {content.Type}");
            }
            
        }

        context.SaveChanges();
    }
}

void UpdateTitles()
{
    var context = ContextFactory.CreateDbContextPostgreSQL();

    foreach (var file in Directory.GetFiles("justwatch\\graphql"))
    {
        var nameParts = Path.GetFileName(file).Split(['_','.']);
        var offsetStr = Helpers.Base64Decode(nameParts[nameParts.Length - 2]);
        var fileOffset = string.IsNullOrWhiteSpace(offsetStr) ? 0 : int.Parse(offsetStr);



        //context.SaveChanges();
    }
}

async Task FetchAlternativeImages()
{
    var directory = "C:\\code\\github\\miettis\\mixflix\\MixFlix\\MixFlix.Crawler\\bin\\Debug\\net8.0\\justwatch\\posters";
    using var httpClient = new HttpClient();

    foreach (var filePath in Directory.GetFiles(directory))
    {
        var filename = Path.GetFileName(filePath).Replace(".avif.jpg", ".avif");
        if (filename.Contains("_s166_"))
        {
            filename = filename.Replace("_s166_", "_s332_");
        }
        else if (filename.Contains("_s332_"))
        {
            filename = filename.Replace("_s332_", "_s166_");

        }

        // https://images.justwatch.com/poster/330035077/s166/and-just-like-that.avif
        var url = $"https://images.justwatch.com/poster/{filename.Replace('_', '/')}" ;

        var targetFile = Path.Combine(directory, filename);

        if (File.Exists(targetFile))
        {
            Console.WriteLine($"File already exists: {targetFile}");
            continue; // Skip if file already exists
        }
        try
        {
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(targetFile, bytes);
                Console.WriteLine($"Downloaded: {targetFile}");
            }
            else
            {
                Console.WriteLine($"Failed to download {url}: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading {url}: {ex.Message}");
        }

        await Task.Delay(1000); // Delay to avoid hitting the server too hard
    }
    var context = ContextFactory.CreateDbContextPostgreSQL();
    var contents = context.Contents
        .Where(x => x.ImageUrl != null)
        .Select(x => new 
        {
            x.Id,
            x.ImageUrl
        })
        .ToList();

    foreach (var content in contents)
    {
        
    }
}
async Task FetchMissingImages()
{
    var directory = "C:\\code\\github\\miettis\\mixflix\\MixFlix\\MixFlix.Crawler\\bin\\Debug\\net8.0\\justwatch\\posters";
    using var httpClient = new HttpClient();

    var context = ContextFactory.CreateDbContextPostgreSQL();
    var contents = context.Contents
        .Where(x => x.ImageUrl != null)
        .Select(x => new 
        {
            x.Id,
            x.ImageUrl
        })
        .ToList();

    
    foreach (var content in contents.Where(x => x.ImageUrl.StartsWith("/poster")))
    {
        if(File.Exists(Path.Combine(directory, $"{content.Id}_md.avif")))
        {
            continue; // Skip if small image already exists
        }
        

        var targetFile = Path.Combine(directory, $"{content.Id}_md.avif");
        if (File.Exists(targetFile))
        {
            continue;
        }
        try
        {
            var filename = content.ImageUrl.Replace("/poster/", "").Replace("{profile}", "s332").Replace("{format}", "avif");
            var url = $"https://images.justwatch.com/poster/{filename}";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(targetFile, bytes);
                Console.WriteLine($"Downloaded: {targetFile}");
            }
            else
            {
                Console.WriteLine($"Failed to download {content.ImageUrl}: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading {content.ImageUrl}: {ex.Message}");
        }
        await Task.Delay(1000); // Delay to avoid hitting the server too hard

    }
    
}

void UploadImages()
{
    var host = "";
    var localDir = "C:\\code\\github\\miettis\\mixflix\\MixFlix\\MixFlix.Crawler\\bin\\Debug\\net8.0\\justwatch\\posters";
    var remoteDir = "/var/lib/docker/volumes/mixflix/_data";
    var username = "";
    var password = "";

    //Uploading File
    using var client = new ScpClient(host, username, password);

    client.Connect();

    using var stream = new MemoryStream();
    client.Download(remoteDir + "/log.txt", stream);

    stream.Position = 0; // Reset stream position to the beginning
    using var reader = new StreamReader(stream);
    var text = reader.ReadToEnd();

    var timestamp = DateTimeOffset.Parse(text);
    timestamp = timestamp.AddDays(-11);


    foreach (var file in new DirectoryInfo(localDir).GetFiles())
    {
        // if file created less than 1 hour ago, skip
        if (file.LastWriteTimeUtc < timestamp)
        {
            continue;
        }
        var filename = file.Name;
        client.Upload(file, remoteDir + "/" + filename);
        Console.WriteLine($"Uploaded: {filename}");
    }

    timestamp = DateTimeOffset.UtcNow;
    using var uploadStream = new MemoryStream();
    using var writer = new StreamWriter(uploadStream);
    writer.WriteLine(timestamp.ToString("o"));
    writer.Flush();
    uploadStream.Position = 0; // Reset stream position to the beginning
    client.Upload(uploadStream, remoteDir + "/log.txt");
}

void FixFilenames()
{
    var directory = "C:\\code\\github\\miettis\\mixflix\\MixFlix\\MixFlix.Crawler\\bin\\Debug\\net8.0\\justwatch\\posters";
    var context = ContextFactory.CreateDbContextPostgreSQL();
    var contents = context.Contents.ToList();

    foreach (var content in contents)
    {
        if (string.IsNullOrEmpty(content.ImageUrl))
        {
            continue; // Skip if no image URL
        }

        var filenameMedium = content.ImageUrl.Replace("/poster/", "").Replace("{profile}", "s166").Replace("{format}", "avif").Replace("/", "_");
        var filePath = Path.Combine(directory, filenameMedium);
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            continue; // Skip if file does not exist
        }

        var newFilename = $"{content.Id}_sm.avif";

        var newFilePath = Path.Combine(directory, newFilename);
        File.Move(filePath, newFilePath);
    }

}


void PopulateTmdbContent()
{
    var dir = "tmdb";

    foreach (var file in Directory.GetFiles(dir))
    {
        var type = file.Contains("movie") ? ContentType.Movie : file.Contains("tv") ? ContentType.Series : ContentType.None;

        var json = File.ReadAllText(file);
        var response = JsonSerializer.Deserialize<DiscoverResponse>(json);
        if (response?.Results == null || response.Results.Length == 0)
        {
            Console.WriteLine($"No results found in {file}");
            continue;
        }

        var tmdbIds = response.Results
            .Where(x => x.Id != null && x.Id > 0)
            .Select(x => x.Id.Value.ToString())
            .ToHashSet();

        var context = ContextFactory.CreateDbContextPostgreSQL();
        var contentItems = context.Contents
           .Where(x => tmdbIds.Contains(x.TmdbId))
           .ToList();
        var categories = context.Categories.Where(x => !string.IsNullOrWhiteSpace(x.TmdbId)).ToList();
        var categoryMap = new Dictionary<string, Category>();
        foreach(var category in categories)
        {
            foreach(var tmdbId in category.TmdbId.Split(','))
            {
                if (categoryMap.ContainsKey(tmdbId))
                {
                    continue; // Skip if already exists
                }
                categoryMap[tmdbId] = category;
            }
        }

        foreach (var item in response.Results)
        {
            if (item.Id == null || item.Id < 1)
            {
                continue; // Skip if no ID
            }
            var tmdbId = item.Id.Value.ToString();
            var content = contentItems.FirstOrDefault(x => x.TmdbId == tmdbId);
            if (content == null)
            {
                content = new MixFlix.Data.Content
                {
                    Type = type,
                    Title = type == ContentType.Movie ? item.Title : item.Name,
                    TitleEn = type == ContentType.Movie ? item.OriginalTitle : item.OriginalName,
                    TmdbId = tmdbId,
                    ImageUrl= item.PosterPath,
                    Language = item.OriginalLanguage,
                    TmdbPopularity = item.Popularity,
                    TmdbScore = item.VoteAverage,
                    ReleaseYear = type == ContentType.Movie ? item.ReleaseDate?.Year : item.FirstAirDate?.Year,
                    ShortDescription = item.Overview,
                    Categories = new List<Category>()
                };

                foreach(var genre in item.GenreIds)
                {
                    if (categoryMap.TryGetValue(genre.ToString(), out var category))
                    {
                        content.Categories.Add(category);
                    }
                }

                context.Contents.Add(content);
            }
            else
            {
                content.Language = item.OriginalLanguage;
                context.Contents.Update(content);
            }
        }

        context.SaveChanges();
    }

}

void PopulateTmdbAvailabilities()
{
    var dir = "tmdb";

    foreach (var file in Directory.GetFiles(dir))
    {
        var nameParts = Path.GetFileName(file).Split('_');
        var serviceId = nameParts[1];

        var json = File.ReadAllText(file);
        var response = JsonSerializer.Deserialize<DiscoverResponse>(json);
        if (response?.Results == null || response.Results.Length == 0)
        {
            Console.WriteLine($"No results found in {file}");
            continue;
        }

        var tmdbIds = response.Results
            .Where(x => x.Id != null && x.Id > 0)
            .Select(x => x.Id.Value.ToString())
            .ToHashSet();

        var context = ContextFactory.CreateDbContextPostgreSQL();
        var contentItems = context.Contents
           .Where(x => tmdbIds.Contains(x.TmdbId))
           .Include(x => x.Availabilities)
           .ToList();

        var services = context.Services
            .Where(x => x.TmdbId != null)
            .ToList();

        var service = services.FirstOrDefault(x => x.TmdbId.Split(',').Contains(serviceId));
        if(service == null)
        {
            Console.WriteLine($"Service with ID {serviceId} not found.");
            continue; // Skip if service not found
        }

        var ranking = ((int)response.Page - 1) * Math.Max(20, response.Results.Length);

        foreach (var item in response.Results)
        {
            ranking++;

            if (item.Id == null || item.Id < 1)
            {
                continue; // Skip if no ID
            }
            var tmdbId = item.Id.Value.ToString();
            var existingContent = contentItems.FirstOrDefault(x => x.TmdbId == tmdbId);
            if (existingContent == null)
            {
                continue; // Skip if content not found
            }

            var availability = existingContent.Availabilities?.FirstOrDefault(x => x.ServiceId == service.Id);
            if(availability == null)
            {
                availability = new ContentAvailability
                {
                    ServiceId = service.Id,
                    ContentId = existingContent.Id,
                    TmdbLastSeen = DateTime.UtcNow,
                    TmdbRanking = ranking
                };

                context.ContentAvailabilities.Add(availability);
            }
            else
            {
                availability.TmdbLastSeen = DateTime.UtcNow;
                availability.TmdbRanking = ranking;

                context.ContentAvailabilities.Update(availability);
            }

            context.Update(existingContent);
        }

        context.SaveChanges();
    }

}

void FixAvailabilities()
{
    var context = ContextFactory.CreateDbContextPostgreSQL();
    var contents = context.Contents
        .Include(x => x.Availabilities)
        .Where(x => x.Availabilities != null && x.Availabilities.Count > 1)
        .ToList();

    foreach(var content in contents)
    {
        // Keep the most recent availability and remove others
        var mostRecent = content.Availabilities.First();
        mostRecent.JustWatchRanking = content.Availabilities.Min(x => x.JustWatchRanking);
        mostRecent.JustWatchLastSeen = content.Availabilities.Max(x => x.JustWatchLastSeen);
        mostRecent.TmdbRanking = content.Availabilities.Min(x => x.TmdbRanking);
        mostRecent.TmdbLastSeen = content.Availabilities.Max(x => x.TmdbLastSeen);
        content.Availabilities.RemoveAll(x => x.Id != mostRecent.Id);
        context.Contents.Update(content);
    }

    context.SaveChanges();

    ;
}

void UpdateTitlesEn()
{
    foreach (var file in Directory.GetFiles("justwatch\\graphql_en"))
    {
        var json = File.ReadAllText(file);
        var response = JsonSerializer.Deserialize<GetPopularTitlesResponse>(json);

        var context = ContextFactory.CreateDbContextPostgreSQL();
        var externalIds = context.Contents
            .Where(x => x.TitleEn == null)
            .Select(x => x.JustWatchId)
            .ToHashSet();

        var categories = context.Categories
            .ToDictionary(x => x.JustWatchId, x => x);

        var services = context.Services
            .ToDictionary(x => x.JustWatchId, x => x);

        var filename = Path.GetFileName(file);
        var parts = filename.Split('_');
        var serviceId = parts[3];
        var service = services.GetValueOrDefault(serviceId);

        foreach (var edge in response?.Data?.PopularTitles?.Edges ?? Enumerable.Empty<Edge>())
        {
            if (externalIds.Contains(edge.Node.Id))
            {
                var existingContent = context.Contents.First(x => x.JustWatchId == edge.Node.Id);
                existingContent.TitleEn = edge.Node.Content?.Title;
                context.Contents.Update(existingContent);

                continue;
            }
        }

        context.SaveChanges();
    }
}

async Task PopulateCast()
{
    var crawler = new TmdbCrawler();
    var offset = 0;
    while (true)
    {
        var context = ContextFactory.CreateDbContextPostgreSQL();
        var contents = context.Contents
            .Where(x => !string.IsNullOrEmpty(x.TmdbId) && x.Cast == null)
            .OrderBy(x => x.Availabilities.Min(a => a.TmdbRanking))
            .Skip(offset)
            .Take(100)
            .ToList();

        foreach (var content in contents)
        {
            var details = await crawler.CrawlDetails(content.Type, content.TmdbId!);
            if (details == null || details.Credits?.Cast == null)
            {
                Console.WriteLine($"No details found for {content.TmdbId}");
                continue; // Skip if no details found
            }
            content.Cast = string.Join(" | ", details.Credits.Cast.OrderBy(x => x.Order).Take(10).Select(x => x.Name));
            context.Update(content);

            await Task.Delay(200);
        }

        

        context.SaveChanges();

        offset += 100;

        if ( contents.Count == 0)
        {
            break;
        }
    }
}

async Task CrawlTmdb()
{
    var providers = new[]
    {
        2, // Apple TV
        8, // Netflix
        119, // Amazon Prime Video
        151, // Britbox
        337, // Disney Plus
        338, // Ruutu
        350, // Apple TV+
        553, // Telia Play
        1773, // SkyShowtime
        1899, // HBO Max
        2029, // MTV Katsomo
    };

    var crawler = new TmdbCrawler();

    foreach(var provider in providers)
    {
        Console.WriteLine($"Crawling TMDB for provider {provider} movies");
        await crawler.Crawl(provider, ContentType.Movie);

        Console.WriteLine($"Crawling TMDB for provider {provider} series");
        await crawler.Crawl(provider, ContentType.Series);

        Console.WriteLine($"Finished crawling TMDB for provider {provider}.");
    }
}

void CompareJustWatchTmdb()
{
    var mappings = new Dictionary<string, string>
    {
        { "dnp", "337" },
        { "nfx", "8" },
        { "prv", "119" },
        { "sst", "1773" },
        { "mxx", "1899" },
        { "atp", "2" },
        { "tlp", "553" },
        { "bbo", "151" },
        { "mtk", "2029" }
    };
    var dir = "tmdb";

    foreach (var mapping in mappings)
    {
        var providerId = mapping.Key;
        var tmdbProviderId = mapping.Value;

        var items = new List<(ContentType Type, long? TmdbId, string Title, string OriginalTitle, int? Year, double? Popularity, long? VoteCount, string? Description)>();
        var tmdbIds = new HashSet<long>();

        foreach (var file in Directory.GetFiles(dir))
        {
            if(!file.Contains($"_{tmdbProviderId}_"))
            {
                continue; // Skip files not matching the provider ID
            }
            var json = File.ReadAllText(file);
            var response = JsonSerializer.Deserialize<DiscoverResponse>(json);
            if (response?.Results == null || response.Results.Length == 0)
            {
                continue;
            }

            foreach (var item in response.Results)
            {
                if (item.Id == null || item.Id < 1 || tmdbIds.Contains(item.Id.Value))
                {
                    continue; // Skip if no ID or already processed
                }
                
                if (file.StartsWith("tmdb\\movie"))
                {
                    // check
                    if (!Helpers.IsLocalText(item.Title))
                    {
                        Console.WriteLine(item.Title);
                        continue;
                    }
                    items.Add((ContentType.Movie, item.Id, item.Title, item.OriginalTitle, item.ReleaseDate?.Year, item.Popularity, item.VoteCount, item.Overview));
                }
                else
                {
                    if (!Helpers.IsLocalText(item.Name))
                    {
                        Console.WriteLine(item.Name);
                        continue; 
                    }
                    items.Add((ContentType.Series, item.Id, item.Name, item.OriginalName, item.FirstAirDate?.Year, item.Popularity, item.VoteCount, item.Overview));
                }

                if (item.Id.HasValue)
                {
                    tmdbIds.Add(item.Id.Value);
                }
            }

        }

        var context = ContextFactory.CreateDbContextPostgreSQL();
        var contentItems = context.Contents
            .Where(x => x.Availabilities.Any(a => a.Service.JustWatchId == providerId))
            .ToList();


        var jwOnly = new List<MixFlix.Data.Content>();
        foreach (var content in contentItems)
        {
            if(content.TmdbId != null)
            {
                continue;
            }
            var match = items.FirstOrDefault(x => x.Type == content.Type && x.Title == content.Title && x.Year == content.ReleaseYear && x.Description == content.ShortDescription);
            if (match.TmdbId != null)
            {
                continue;
            }

            jwOnly.Add(content);
        }

        var tmdbOnly = new List<(ContentType Type, long? TmdbId, string Title, string OriginalTitle, int? Year, double? Popularity, long? VoteCount, string? Description)>();
        foreach (var item in items)
        {
            var match = contentItems.FirstOrDefault(x => x.TmdbId == item.TmdbId?.ToString());
            if(match != null)
            {
                continue;
            }
            match = contentItems.FirstOrDefault(x => x.Title == item.Title && x.ShortDescription == item.Description && x.ReleaseYear == item.Year);
            if (match != null)
            {
                continue;
            }

            tmdbOnly.Add(item);
        }

        ;
    }

    

}

async Task DownloadTmdbPosters()
{
    var dir = "tmdb";

    var imagePaths = new Dictionary<string, string>();

    foreach (var file in Directory.GetFiles(dir))
    {
        var type = file.Contains("movie") ? ContentType.Movie : file.Contains("tv") ? ContentType.Series : ContentType.None;

        var json = File.ReadAllText(file);
        var response = JsonSerializer.Deserialize<DiscoverResponse>(json);
        if (response?.Results == null || response.Results.Length == 0)
        {
            Console.WriteLine($"No results found in {file}");
            continue;
        }

        foreach (var item in response.Results)
        {
            if (item.Id == null || item.Id < 1)
            {
                continue; // Skip if no ID
            }

            imagePaths.TryAdd(item.Id.ToString(), item.PosterPath);
        }
    }

    // https://media.themoviedb.org/t/p/w300_and_h450_bestv2/yvirUYrva23IudARHn3mMGVxWqM.jpg
    var context = ContextFactory.CreateDbContextPostgreSQL();
    var contents = context.Contents.Where(x => x.TmdbId != null).ToList();
    var client = new HttpClient();
    foreach (var content in contents)
    {
        var directory = "C:\\code\\github\\miettis\\mixflix\\MixFlix\\MixFlix.Crawler\\bin\\Debug\\net8.0\\tmdb_posters";
        var filename = $"{content.Id}.jpg";
        var filePath = Path.Combine(directory, filename);
        if (File.Exists(filePath))
        {
            Console.WriteLine($"File already exists: {filePath}");
            continue; // Skip if file already exists
        }

        if(!imagePaths.TryGetValue(content.TmdbId, out var imageUrl))
        {
            Console.WriteLine($"No image URL found for content {content.TmdbId}");
            continue; // Skip if no image URL
        }
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            continue;
        }

        var url = $"https://media.themoviedb.org/t/p/w300_and_h450_bestv2{imageUrl}";
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to download {url}: {response.StatusCode}");
            continue; // Skip if download failed
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(filePath, bytes);

        await Task.Delay(200);
    }
}

