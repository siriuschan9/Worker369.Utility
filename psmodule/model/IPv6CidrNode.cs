using System.Collections.Generic;
using System.Linq;

namespace Worker369.Utility;

public class IPv6CidrNode
{
    private readonly IPv6Subnet _subnet;
    private bool _mapped;

    public IPv6Subnet CIDR => _subnet;

    public bool IsMapped => _mapped;

    public IPv6CidrNode(IPv6Subnet subnet)
    {
        _subnet = subnet;
        _mapped = false;
    }

    public override string ToString() => _subnet.ToString();

    public IEnumerable<IPv6CidrNode> MapSubnets(IPv6Subnet[] subnets)
    {
        if (subnets is null || subnets.Length == 0) yield return this;

        else foreach (var subnet in Map(subnets.Distinct().ToArray())) yield return subnet;
    }

    private IEnumerable<IPv6CidrNode> Map(IPv6Subnet[] subnets)
    {
        if (subnets.Length == 1 && subnets[0] == _subnet) // Mapped node - Works if subnets have no overlaps.
        {
            _mapped = true;
            yield return this;
            yield break;
        }

        if (subnets.Contains(_subnet))                    // Added to take care of cases where subnets have overlaps.
        {
            _mapped = true;
            yield return this;
            yield break;
        }

        // Check if this is a leaf node.
        if (_subnet.PrefixLength == 32) yield break;

        // Not a leaf node - go one level down, like a binary tree.
        var children     = IPv6Subnet.Split(_subnet, 1).ToArray();
        var left_subnet  = children[0];
        var right_subnet = children[1];

        // Find subnets that overlaps with the left child.
        var left_overlaps  = subnets.Where(s => IPv6Subnet.IsOverlapping(left_subnet, s)).ToArray();

        // Find subnets that overlaps with the right child.
        var right_overlaps = subnets.Where(s => IPv6Subnet.IsOverlapping(right_subnet, s)).ToArray();

        // Recursively map subnets that overlaps with the left child - Depth First Search.
        if (left_overlaps.Length > 0)
        {
            var left_node = new IPv6CidrNode(left_subnet);
            foreach (var subnet in left_node.Map(left_overlaps)) yield return subnet;
        }

        // No subnets overlap with the left child - Report the left child as unmapped node.
        if (left_overlaps.Length == 0)
        {
            var left_node = new IPv6CidrNode(left_subnet);
            yield return left_node;
        }

        // Resursively map subnets that overlaps with the right child - Depth First Search.
        if (right_overlaps.Length > 0)
        {
            var right_node = new IPv6CidrNode(right_subnet);
            foreach (var subnet in right_node.Map(right_overlaps)) yield return subnet;
        }

        // No subnets overlap with the right child - Report the right child as unmapped node.
        if (right_overlaps.Length == 0)
        {
            var right_node = new IPv6CidrNode(right_subnet);
            yield return right_node;
        }
    }
}