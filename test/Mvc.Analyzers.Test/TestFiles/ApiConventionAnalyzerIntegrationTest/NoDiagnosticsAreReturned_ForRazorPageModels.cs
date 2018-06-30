using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class Home : PageModel
    {
        public IActionResult OnPost(int id)
        {
            if (id == 0)
            {
                /*MM*/
                return NotFound();
            }

            return Page();
        }
    }
}
