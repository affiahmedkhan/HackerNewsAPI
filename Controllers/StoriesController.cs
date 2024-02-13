using HackerNewsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HackerNewsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoriesController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;

        public StoriesController(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        [HttpGet("{n}")]
        public async Task<ActionResult<IEnumerable<Story>>> GetTopStories(int n)
        {
            if (n <= 0)
            {
                return BadRequest("The number of stories requested must be greater than 0.");
            }

            string cacheKey = $"top_{n}_stories";
            if (!_cache.TryGetValue(cacheKey, out List<Story> cachedStories))
            {
                var httpClient = _httpClientFactory.CreateClient();
                string bestStoriesIdsResponse;
                try
                {
                    bestStoriesIdsResponse = await httpClient.GetStringAsync("https://hacker-news.firebaseio.com/v0/beststories.json");
                }
                catch (HttpRequestException)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "Unable to retrieve data from Hacker News API.");
                }

                if (string.IsNullOrEmpty(bestStoriesIdsResponse))
                {
                    return NotFound("Could not retrieve stories from Hacker News.");
                }

                List<int> bestStoriesIds;
                try
                {
                    bestStoriesIds = JsonSerializer.Deserialize<List<int>>(bestStoriesIdsResponse);
                }
                catch (JsonException)
                {
                    return BadRequest("Error parsing the stories list from Hacker News.");
                }

                if (bestStoriesIds == null)
                {
                    return NotFound("No best stories found.");
                }

                var fetchTasks = bestStoriesIds.Take(n).Select(async id =>
                {
                    try
                    {
                        var storyResponse = await httpClient.GetStringAsync($"https://hacker-news.firebaseio.com/v0/item/{id}.json");
                        if (!string.IsNullOrEmpty(storyResponse))
                        {
                            var jsonElement = JsonSerializer.Deserialize<JsonElement>(storyResponse);
                            return new Story
                            {
                                PostedBy = jsonElement.GetProperty("by").GetString(),
                                Score = jsonElement.GetProperty("score").GetInt32(),
                                Title = jsonElement.GetProperty("title").GetString(),
                                Uri = jsonElement.TryGetProperty("url", out JsonElement url) ? url.GetString() : null,
                                CommentCount = jsonElement.TryGetProperty("descendants", out JsonElement descendants) ? descendants.GetInt32() : 0,
                                Time = Story.UnixTimeStampToDateTime(jsonElement.GetProperty("time").GetDouble())
                            };
                        }
                    }
                    catch
                    {
                    }
                    return null;
                });

                var stories = await Task.WhenAll(fetchTasks);
                var returnableStories = stories.Where(story => story != null).OrderByDescending(story => story.Score).ToList();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)); 
                _cache.Set(cacheKey, returnableStories, cacheEntryOptions);
                return returnableStories;
            }

            return cachedStories;
        }
    }
}
