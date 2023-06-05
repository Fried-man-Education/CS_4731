using System.Collections.Generic;
using UnityEngine;


public delegate int GetNodeCount();

public delegate Vector2 GetNode(int nodeIndex);

public delegate List<int> GetNodeAdjacencies(int nodeIndex);