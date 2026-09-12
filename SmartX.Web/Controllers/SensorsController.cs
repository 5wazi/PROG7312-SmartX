using Microsoft.AspNetCore.Mvc;
using SmartX.Web.Models;
using SmartX.Web.Services;

namespace SmartX.Web.Controllers;

public class SensorsController : Controller
{
    private readonly SmartXApiClient _api;

    public SensorsController(SmartXApiClient api)
    {
        _api = api;
    }

    // GET /Sensors
    public async Task<IActionResult> Index()
    {
        var model = new SensorsIndexViewModel
        {
            Sensors = await _api.GetSensorsAsync(),
            Form = new SensorRegistrationViewModel()
        };
        return View(model);
    }

    // POST /Sensors/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(SensorRegistrationViewModel form)
    {
        var model = new SensorsIndexViewModel { Form = form };

        if (!ModelState.IsValid)
        {
            model.Sensors = await _api.GetSensorsAsync();
            return View("Index", model);
        }

        var request = new SensorRegistrationRequest(
            form.MacAddress, form.DeploymentLocation, form.Category,
            form.ValueKind, form.Unit, form.SafeMin, form.SafeMax);

        var (success, error) = await _api.RegisterSensorAsync(request);

        model.Sensors = await _api.GetSensorsAsync();
        model.RegisterSuccess = success;
        model.RegisterError = success ? null : error;

        if (success)
        {
            // Reset the form on success but keep the freshly loaded sensor list.
            model.Form = new SensorRegistrationViewModel();
        }

        return View("Index", model);
    }

    // POST /Sensors/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _api.DeleteSensorAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // POST /Sensors/UploadAttachment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(Guid sensorId, IFormFile? file)
    {
        if (file is not null && file.Length > 0)
        {
            await using var stream = file.OpenReadStream();
            await _api.UploadAttachmentAsync(sensorId, stream, file.FileName, file.ContentType);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET /Sensors/List — JSON, used by site.js to auto-refresh the table without a full page reload
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var sensors = await _api.GetSensorsAsync();
        return Json(sensors);
    }
}
