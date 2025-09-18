using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MixFlix.Data;
using MixFlix.Web.Models;
using System.Text.Json;

namespace MixFlix.Web.Controllers
{
    public class GroupsController : ApiController
    {
        public GroupsController(Context context) : base(context) { }

        // GET: api/groups - Get all groups where current user is a member
        [HttpGet("")]
        [EndpointName("GetGroups")]
        [Produces<GroupResponse[]>]
        public async Task<IActionResult> GetGroups()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var groups = await Context.Groups
                .Include(g => g.Members)
                .Where(g => g.Members.Any(m => m.UserId == user.Id))
                .Select(x => new GroupResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreatorId = x.CreatorId,
                    SettingsJson = x.Settings,
                    MemberCount = x.Members.Count,
                    RequireApproval = x.RequireApproval ?? false,
                    IsCreator = x.CreatorId == user.Id,
                    IsMember = true,
                    
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            foreach(var group in groups)
            {
                group.Settings = string.IsNullOrWhiteSpace(group.SettingsJson) 
                    ? new GroupSettings() 
                    : JsonSerializer.Deserialize<GroupSettings>(group.SettingsJson) ?? new GroupSettings();

                group.ShareLink = $"https://{Request.Host.ToUriComponent()}/#/groups/{group.Id}";
            }

            return Ok(groups);
        }

        [HttpGet("{id}", Name = nameof(GetGroup))]
        [EndpointName("GetGroup")]
        [Produces<GroupResponse>]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroup(Guid id)
        {
            var user = await GetCurrentUserAsync();

            var group = await Context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            var response = new GroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                CreatorId = group.CreatorId,
                MemberCount = group.Members.Count,
                RequireApproval = group.RequireApproval ?? false,
                IsCreator = group.CreatorId == user?.Id,
                IsMember = group.Members.Any(x => x.UserId == user?.Id),
                Settings = string.IsNullOrWhiteSpace(group.Settings)
                    ? new GroupSettings()
                    : JsonSerializer.Deserialize<GroupSettings>(group.Settings) ?? new GroupSettings(),
                Members = group.CreatorId == user?.Id ? group.Members.Select(x => new MemberResponse {  Id = x.UserId, Name = x.User.Name, Status = x.Status }).ToList() : null,
                ShareLink = $"https://{Request.Host.ToUriComponent()}/#/groups/{group.Id}"
            };

            return Ok(response);
        }

