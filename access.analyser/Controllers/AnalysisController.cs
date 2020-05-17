using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace access.analyser.Controllers
{
    [Authorize]
    public class AnalysisController : Controller
    {
        /// <summary>
        /// Allows the setup of analysis
        /// </summary>
        /// <returns>Index view</returns>
        public IActionResult Index () => View ();
    }
}