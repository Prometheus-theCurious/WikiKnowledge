using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WikiKnowledge.CallServices;
using WikiKnowledge.WikipediaServices;

namespace WikiKnowledge.Pages.Operations
{
    public class IndexModel : PageModel
    {
        private readonly Services _wikipediaServiceCalls;

        public IndexModel(Services wikipediaServiceCalls)
        {
            _wikipediaServiceCalls = wikipediaServiceCalls;
        }

        // Exposed to Razor to render the results
        [BindProperty]
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

        public async Task<IActionResult> OnGetLanguagesAsync(string term)
        {
            var title = Uri.UnescapeDataString(term)??Query; 
            var langs = await _wikipediaServiceCalls.GetWikipediaLanguagesAsync(title, "en");
            return new JsonResult(langs);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var title = SelectedLanguageTitle.Trim();
            var lang = SelectedLanguageCode.Trim();
            var extract = await _wikipediaServiceCalls.GetWikipediaSummaryAsync(title, lang);

            Summary = new WikipediaSummary
            {
                Title = title,
                Extract = extract
            };

            return Page();
        }
    }
}
