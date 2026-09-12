namespace SmartX.Api.Models;

/// <summary>
/// A single node in a nested device-deployment tree, e.g.
/// Facility A -&gt; Zone 1 -&gt; Sub-Zone B. Sensors are registered against a
/// dot/arrow-separated path through this tree, and that path is verified with
/// a recursive descent (see Validation/DeploymentValidator.cs) before the
/// sensor profile is accepted.
/// </summary>
public class DeploymentNode
{
    public string Name { get; set; } = string.Empty;
    public List<DeploymentNode> Children { get; set; } = new();

    public DeploymentNode() { }

    public DeploymentNode(string name, params DeploymentNode[] children)
    {
        Name = name;
        Children = children.ToList();
    }
}
