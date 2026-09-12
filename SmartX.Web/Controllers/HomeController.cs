using Microsoft.AspNetCore.Mvc;

namespace SmartX.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
