using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AymanFreelance.PL.Controllers
{
    [Authorize(Roles = "SponsorClient")]
    public class SponsorClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
