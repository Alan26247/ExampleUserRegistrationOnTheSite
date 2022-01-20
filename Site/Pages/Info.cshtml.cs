using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Site.Pages
{
    public class InfoModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Model Info { get; set; }

        public void OnGet()
        {
        }

        public class Model
        {
            public string Header { get; set; }
            public string Text { get; set; }
            public string RedirectPage { get; set; }
        }
    }
}
