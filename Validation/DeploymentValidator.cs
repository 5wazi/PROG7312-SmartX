using SmartX.Api.Models;

namespace SmartX.Api.Validation;

/// <summary>
/// Validates that a sensor's deployment path (e.g. "Facility A/Zone 1/Sub-Zone B")
/// is a real, connected route through the multi-tier deployment tree, using a
/// recursive descent rather than a flat string lookup. This is intentional:
/// the deployment tree can be arbitrarily deep (Facility -> Zone -> Sub-Zone ->
/// Rack -> Shelf, ...), and a recursive walk naturally handles any depth without
/// bespoke code for each tier.
/// </summary>
public static class DeploymentValidator
{
    public const char PathSeparator = '/';

    /// <summary>
    /// Recursively verifies that <paramref name="segments"/> describes an unbroken
    /// path from <paramref name="node"/> down through its children.
    /// Base case: no more segments to match -> the path so far is valid.
    /// Recursive case: the current node matches the head segment, so recurse into
    /// each child looking for a match on the remaining tail.
    /// </summary>
    public static bool IsValidPath(DeploymentNode node, IReadOnlyList<string> segments, int depth = 0)
    {
        // Base case 1: nothing left to validate at this node — the whole path matched.
        if (depth >= segments.Count)
        {
            return true;
        }

        // The current node must match the segment at this depth.
        if (!string.Equals(node.Name, segments[depth], StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Base case 2: this was the last segment and it matched this node — done.
        if (depth == segments.Count - 1)
        {
            return true;
        }

        // Recursive case: try to match the remaining tail against each child.
        foreach (var child in node.Children)
        {
            if (IsValidPath(child, segments, depth + 1))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Splits a "Facility A/Zone 1/Sub-Zone B" style path into ordered segments.</summary>
    public static List<string> ParsePath(string path) =>
        path.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

    /// <summary>
    /// Validates a raw path string against the tree, returning a human-readable
    /// result for the API/UI layer.
    /// </summary>
    public static DeploymentValidationResult Validate(DeploymentNode root, string rawPath)
    {
        var segments = ParsePath(rawPath);
        if (segments.Count == 0)
        {
            return new DeploymentValidationResult(false, rawPath, "Path is empty.");
        }

        var isValid = IsValidPath(root, segments);
        var message = isValid
            ? $"'{rawPath}' is a valid route through the deployment tree."
            : $"'{rawPath}' does not match any configured facility/zone/sub-zone chain.";

        return new DeploymentValidationResult(isValid, rawPath, message);
    }
}
