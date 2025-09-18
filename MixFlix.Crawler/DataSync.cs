using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using MixFlix.Data;

namespace MixFlix.Crawler
{
    internal class DataSync
    {
        private readonly DataSyncSettings _settings;
        private readonly string? _sourceConnection;
        private readonly string? _targetConnection;
        private readonly ILogger<DataSync> _logger;

        public DataSync(DataSyncSettings settings, ILogger<DataSync> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task SyncCategories()
        {
            _logger.LogInformation("Starting SyncCategories");
            using var sourceContext = GetSourceContext();
            using var targetContext = GetTargetContext();
            var categories = await sourceContext.Categories.ToListAsync();
            foreach (var category in categories)
            {
                var existingCategory = await targetContext.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);
                if (existingCategory == null)
                {
                    targetContext.Categories.Add(category);
                }
                else
                {
                    existingCategory.Name = category.Name;
                    existingCategory.TmdbId = category.TmdbId;
                    existingCategory.JustWatchId = category.JustWatchId;
                }
            }
            await targetContext.SaveChangesAsync();
            _logger.LogInformation("Finished SyncCategories");
        }

        public async Task SyncServices()
        {
            _logger.LogInformation("Starting SyncServices");
            using var sourceContext = GetSourceContext();
            using var targetContext = GetTargetContext();
            var services = await sourceContext.Services.ToListAsync();
            foreach (var service in services)
            {
                var existingService = await targetContext.Services.FirstOrDefaultAsync(s => s.Id == service.Id);
                if (existingService == null)
                {
                    targetContext.Services.Add(service);
                }
                else
                {
                    existingService.Name = service.Name;
                    existingService.Logo = service.Logo;
                    existingService.JustWatchId = service.JustWatchId;
                    existingService.TmdbId = service.TmdbId;
                }
            }
            await targetContext.SaveChangesAsync();
            _logger.LogInformation("Finished SyncServices");
        }

        public async Task SyncContents()
        {
            _logger.LogInformation("Starting SyncContents");
            using var sourceContext = GetSourceContext();
            
            const int batchSize = 100; // You can adjust the batch size as needed
            var totalCount = await sourceContext.Contents.CountAsync();
            int processed = 0;
            for (int skip = 0; skip < totalCount; skip += batchSize)
            {
                var contents = await sourceContext.Contents
                    .Include(x => x.Categories)
                    .Include(x => x.Availabilities)
                    .OrderBy(x => x.Id)
                    .Skip(skip)
                    .Take(batchSize)
                    .ToListAsync();

                using var targetContext = GetTargetContext();
                var targetCategories = await targetContext.Categories
                    .ToListAsync();


                foreach (var content in contents)
                {
                    var existingContent = await targetContext.Contents
                        .Include(x => x.Categories)
                        .Include(x => x.Availabilities)
                        .FirstOrDefaultAsync(c => c.Id == content.Id);

                    var categoryIds = content.Categories.Select(c => c.Id).ToList();
                    

                    if (existingContent == null)
                    {
                        content.Categories = targetCategories.Where(x => categoryIds.Contains(x.Id)).ToList();

                        targetContext.Contents.Add(content);
                    }
                    else
                    {
                        existingContent.JustWatchId = content.JustWatchId;
                        existingContent.TmdbId = content.TmdbId;
                        existingContent.ImdbId = content.ImdbId;
                        existingContent.Title = content.Title;
                        existingContent.TitleEn = content.TitleEn;
                        existingContent.ShortDescription = content.ShortDescription;
                        existingContent.ImageUrl = content.ImageUrl;
                        existingContent.ClipUrl = content.ClipUrl;
                        existingContent.ReleaseYear = content.ReleaseYear;
                        existingContent.Runtime = content.Runtime;
                        existingContent.LikeCount = content.LikeCount;
                        existingContent.DislikeCount = content.DislikeCount;
                        existingContent.ImdbVotes = content.ImdbVotes;
                        existingContent.ImdbScore = content.ImdbScore;
                        existingContent.TmdbPopularity = content.TmdbPopularity;
                        existingContent.TmdbScore = content.TmdbScore;
                        existingContent.TomatoMeter = content.TomatoMeter;
                        existingContent.JustWatchRating = content.JustWatchRating;
                        existingContent.Language = content.Language;
                        existingContent.Cast = content.Cast;
                        existingContent.Categories = targetCategories.Where(x => categoryIds.Contains(x.Id)).ToList();

                        targetContext.Update(existingContent);
                    }
                }
                await targetContext.SaveChangesAsync();

                processed += contents.Count;
                _logger.LogInformation($"Processed {processed}/{totalCount} contents");
            }
            _logger.LogInformation("Finished SyncContents");
        }

