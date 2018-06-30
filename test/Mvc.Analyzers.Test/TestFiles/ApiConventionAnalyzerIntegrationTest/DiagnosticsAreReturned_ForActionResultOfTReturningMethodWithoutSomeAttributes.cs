using Microsoft.AspNetCore.Mvc;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [ApiController]
    public class DiagnosticsAreReturned_ForActionResultOfTReturningMethodWithoutSomeAttributes : ControllerBase
    {
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult Put(int id, object model)
        {
            if (id == 0)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                /*MM*/return UnprocessableEntity();
            }

            return Ok();
        }
    }
}
