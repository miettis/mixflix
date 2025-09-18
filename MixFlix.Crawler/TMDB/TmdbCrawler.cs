using MixFlix.Data;

namespace MixFlix.Crawler.TMDB
{
    public class TmdbCrawler
    {
        private readonly string accessToken = "";

        public async Task Crawl(int providerId, ContentType contentType)
        {
            var type = contentType == ContentType.Movie ? "movie" : "tv";


            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var page = providerId == 2 && contentType == ContentType.Movie ? 229 : 1;
            while (true)
            {
                var url = $"https://api.themoviedb.org/3/discover/{type}?include_adult=false&include_video=false&language=fi-FI&page={page}&sort_by=popularity.desc&watch_region=FI&with_watch_monetization_types=flatrate&with_watch_providers={providerId}";

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch data: {response.StatusCode}");
                    break;
                }
                var json = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonSerializer.Deserialize<DiscoverResponse>(json);

                var path = $"tmdb\\{type}_{providerId}_{page}.json";
                File.WriteAllText(path, json);


                if (data.TotalPages == null || data.TotalPages == page || data.Results == null || data.Results.Length == 0)
                {
                    Console.WriteLine("No results found or reached the end of pages.");
                    break;
                }

                page++;
                await Task.Delay(1000); // To avoid hitting API rate limits
            }
        }

        public async Task<DetailsWithCreditsResponse> CrawlDetails(ContentType type, string id)
        {
            var typeName = type == ContentType.Movie ? "movie" : "tv";
            var url = $"https://api.themoviedb.org/3/{typeName}/{id}?append_to_response=credits&language=fi-FI";
            var client = GetClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch details for {typeName} with ID {id}: {response.StatusCode}");
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            var path = $"tmdb_details\\{typeName}_{id}_details.json";
            File.WriteAllText(path, json);
            var data = System.Text.Json.JsonSerializer.Deserialize<DetailsWithCreditsResponse>(json);
            return data;
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return client;
        }
    }
}
