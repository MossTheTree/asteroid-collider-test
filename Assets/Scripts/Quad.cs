using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad
{

    // Node size
    public Vector2Int bottomLeftPix;
    public Vector2Int topRightPix;

    // Node characteristics
    public bool isLeaf;
    public bool hasCollider;
    public string nodeState;
    public int nodeDepth;
    public BoxCollider2D nodeCollider;

    // Node position
    public bool isBottomLeft;
    public bool isBottomRight;
    public bool isTopLeft;
    public bool isTopRight;

    // Child nodes
    public Quad topLeft;
    public Quad topRight;
    public Quad bottomLeft;
    public Quad bottomRight;

    // Node construction
    public Quad(Vector2Int first, Vector2Int last)
    {
        bottomLeftPix = first;
        topRightPix = last;
        isLeaf = false;
        hasCollider = false;
        nodeCollider = new BoxCollider2D();
    }

}
