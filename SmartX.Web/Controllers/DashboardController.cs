using Microsoft.AspNetCore.Mvc;
using SmartX.Web.Models;
using SmartX.Web.Services;

namespace SmartX.Web.Controllers;

public class DashboardController : Controller
{
    private readonly SmartXApiClient _api;

    public DashboardController(SmartXApiClient api)
    {
        _api = api;
    }

    // GET /Dashboard — first paint is server-rendered like any classic MVC view
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            Sensors = await _api.GetSensorsAsync(),
            Stats = await _api.GetStatsAsync()
        };
        return View(model);
    }

    // GET /Dashboard/LiveData — polled every couple of seconds by site.js so the
    // sparklines and health streaks feel live without a full page refresh.
    [HttpGet]
    public async Task<IActionResult> LiveData()
    {
        var sensors = await _api.GetSensorsAsync();
        var stats = await _api.GetStatsAsync();
        return Json(new { sensors, stats });
    }
}
