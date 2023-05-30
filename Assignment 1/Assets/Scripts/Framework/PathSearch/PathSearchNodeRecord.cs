using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PathSearchNodeRecord : IComparable<PathSearchNodeRecord>
{
    public int NodeIndex { get; set; }
    public int FromNodeIndex { get; set; }
    public float CostSoFar { get; set; }
    public float EstimatedTotalCost { get; set; }


    public PathSearchNodeRecord(int nodeIndex, int fromNodeIndex, float costSoFar, float estimatedTotalCost )
    {
        NodeIndex = nodeIndex;
        FromNodeIndex = fromNodeIndex;
        CostSoFar = costSoFar;
        EstimatedTotalCost = estimatedTotalCost;
    }

    public PathSearchNodeRecord(int nodeIndex, int fromNodeIndex):
        this(nodeIndex, fromNodeIndex, 0f, 0f)
    {    
    }

    public PathSearchNodeRecord(int nodeIndex) :
    this(nodeIndex, -1)
    {
    }

    public int CompareTo(PathSearchNodeRecord other)
    {
        return NodeIndex.CompareTo(other.NodeIndex);
    }
}
