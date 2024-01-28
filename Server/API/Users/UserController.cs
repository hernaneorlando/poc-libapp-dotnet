using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Users
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
