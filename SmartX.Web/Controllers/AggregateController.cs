using Microsoft.AspNetCore.Mvc;
using SmartX.Web.Models;
using SmartX.Web.Services;

namespace SmartX.Web.Controllers;

public class AggregateController : Controller
{
    private readonly SmartXApiClient _api;

    public AggregateController(SmartXApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AggregateViewModel { Sensors = await _api.GetSensorsAsync() };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Compute(AggregateViewModel model)
    {
        model.Sensors = await _api.GetSensorsAsync();

        if (model.SensorAId is null || model.SensorBId is null)
        {
            model.Error = "Pick both sensors first.";
            return View("Index", model);
        }

        var result = await _api.AggregateAsync(model.SensorAId.Value, model.SensorBId.Value, model.Operation);
        if (result is null)
        {
            model.Error = "The API rejected this combination — the two sensors probably don't share a unit.";
        }
        else
        {
            model.Result = result;
        }

        return View("Index", model);
    }
}
