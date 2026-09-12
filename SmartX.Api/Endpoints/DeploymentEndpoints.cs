using SmartX.Api.Models;
using SmartX.Api.Services;
using SmartX.Api.Validation;

namespace SmartX.Api.Endpoints;

public static class DeploymentEndpoints
{
    public static void MapDeploymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/deployment").WithTags("Deployment");

        group.MapGet("/tree", (DeploymentTreeService tree) => Results.Ok(tree.Root));

        group.MapPost("/validate", (DeploymentValidationRequest request, DeploymentTreeService tree) =>
        {
            var result = DeploymentValidator.Validate(tree.Root, request.Path);
            return Results.Ok(result);
        });
    }
}
