using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [ApiController]
    public class DiagnosticsAreReturned_ForActionResultOfTReturningMethodWithoutAnyAttributes : ControllerBase
    {
        public ActionResult<string> Method(Guid? id)
        {
            if (id == null)
            {
                /*MM*/return NotFound();
            }

            return "Hello world";
        }
    }
}
