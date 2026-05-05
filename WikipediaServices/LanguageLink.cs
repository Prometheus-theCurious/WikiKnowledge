using System.Text.Json.Serialization;

namespace WikiKnowledge.WikipediaServices
{
    public class LanguageLink
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
