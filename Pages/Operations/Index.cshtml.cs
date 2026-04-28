using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WikiKnowledge.Pages.API
{
    public class IndexModel : PageModel
    {
        public class WikipediaSummary
        {
            public string Title { get; set; }
            public string Extract { get; set; }
        }

        public class LanguageLink
        {
            [JsonPropertyName("code")]
            public string Code { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }
        }

        // Exposed to Razor to render the results
        public WikipediaSummary Summary { get; set; }

        // Bound to the textbox on the page
        [BindProperty]
        public string Query { get; set; }

        // Bound to the selected language code from the combobox
        [BindProperty]
        public string SelectedLanguageCode { get; set; }

        // Bound to the selected page title in the selected language (separate from Query)
        [BindProperty]
        public string SelectedLanguageTitle { get; set; }

        // Fetches the summary from a specific language wiki (default "de")
        static async Task<string> GetWikipediaSummaryAsync(string title, string language)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            var domain = language;
            string apiUrl = $"https://{Uri.EscapeDataString(language)}.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(title)}";
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

        // Return structured language links (code, localized title, url)
        static async Task<List<LanguageLink>> GetWikipediaLanguagesAsync(string title, string language)
        {
            var result = new List<LanguageLink>();
            if (string.IsNullOrWhiteSpace(title))
                return result;

            string apiUrl = $"https://{Uri.EscapeDataString(language)}.wikipedia.org/w/api.php?action=query&prop=langlinks&titles={Uri.EscapeDataString(title)}&lllimit=500&format=json";

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
                            string code = link.TryGetProperty("lang", out JsonElement langEl) ? langEl.GetString() : null;
                            string localized = link.TryGetProperty("*", out JsonElement titleEl) ? titleEl.GetString() : null;
                            if (!string.IsNullOrEmpty(code))
                            {
                                var url = !string.IsNullOrEmpty(localized)
                                    ? $"https://{code}.wikipedia.org/wiki/{Uri.EscapeDataString(localized.Replace(' ', '_'))}"
                                    : $"https://{code}.wikipedia.org";
                                result.Add(new LanguageLink { Code = code, Title = localized ?? code, Url = url });
                            }
                        }
                    }
                }
            }

            return result;
        }

        // AJAX handler for the combobox; returns JSON array of { code, title, url }
        public async Task<IActionResult> OnGetLanguagesAsync(string term)
        {
            var title = string.IsNullOrWhiteSpace(term) ? Query ?? "Deutschland" : Uri.UnescapeDataString(term);
            var langs = await GetWikipediaLanguagesAsync(title, SelectedLanguageCode??"de");
            return new JsonResult(langs);
        }

        // Handle the form POST; if a language title was selected, use that title on the selected language wiki.
        public async Task<IActionResult> OnPostAsync()
        {
            var title = SelectedLanguageTitle.Trim();
            var lang = SelectedLanguageCode.Trim();
            var extract = await GetWikipediaSummaryAsync(title, lang);

            Summary = new WikipediaSummary
            {
                Title = title,
                Extract = extract
            };

            return Page();
        }
    }
}