        public async Task SyncContentAvailabilities()
        {
            _logger.LogInformation("Starting SyncContentAvailabilities");
            using var sourceContext = GetSourceContext();
            const int batchSize = 500;
            var totalCount = await sourceContext.ContentAvailabilities.CountAsync();
            int processed = 0;
            for (int skip = 0; skip < totalCount; skip += batchSize)
            {
                var contentAvailabilities = await sourceContext.ContentAvailabilities
                    .OrderBy(x => x.Id)
                    .Skip(skip)
                    .Take(batchSize)
                    .ToListAsync();

                using var targetContext = GetTargetContext();
                foreach (var availability in contentAvailabilities)
                {
                    var existingAvailability = await targetContext.ContentAvailabilities.FirstOrDefaultAsync(cs => cs.Id == availability.Id);
                    if (existingAvailability == null)
                    {
                        targetContext.ContentAvailabilities.Add(availability);
                    }
                    else
                    {
                        existingAvailability.ServiceId = availability.ServiceId;
                        existingAvailability.ContentId = availability.ContentId;
                        existingAvailability.JustWatchLastSeen = availability.JustWatchLastSeen;
                        existingAvailability.JustWatchRanking = availability.JustWatchRanking;
                        existingAvailability.TmdbLastSeen = availability.TmdbLastSeen;
                        existingAvailability.TmdbRanking = availability.TmdbRanking;
                    }
                }
                await targetContext.SaveChangesAsync();
                processed += contentAvailabilities.Count;
                _logger.LogInformation($"Processed {processed}/{totalCount} content availabilities");
            }
            _logger.LogInformation("Finished SyncContentAvailabilities");
        }

        public void SyncImages()
        {
            using var client = new ScpClient(_settings.Host, _settings.Username, _settings.Password);

            client.Connect();

            DateTimeOffset timestamp;

            try
            {

                using var stream = new MemoryStream();
                client.Download(_settings.RemoteDir + "/log.txt", stream);

                stream.Position = 0; // Reset stream position to the beginning
                using var reader = new StreamReader(stream);
                var text = reader.ReadToEnd();

                timestamp = DateTimeOffset.Parse(text);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read log.txt, using current time minus 7 days");
                timestamp = DateTimeOffset.Now.AddDays(-7);
            }

            foreach (var file in new DirectoryInfo(_settings.LocalDir).GetFiles())
            {
                // if file created less than 1 hour ago, skip
                if (file.LastWriteTimeUtc < timestamp)
                {
                    continue;
                }
                var filename = file.Name;
                try
                {
                    client.Upload(file, _settings.RemoteDir + "/" + filename);
                    Console.WriteLine($"Uploaded: {filename}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to upload {filename}");
                }
            }

            timestamp = DateTimeOffset.UtcNow;
            using var uploadStream = new MemoryStream();
            using var writer = new StreamWriter(uploadStream);
            writer.WriteLine(timestamp.ToString("o"));
            writer.Flush();
            uploadStream.Position = 0; // Reset stream position to the beginning
            client.Upload(uploadStream, _settings.RemoteDir + "/log.txt");
        }

        private Context GetSourceContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            Context.ConfigureOptions(optionsBuilder, _settings.SourceConnection);
            return new Context(optionsBuilder.Options);
        }
        private Context GetTargetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            Context.ConfigureOptions(optionsBuilder, _settings.TargetConnection);
            return new Context(optionsBuilder.Options);
        }

        internal async Task TestConnections()
        {
            _logger.LogInformation("Testing connections");
            using var sourceContext = GetSourceContext();
            using var targetContext = GetTargetContext();
            try
            {
                await sourceContext.Database.CanConnectAsync();
                _logger.LogInformation("Source connection is valid.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Source connection failed.");
            }
            try
            {
                await targetContext.Database.CanConnectAsync();
                _logger.LogInformation("Target connection is valid.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Target connection failed.");
            }
        }
    }
}
