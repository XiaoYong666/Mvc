using Microsoft.AspNetCore.Mvc;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class NoDiagnosticsAreReturned_ForNonApiController : Controller
    {
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult Method(int id)
        {
            if (id == 0)
            {
                /*MM*/
                return NotFound();
            }

            return Ok();
        }
    }
}
