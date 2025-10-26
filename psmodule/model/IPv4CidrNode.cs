using System.Collections.Generic;
using System.Linq;

namespace Worker369.Utility;

public class IPv4CidrNode
{
    private readonly IPv4Subnet _subnet;
    private bool _mapped;

    public IPv4Subnet CIDR     => _subnet;
    public bool       IsMapped => _mapped;

    public IPv4CidrNode(IPv4Subnet subnet)
    {
        _subnet = subnet;
        _mapped = false;
    }

    public override string ToString() => _subnet.ToString();

    public IEnumerable<IPv4CidrNode> MapSubnets(IPv4Subnet[] subnets)
    {
        if (subnets is null || subnets.Length == 0) yield return this;

        else foreach (var subnet in Map(subnets.Distinct().ToArray())) yield return subnet;
    }

    private IEnumerable<IPv4CidrNode> Map(IPv4Subnet[] subnets)
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
        var children     = IPv4Subnet.Split(_subnet, 1).ToArray();
        var left_subnet  = children[0];
        var right_subnet = children[1];

        // Find subnets that overlaps with the left child.
        var left_overlaps  = subnets.Where(s => IPv4Subnet.IsOverlapping(left_subnet, s)).ToArray();

        // Find subnets that overlaps with the right child.
        var right_overlaps = subnets.Where(s => IPv4Subnet.IsOverlapping(right_subnet, s)).ToArray();

        // Recursively map subnets that overlaps with the left child - Depth First Search.
        if (left_overlaps.Length > 0)
        {
            var left_node = new IPv4CidrNode(left_subnet);
            foreach (var subnet in left_node.Map(left_overlaps)) yield return subnet;
        }

        // No subnets overlap with the left child - Report the left child as unmapped node.
        if (left_overlaps.Length == 0)
        {
            var left_node = new IPv4CidrNode(left_subnet);
            yield return left_node;
        }

        // Resursively map subnets that overlaps with the right child - Depth First Search.
        if (right_overlaps.Length > 0)
        {
            var right_node = new IPv4CidrNode(right_subnet);
            foreach (var subnet in right_node.Map(right_overlaps)) yield return subnet;
        }

        // No subnets overlap with the right child - Report the right child as unmapped node.
        if (right_overlaps.Length == 0)
        {
            var right_node = new IPv4CidrNode(right_subnet);
            yield return right_node;
        }
    }
}

/*
                            CIDR     FirstIP        LastIP SubnetSize IsMapped
-------------------------------- ----------- ------------- ---------- --------
          - 10.10.0.0/21           10.10.0.0   10.10.7.255      2,048    False
-------------------------------- ----------- ------------- ---------- --------
              - 10.10.8.0/23       10.10.8.0   10.10.9.255        512    False
-------------------------------- ----------- ------------- ---------- --------
                - 10.10.10.0/24   10.10.10.0  10.10.10.255        256     True
-------------------------------- ----------- ------------- ---------- --------
                - 10.10.11.0/24   10.10.11.0  10.10.11.255        256    False
-------------------------------- ----------- ------------- ---------- --------
            - 10.10.12.0/22       10.10.12.0  10.10.15.255      1,024    False
-------------------------------- ----------- ------------- ---------- --------
            - 10.10.16.0/22       10.10.16.0  10.10.19.255      1,024    False
-------------------------------- ----------- ------------- ---------- --------
                - 10.10.20.0/24   10.10.20.0  10.10.20.255        256     True
-------------------------------- ----------- ------------- ---------- --------
                - 10.10.21.0/24   10.10.21.0  10.10.21.255        256    False
-------------------------------- ----------- ------------- ---------- --------
              - 10.10.22.0/23     10.10.22.0  10.10.23.255        512    False
-------------------------------- ----------- ------------- ---------- --------
            - 10.10.24.0/22       10.10.24.0  10.10.27.255      1,024    False
-------------------------------- ----------- ------------- ---------- --------
              - 10.10.28.0/23     10.10.28.0  10.10.29.255        512    False
-------------------------------- ----------- ------------- ---------- --------
                - 10.10.30.0/24   10.10.30.0  10.10.30.255        256     True
-------------------------------- ----------- ------------- ---------- --------
                - 10.10.31.0/24   10.10.31.0  10.10.31.255        256    False
-------------------------------- ----------- ------------- ---------- --------
      - 10.10.32.0/19             10.10.32.0  10.10.63.255      8,192    False
-------------------------------- ----------- ------------- ---------- --------
    - 10.10.64.0/18               10.10.64.0 10.10.127.255     16,384    False
-------------------------------- ----------- ------------- ---------- --------
    - 10.10.128.0/18             10.10.128.0 10.10.191.255     16,384    False
-------------------------------- ----------- ------------- ---------- --------
          - 10.10.192.0/21       10.10.192.0 10.10.199.255      2,048    False
-------------------------------- ----------- ------------- ---------- --------
                - 10.10.200.0/24 10.10.200.0 10.10.200.255        256     True
-------------------------------- ----------- ------------- ---------- --------
                - 10.10.201.0/24 10.10.201.0 10.10.201.255        256    False
-------------------------------- ----------- ------------- ---------- --------
              - 10.10.202.0/23   10.10.202.0 10.10.203.255        512    False
-------------------------------- ----------- ------------- ---------- --------
            - 10.10.204.0/22     10.10.204.0 10.10.207.255      1,024    False
-------------------------------- ----------- ------------- ---------- --------
        - 10.10.208.0/20         10.10.208.0 10.10.223.255      4,096    False
-------------------------------- ----------- ------------- ---------- --------
      - 10.10.224.0/19           10.10.224.0 10.10.255.255      8,192    False
-------------------------------- ----------- ------------- ---------- --------
*/