using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MixFlix.Data;
using MixFlix.Web.Models;
using System.Security.Claims;

namespace MixFlix.Web.Controllers
{
    public class ContentController : ApiController
    {
        private readonly IConfiguration _config;
        public ContentController(Context context, IConfiguration config) : base(context) 
        { 
            _config = config;
        }

        // POST: api/content/{contentId}/review
        [HttpPost("{contentId}/review")]
        [EndpointName("ReviewContent")]
        public async Task<IActionResult> ReviewContent(Guid contentId, [FromBody] ReviewRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var user = await Context.Users.FirstOrDefaultAsync(u => u.ExternalId == userIdClaim);
            if (user == null)
            {
                return Unauthorized();
            }

            var content = await Context.Contents.FirstOrDefaultAsync(c => c.Id == contentId);
            if (content == null)
            {
                return NotFound();
            }

            var existingRating = await Context.UserRatings.FirstOrDefaultAsync(r => r.UserId == user.Id && r.ContentId == contentId && r.Type == request.Type);
            if (existingRating != null)
            {
                existingRating.Rating = request.Rating;
                existingRating.RatingTime = DateTimeOffset.UtcNow;
            }
            else
            {
                var rating = new UserRating
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ContentId = contentId,
                    Rating = request.Rating,
                    Type = request.Type,
                    RatingTime = DateTimeOffset.UtcNow
                };
                Context.UserRatings.Add(rating);
            }
            await Context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("images/{*path}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImage(string path)
        {
            var dir = _config["ImageDir"];
            var filePath = Path.Combine(dir, path.Replace("/","_"));

            if (System.IO.File.Exists(filePath))
            {
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "image/avif");
            }

            var dir2 = @"C:\code\github\miettis\mixflix\MixFlix\MixFlix.Crawler\bin\Debug\net8.0\tmdb_posters";
            filePath = Path.Combine(dir2, path.Replace("/", "_"));
            if (System.IO.File.Exists(filePath))
            {
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "image/avif");
            }

            return NotFound();
        }

