using System.Text.Json;
using WikiKnowledge.WikipediaServices;

namespace WikiKnowledge.CallServices
{
    public class Services
    {
        public async Task<string> GetWikipediaSummaryAsync(string title, string language)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            var domain = language;
            string apiUrl = $"https://{language}.wikipedia.org/api/rest_v1/page/summary/{title}";
            //https://{Uri.EscapeDataString(language)}.wikipedia.org/w/api.php?action=query&titles={Uri.EscapeDataString(title)}&prop=extracts&exintro=true&format=json
            //https://{Uri.EscapeDataString(language)}.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(title)}

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpWikipediaClient/1.0");

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return null; // Could be 404 if article not found
            }

            string json = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("extract", out JsonElement extractElement))
            {
                return extractElement.GetString();
            }

            return null;
        }

        public async Task<List<LanguageLink>> GetWikipediaLanguagesAsync(string title, string language)
        {
            var result = new List<LanguageLink>();
            if (string.IsNullOrWhiteSpace(title))
                return result;

            string apiUrl = $"https://{language}.wikipedia.org/w/api.php?action=query&prop=langlinks&titles={title}&lllimit=500&format=json";

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpWikipediaClient/1.0");

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
                return result;

            string json = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("query", out JsonElement queryElement) &&
                queryElement.TryGetProperty("pages", out JsonElement pagesElement))
            {
                foreach (JsonProperty pageProp in pagesElement.EnumerateObject())
                {
                    JsonElement page = pageProp.Value;
                    if (page.TryGetProperty("langlinks", out JsonElement langlinks))
                    {
                        foreach (JsonElement link in langlinks.EnumerateArray())
                        {
                            string langcode = link.TryGetProperty("lang", out JsonElement langEl) ? langEl.GetString() : null; //https://de.wikipedia.org/w/api.php?action=query&prop=langlinks&titles=deutschland&lllimit=500&format=json 
                            string localizedarticlename = link.TryGetProperty("*", out JsonElement titleEl) ? titleEl.GetString() : null; //condition ? value_if_true : value_if_false
                            if (!string.IsNullOrEmpty(langcode)) // if string ("en") IsNotNull then execute
                            {
                                var url = !string.IsNullOrEmpty(localizedarticlename)
                                    ? $"https://{langcode}.wikipedia.org/wiki/{localizedarticlename.Replace(" ", "")}"
                                    : $"https://{langcode}.wikipedia.org";
                                result.Add(new LanguageLink { Code = langcode, Title = localizedarticlename, Url = url });
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
