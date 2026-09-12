using SmartX.Api.Models;

namespace SmartX.Api.Services;

/// <summary>
/// Holds the (mock) multi-tier facility deployment tree that sensor
/// registration paths are recursively validated against.
/// </summary>
public class DeploymentTreeService
{
    public DeploymentNode Root { get; }

    public DeploymentTreeService()
    {
        // A synthetic root lets Facility A and Facility B share one tree so the
        // recursive validator has real depth (Root -> Facility -> Zone -> Sub-Zone)
        // to walk, matching the brief's "Sub-Zone B -> Zone 1 -> Facility A" example.
        Root = new DeploymentNode("Root",
            new DeploymentNode("Facility A",
                new DeploymentNode("Zone 1",
                    new DeploymentNode("Sub-Zone A"),
                    new DeploymentNode("Sub-Zone B"),
                    new DeploymentNode("Sub-Zone C")),
                new DeploymentNode("Zone 2",
                    new DeploymentNode("Sub-Zone A"),
                    new DeploymentNode("Sub-Zone B")),
                new DeploymentNode("Zone 3",
                    new DeploymentNode("Greenhouse 1"),
                    new DeploymentNode("Greenhouse 2"))),
            new DeploymentNode("Facility B",
                new DeploymentNode("Zone 1",
                    new DeploymentNode("Sub-Zone A"),
                    new DeploymentNode("Sub-Zone B"))));
    }
}
