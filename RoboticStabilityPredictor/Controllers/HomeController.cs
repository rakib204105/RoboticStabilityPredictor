using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoboticStabilityPredictor.Models;

namespace RoboticStabilityPredictor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            

            return View(); // Show landing page for guests
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuestLogin([FromServices] SignInManager<IdentityUser> signInManager, [FromServices] UserManager<IdentityUser> userManager)
        {
            var guestEmail = $"guest_{Guid.NewGuid()}@guest.com";
            var guestUser = new IdentityUser { UserName = guestEmail, Email = guestEmail };

            var result = await userManager.CreateAsync(guestUser);
            if (result.Succeeded)
            {
                await signInManager.SignInAsync(guestUser, isPersistent: false);
                return RedirectToAction("InputRobotType", "Robotic");
            }

            return RedirectToAction("Index");
        }

    }
}