        // POST: api/groups - Create a new group
        [HttpPost("")]
        [EndpointName("CreateGroup")]
        [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateGroup([FromBody] GroupRequest request)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var group = new Group
            {
                Name = request.Name,
                RequireApproval = request.RequireApproval,
                CreatorId = user.Id,
                Settings = request.Settings != null ? JsonSerializer.Serialize(request.Settings) : "",
                Members = new List<GroupMember>()
            };
            group.Members.Add(new GroupMember { Group = group, User = user, Status = GroupMemberStatus.Approved });
            Context.Groups.Add(group);
            await Context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGroups), new { id = group.Id }, new GroupResponse 
            { 
                Id = group.Id, 
                Name = group.Name,
                MemberCount = 1,
                CreatorId = group.CreatorId
            }
            );
        }

        // PUT: api/groups/{id} - Update group (only by creator)
        [HttpPut("{id}")]
        [EndpointName("UpdateGroup")]
        public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] GroupRequest request)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var group = await Context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            if (group.CreatorId != user.Id)
            {
                return Forbid();
            }

            group.Name = request.Name;
            group.RequireApproval = request.RequireApproval;
            group.Settings = request.Settings != null ? JsonSerializer.Serialize(request.Settings) : "";
            // Ensure creator remains a member
            if (!group.Members.Any(m => m.UserId == user.Id))
            {
                group.Members.Add(new GroupMember { Group = group, User = user, Status = GroupMemberStatus.Approved });
            }
            await Context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: api/groups/{id} - Delete group (only by creator)
        [HttpDelete("{id}")]
        [EndpointName("DeleteGroup")]
        public async Task<IActionResult> DeleteGroup(Guid id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var group = await Context.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }
            if (group.CreatorId != user.Id)
            {
                return Forbid();
            }
            Context.Groups.Remove(group);
            await Context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/groups/{groupId}/content - Get filtered content for a group
        [HttpGet("{groupId}/content")]
        [EndpointName("GetGroupContent")]
        [Produces<ContentResponse[]>]
        public async Task<IActionResult> GetGroupContent(Guid groupId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var group = await Context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (!group.Members.Any(m => m.UserId == user.Id))
            {
                return Forbid();
            }

            // Deserialize settings
            GroupSettings? settings = null;
            if (!string.IsNullOrWhiteSpace(group.Settings))
            {
                try
                {
                    settings = JsonSerializer.Deserialize<GroupSettings>(group.Settings);
                }
                catch
                {
                }
            }

            var query = Context.Contents
                .Include(c => c.Categories)
                .Include(c => c.Availabilities)
                .AsQueryable();

            query = ApplyGroupSettingsFilter(query, settings);

            // Filter out content the user has already reviewed
            var reviewedContentIds = await Context.UserRatings
                .Where(x => x.UserId == user.Id)
                .Select(x => x.ContentId)
                .ToListAsync();

            query = query.Where(c => !reviewedContentIds.Contains(c.Id));

            query = query.OrderBy(x => x.Availabilities.Min(a => a.JustWatchRanking));

            // fetch content other group members have reviewed
            var otherMemberIds = group.Members.Select(m => m.UserId).Except([user.Id]).ToList();
            var interestingContentForOthers = await Context.UserRatings
                .Where(x => otherMemberIds.Contains(x.UserId) && x.Rating > 0)
                .Select(x => new { x.ContentId, x.Rating })
                .ToListAsync();

            var rankedContent = interestingContentForOthers
                .GroupBy(x => x.ContentId)
                .Select(g => new
                {
                    ContentId = g.Key,
                    Sum = g.Sum(x => x.Rating),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Sum)
                .Select((x, index) => new {x.ContentId, Index = index})
                .ToDictionary(x =>x.ContentId, x => x.Index);

            var contentItems = query
                .Include(x => x.Categories)
                .Include(x => x.Availabilities)
                .ThenInclude(x => x.Service)
                .Take(100)
                .ToList()
                .Select(x => new ContentResponse 
                {
                    Id = x.Id,
                    Type = x.Type,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    ImageUrl = x.ImageUrl,
                    ClipUrl = x.ClipUrl,
                    ReleaseYear = x.ReleaseYear,
                    Runtime = x.Runtime,
                    LikeCount = x.LikeCount,
                    DislikeCount = x.DislikeCount,
                    ImdbVotes = x.ImdbVotes,
                    ImdbScore = x.ImdbScore,
                    TmdbPopularity = x.TmdbPopularity,
                    TmdbScore = x.TmdbScore,
                    TomatoMeter = x.TomatoMeter,
                    JustWatchRating = x.JustWatchRating,
                    Languages = x.Language?.Split(','),
                    Cast = x.Cast?.Split(" | "),
                    Categories = x.Categories.Select(c => new CategoryResponse 
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToList(),
                    Availabilities = x.Availabilities.Select(a => new ContentAvailabilityResponse 
                    {
                        ServiceId = a.ServiceId,
                        ServiceLogo = a.Service.Logo,
                        ServiceName = a.Service.Name,
                        Ranking = a.JustWatchRanking
                    }).ToList()
                })
                .ToList();

            foreach(var item in contentItems)
            {
                item.ImageUrl = GetContentImageUrl(item.Id, item.ImageUrl);
            }

            contentItems.OrderBy(x => rankedContent.GetValueOrDefault(x.Id, rankedContent.Count));

            return Ok(contentItems);
        }

        [HttpPost("{id}/join")]
        [EndpointName("JoinGroup")]
        public async Task<IActionResult> JoinGroup(Guid id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var group = await Context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if(!group.Members.Any(x => x.UserId == user.Id))
            {
                group.Members.Add(new GroupMember
                {
                    Group = group,
                    User = user,
                    Status = group.RequireApproval == true ? GroupMemberStatus.Pending : GroupMemberStatus.Approved
                });

                await Context.SaveChangesAsync();
            }

            return Ok();
        }

        // PUT: api/groups/{id}/users/{userId}/state - Change group member state
        [HttpPut("{id}/users/{userId}/state")]
        [EndpointName("ChangeMemberState")]
        public async Task<IActionResult> ChangeMemberState(Guid id, Guid userId, [FromBody] ChangeMemberStateRequest request)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var group = await Context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (group.CreatorId != user.Id)
            {
                return Unauthorized();
            }

            var member = group.Members.FirstOrDefault(x => x.UserId == userId);
            if (member == null)
            {
                return BadRequest();
            }

            if(request.State == GroupMemberStatus.Rejected)
            {
                group.Members.Remove(member);
            }
            else
            {
                member.Status = request.State;
            }
            await Context.SaveChangesAsync();
            return Ok();
        }

        // GET: api/groups/{groupId}/suggestions - Get most suitable content for the group
        [HttpGet("{groupId}/suggestions")]
        [EndpointName("GetGroupSuggestions")]
        [Produces<GroupSuggestionsResponse>]
        public async Task<IActionResult> GetGroupSuggestions(Guid groupId, [FromQuery] int? pageSize, [FromQuery] int? offset)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var group = await Context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound();
            }
            if (!group.Members.Any(m => m.UserId == user.Id))
            {
                return Forbid();
            }

            // Deserialize settings
            GroupSettings? settings = null;
            if (!string.IsNullOrWhiteSpace(group.Settings))
            {
                try
                {
                    settings = JsonSerializer.Deserialize<GroupSettings>(group.Settings);
                }
                catch { }
            }

            var memberIds = group.Members.Select(m => m.UserId).ToList();
            var groupSize = memberIds.Count;
            var requiredRaters = groupSize == 2 ? 2 : groupSize - 1;

            // Get all ratings for group members
            var ratings = await Context.UserRatings
                .Where(r => memberIds.Contains(r.UserId) && r.Rating > 0)
                .Select(r => new 
                {
                    r.ContentId,
                    r.Rating,
                    r.UserId,
                    MinRanking = r.Content.Availabilities.Min(a => a.JustWatchRanking),
                })
                .ToListAsync();

            // Filter content by group settings
            var contentQuery = Context.Contents
                .Include(c => c.Categories)
                .Include(c => c.Availabilities)
                .ThenInclude(a => a.Service)
                .AsQueryable();

            contentQuery = ApplyGroupSettingsFilter(contentQuery, settings);

            // Only consider content that matches settings and has enough ratings
            var ratedContentIds = ratings.GroupBy(r => r.ContentId)
                .Where(g => g.Select(x => x.UserId).Distinct().Count() >= requiredRaters)
                .Select(g => g.Key)
                .ToList();

            var filteredContent = await contentQuery
                .Where(c => ratedContentIds.Contains(c.Id))
                .ToListAsync();

            var contentMap = filteredContent.ToDictionary(c => c.Id);

            var grouped = ratings
                .Where(r => contentMap.ContainsKey(r.ContentId))
                .GroupBy(r => r.ContentId)
                .Select(g => new
                {
                    ContentId = g.Key,
                    RatingSum = g.Sum(x => x.Rating),
                    RatingCount = g.Select(x => x.UserId).Distinct().Count(),
                    MinRanking = g.Min(x => x.MinRanking),
                    Rating = (double)g.Average(x => x.Rating) - Math.Sqrt(g.Average(x => Math.Pow((double)(x.Rating - g.Average(y => y.Rating)), 2))) / group.Members.Count,
                })
                .OrderByDescending(x => x.Rating)
                .ThenBy(x => x.MinRanking)
                .ToList();

            int totalCount = grouped.Count;
            int ps = pageSize ?? 20;
            int off = offset ?? 0;
            var paged = grouped.Skip(off).Take(ps).ToList();

            var result = paged
                .Where(x => contentMap.ContainsKey(x.ContentId))
                .Select(x => {
                    var c = contentMap[x.ContentId];
                    return new GroupSuggestion
                    {
                        Id = c.Id,
                        Type = c.Type,
                        Title = c.Title,
                        ShortDescription = c.ShortDescription,
                        ImageUrl = GetContentImageUrl(c.Id, c.ImageUrl),
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
                        Categories = c.Categories?.Select(cat => new CategoryResponse { Id = cat.Id, Name = cat.Name }).ToList() ?? new List<CategoryResponse>(),
                        Availabilities = c.Availabilities?.Select(a => new ContentAvailabilityResponse { ServiceId = a.ServiceId, ServiceName = a.Service.Name, ServiceLogo = a.Service.Logo, Ranking = a.JustWatchRanking }).ToList() ?? new List<ContentAvailabilityResponse>(),
                        TotalScore = x.RatingSum,
                        RatingCount = x.RatingCount,
                        WeightedScore = (decimal)Math.Round(x.Rating, 1),

                    };
                })
                .ToList();

            var response = new GroupSuggestionsResponse
            {
                Items = result,
                TotalCount = totalCount
            };

            return Ok(response);
        }

        private IQueryable<Content> ApplyGroupSettingsFilter(IQueryable<Content> query, GroupSettings? settings)
        {
            if (settings != null)
            {
                if (settings.Services != null && settings.Services.Count > 0)
                {
                    query = query.Where(c => c.Availabilities.Any(a => settings.Services.Contains(a.ServiceId)));
                }
                if (settings.ContentTypes != null && settings.ContentTypes.Count > 0)
                {
                    query = query.Where(c => settings.ContentTypes.Contains(c.Type));
                }
                if (settings.Categories != null && settings.Categories.Count > 0)
                {
                    query = query.Where(c => c.Categories.Any(cat => settings.Categories.Contains(cat.Id)));
                }
                if (settings.ReleaseYear != null)
                {
                    if (settings.ReleaseYear.Min.HasValue)
                        query = query.Where(c => c.ReleaseYear >= settings.ReleaseYear.Min);
                    if (settings.ReleaseYear.Max.HasValue)
                        query = query.Where(c => c.ReleaseYear <= settings.ReleaseYear.Max);
                }
                if (settings.ImdbVotes != null)
                {
                    if (settings.ImdbVotes.Min.HasValue)
                        query = query.Where(c => c.ImdbVotes >= settings.ImdbVotes.Min);
                    if (settings.ImdbVotes.Max.HasValue)
                        query = query.Where(c => c.ImdbVotes <= settings.ImdbVotes.Max);
                }
                if (settings.ImdbScore != null)
                {
                    if (settings.ImdbScore.Min.HasValue)
                        query = query.Where(c => c.ImdbScore >= settings.ImdbScore.Min);
                    if (settings.ImdbScore.Max.HasValue)
                        query = query.Where(c => c.ImdbScore <= settings.ImdbScore.Max);
                }
                if (settings.TmdbPopularity != null)
                {
                    if (settings.TmdbPopularity.Min.HasValue)
                        query = query.Where(c => c.TmdbPopularity >= settings.TmdbPopularity.Min);
                    if (settings.TmdbPopularity.Max.HasValue)
                        query = query.Where(c => c.TmdbPopularity <= settings.TmdbPopularity.Max);
                }
                if (settings.TmdbScore != null)
                {
                    if (settings.TmdbScore.Min.HasValue)
                        query = query.Where(c => c.TmdbScore >= settings.TmdbScore.Min);
                    if (settings.TmdbScore.Max.HasValue)
                        query = query.Where(c => c.TmdbScore <= settings.TmdbScore.Max);
                }
                if (settings.TomatoMeter != null)
                {
                    if (settings.TomatoMeter.Min.HasValue)
                        query = query.Where(c => c.TomatoMeter >= settings.TomatoMeter.Min);
                    if (settings.TomatoMeter.Max.HasValue)
                        query = query.Where(c => c.TomatoMeter <= settings.TomatoMeter.Max);
                }
                if (settings.JustWatchRating != null)
                {
                    if (settings.JustWatchRating.Min.HasValue)
                        query = query.Where(c => c.JustWatchRating >= settings.JustWatchRating.Min);
                    if (settings.JustWatchRating.Max.HasValue)
                        query = query.Where(c => c.JustWatchRating <= settings.JustWatchRating.Max);
                }
            }
            return query;
        }
    }
}
