using Microsoft.AspNetCore.Mvc;
using SmartX.Web.Models;
using SmartX.Web.Services;

namespace SmartX.Web.Controllers;

public class DeploymentController : Controller
{
    private readonly SmartXApiClient _api;

    public DeploymentController(SmartXApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Tree"] = await _api.GetDeploymentTreeAsync();
        return View(new DeploymentValidateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(DeploymentValidateViewModel model)
    {
        ViewData["Tree"] = await _api.GetDeploymentTreeAsync();

        if (ModelState.IsValid)
        {
            model.Result = await _api.ValidateDeploymentPathAsync(model.Path);
        }

        return View("Index", model);
    }
}
