using System.ComponentModel.DataAnnotations;

namespace SmartX.Web.Models;

public class SensorRegistrationViewModel
{
    [Required(ErrorMessage = "MAC address / unique identifier is required.")]
    [Display(Name = "Device MAC address / unique identifier")]
    public string MacAddress { get; set; } = "";

    [Required(ErrorMessage = "Deployment location is required.")]
    [Display(Name = "Deployment location")]
    public string DeploymentLocation { get; set; } = "Root/Facility A/Zone 1/Sub-Zone B";

    public SensorCategory Category { get; set; } = SensorCategory.Environmental;

    public TelemetryValueKind ValueKind { get; set; } = TelemetryValueKind.Float;

    [Required]
    public string Unit { get; set; } = "%";

    [Display(Name = "Safe range — min")]
    public double SafeMin { get; set; } = 30;

    [Display(Name = "Safe range — max")]
    public double SafeMax { get; set; } = 70;
}

public class DeploymentValidateViewModel
{
    [Required(ErrorMessage = "Enter a path to validate.")]
    public string Path { get; set; } = "Root/Facility A/Zone 1/Sub-Zone B";

    public DeploymentValidationResult? Result { get; set; }
}

public class AggregateViewModel
{
    public List<SensorSummaryDto> Sensors { get; set; } = new();

    [Display(Name = "Sensor A")]
    public Guid? SensorAId { get; set; }

    [Display(Name = "Sensor B")]
    public Guid? SensorBId { get; set; }

    public string Operation { get; set; } = "add";

    public AggregateResult? Result { get; set; }

    public string? Error { get; set; }
}

public class SensorsIndexViewModel
{
    public List<SensorSummaryDto> Sensors { get; set; } = new();
    public SensorRegistrationViewModel Form { get; set; } = new();
    public string? RegisterError { get; set; }
    public bool RegisterSuccess { get; set; }
}

public class DashboardViewModel
{
    public List<SensorSummaryDto> Sensors { get; set; } = new();
    public DashboardStats? Stats { get; set; }
}