        [HttpPost("search")]
        [EndpointName("SearchContent")]
        [Produces<SearchContentResponse>]
        public async Task<IActionResult> SearchContent([FromBody] SearchContentRequest request)
        {
            var user = await GetCurrentUserAsync();
            var query = Context.Contents
                .Include(c => c.Categories)
                .Include(c => c.Availabilities)
                .ThenInclude(a => a.Service)
                .AsQueryable();

            // Filter by services
            if (request.Services != null && request.Services.Count > 0)
            {
                query = query.Where(c => c.Availabilities.Any(a => request.Services.Contains(a.ServiceId)));
            }
            // Filter by content types
            if (request.ContentTypes != null && request.ContentTypes.Count > 0)
            {
                query = query.Where(c => request.ContentTypes.Contains(c.Type));
            }
            // Filter by categories
            if (request.Categories != null && request.Categories.Count > 0)
            {
                query = query.Where(c => c.Categories.Any(cat => request.Categories.Contains(cat.Id)));
            }
            // Filter by title contains (case-insensitive)
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                var titleFilter = request.Title.ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(titleFilter));
            }
            // Filter by reviewed/not reviewed
            if (user != null)
            {
                if (request.Reviewed == true)
                {
                    query = query.Where(c => Context.UserRatings.Any(r => r.UserId == user.Id && r.ContentId == c.Id));
                }
                if (request.NotReviewed == true)
                {
                    query = query.Where(c => !Context.UserRatings.Any(r => r.UserId == user.Id && r.ContentId == c.Id));
                }
            }
            // Filter by ratings (if provided)
            if (request.Ratings != null && request.Ratings.Length > 0)
            {
                var ratedContentIds = Context.UserRatings.Where(r => r.UserId == user.Id && request.Ratings.Contains(r.Rating))
                                                    .Select(r => r.ContentId)
                                                    .ToList();

                query = query.Where(c => ratedContentIds.Contains(c.Id));
            }

            int totalCount = await query.CountAsync();
            int pageSize = request.PageSize ?? 20;
            int offset = request.Offset ?? 0;

            var contents = await query.Skip(offset).Take(pageSize).ToListAsync();
            var result = contents.Select(c => new UserContentResponse
            {
                Id = c.Id,
                Type = c.Type,
                Title = c.Title,
                ShortDescription = c.ShortDescription,
                ImageUrl = c.ImageUrl,
                ClipUrl = c.ClipUrl,
                ReleaseYear = c.ReleaseYear,
                Runtime = c.Runtime,
                LikeCount = c.LikeCount,
                DislikeCount = c.DislikeCount,
                ImdbVotes = c.ImdbVotes,
                ImdbScore = c.ImdbScore,
                TmdbPopularity = c.TmdbPopularity,
                TmdbScore = c.TmdbScore,
                TomatoMeter = c.TomatoMeter,
                JustWatchRating = c.JustWatchRating,
                Languages = c.Language?.Split(','),
                Cast = c.Cast?.Split(" | "),
                Categories = c.Categories?.Select(cat => new CategoryResponse 
                { 
                    Id = cat.Id, 
                    Name = cat.Name 
                }).ToList() ?? new List<CategoryResponse>(),
                Availabilities = c.Availabilities?.Select(a => new ContentAvailabilityResponse 
                {
                    ServiceId = a.ServiceId, 
                    ServiceName = a.Service.Name, 
                    Ranking = a.JustWatchRanking, 
                    ServiceLogo = a.Service.Logo 
                }).ToList() ?? new List<ContentAvailabilityResponse>()
            }).ToList();

            var contentIds = contents.Select(c => c.Id).ToList();

            var userReviews = await Context.UserRatings
                .Where(r => r.UserId == user.Id && contentIds.Contains(r.ContentId))
                .ToDictionaryAsync(r => r.ContentId, r => new { r.Rating, r.RatingTime });

            foreach (var item in result)
            {
                if (userReviews.TryGetValue(item.Id, out var rating))
                {
                    item.UserRating = rating.Rating;
                    item.UserRatingTime = rating.RatingTime;
                }
                else
                {
                    item.UserRating = null;
                }

                item.ImageUrl = GetContentImageUrl(item.Id, item.ImageUrl);
            }

            // Sorting
            switch (request.SortBy)
            {
                case ContentSorting.Alphabetical:
                    result = result.OrderBy(c => c.Title).ToList();
                    break;
                case ContentSorting.MostPopular:
                    result = result.OrderByDescending(c => c.Availabilities.Min(x => x.Ranking)).ToList();
                    break;
                case ContentSorting.YearAscending:
                    result = result.OrderBy(c => c.ReleaseYear).ToList();
                    break;
                case ContentSorting.YearDescending:
                    result = result.OrderByDescending(c => c.ReleaseYear).ToList();
                    break;
                case ContentSorting.ImdbRating:
                    result = result.OrderByDescending(c => c.ImdbScore).ToList();
                    break;
                case ContentSorting.TmdbRating:
                    result = result.OrderByDescending(c => c.TmdbScore).ToList();
                    break;
                case ContentSorting.MyRatingAscending:
                    result = result.OrderBy(c => c.UserRating).ToList();
                    break;
                case ContentSorting.MyRatingDescending:
                    result = result.OrderByDescending(c => c.UserRating).ToList();
                    break;
                case ContentSorting.MyRatingTimeAscending:
                    result = result.OrderBy(c => c.UserRatingTime).ToList();
                    break;
                case ContentSorting.MyRatingTimeDescending:
                    result = result.OrderByDescending(c => c.UserRatingTime).ToList();
                    break;
                default:
                    // No sorting
                    break;
            }

            var response = new SearchContentResponse
            {
                Items = result,
                TotalCount = totalCount
            };

            return Ok(response);
        }

        [HttpGet("duplicates")]
        [EndpointName("GetPossibleDuplicates")]
        [Produces<PossibleDuplicateResponse[]>]
        public async Task<IActionResult> GetPossibleDuplicates()
        {
            var contents = await Context.Contents
                .Where(x => x.JustWatchId == null || x.TmdbId == null)
                .Select(x => new { x.Id, x.Type, x.Title, x.TitleEn, x.ReleaseYear, x.ShortDescription, x.ImageUrl })
                .ToListAsync();

            var duplicates = contents.GroupBy(x => new { x.Type, x.Title, x.ReleaseYear })
                .Where(x => x.Count() > 1)
                .Select(x => new PossibleDuplicateResponse
                {
                    Type = x.Key.Type,
                    Title = x.Key.Title,
                    ReleaseYear = x.Key.ReleaseYear ?? 0,
                    Items = x.Select(item => new PossibleDuplicateItem
                    {
                        Id = item.Id,
                        ShortDescription = item.ShortDescription,
                        ImageUrl = GetContentImageUrl(item.Id, item.ImageUrl)
                    }).ToList()
                });

            return Ok(duplicates);
        }

        [HttpPost("merge")]
        [EndpointName("MergeContent")]
        public async Task<IActionResult> MergeContent([FromBody] MergeContentRequest request)
        {

            var user = await GetCurrentUserAsync();
            if(user.Email != "mikemietti@gmail.com")
            {
                return Unauthorized();
            }

            if(request.ContentIds == null || request.ContentIds.Length < 2 || request.DescriptionContentId == Guid.Empty)
            {
                return BadRequest();
            }

            var contents = await Context.Contents
                .Where(c => request.ContentIds.Contains(c.Id))
                .ToListAsync();

            var primaryContent = contents.FirstOrDefault(c => c.TmdbId != null);
            var secondaryContent = contents.FirstOrDefault(c => c.JustWatchId != null);

            if (primaryContent == null || secondaryContent == null)
            {
                return BadRequest("Both primary (TMDB) and secondary (JustWatch) content must be present.");
            }

            // Copy UserRatings from secondary to primary
            var secondaryRatings = await Context.UserRatings
                .Where(r => r.ContentId == secondaryContent.Id)
                .ToListAsync();

            foreach (var rating in secondaryRatings)
            {
                var existing = await Context.UserRatings.FirstOrDefaultAsync(r => r.UserId == rating.UserId && r.ContentId == primaryContent.Id && r.Type == rating.Type);
                if (existing == null)
                {
                    Context.UserRatings.Add(new UserRating
                    {
                        Id = Guid.NewGuid(),
                        UserId = rating.UserId,
                        ContentId = primaryContent.Id,
                        Rating = rating.Rating,
                        Type = rating.Type,
                        RatingTime = rating.RatingTime
                    });
                }
            }

            if (request.DescriptionContentId != primaryContent.Id)
            {
                // Copy description from the content with description
                var descriptionContent = contents.FirstOrDefault(c => c.Id == request.DescriptionContentId);
                if (descriptionContent != null)
                {
                    primaryContent.ShortDescription = descriptionContent.ShortDescription;
                }
            }

            Context.Contents.Remove(secondaryContent);

            await Context.SaveChangesAsync();

            return Ok();
        }
    }
}
