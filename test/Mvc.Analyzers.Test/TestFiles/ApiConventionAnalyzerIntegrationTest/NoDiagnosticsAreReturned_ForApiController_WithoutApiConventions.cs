namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [ApiController]
    public class NoDiagnosticsAreReturned_ForApiController_WithoutApiConventions : ControllerBase
    {
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult Method(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
