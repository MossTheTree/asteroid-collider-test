using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidBuilder : MonoBehaviour
{

    [SerializeField] Texture2D srcTexture;
    Texture2D asteroidTexture;
    SpriteRenderer asteroidSprite;
    List<Vector2Int> changedPixels;

    Quad rootNode;
    Quad nodeToChange;
    List<Quad> nodesToChange;

    float spriteWidthUnits, spriteHeightUnits;
    int spriteWidthPixels, spriteHeightPixels;

    private CustomCollider2D asteroidCollider;
    private PhysicsShapeGroup2D asteroidColliderGroup;

    void Start()
    {
        
        // Initalize our sprite and texture
        asteroidSprite = GetComponent<SpriteRenderer>();
        asteroidTexture = Instantiate(srcTexture);
        asteroidTexture.Apply();
        MakeSprite(asteroidSprite, asteroidTexture);
        changedPixels = new List<Vector2Int>();

        // Set the variables to manage pixel to world
        // coordinate transforms
        spriteWidthPixels = asteroidSprite.sprite.texture.width;
        spriteHeightPixels = asteroidSprite.sprite.texture.height;
        spriteWidthUnits = spriteWidthPixels / asteroidSprite.sprite.pixelsPerUnit;
        spriteHeightUnits = spriteHeightPixels / asteroidSprite.sprite.pixelsPerUnit;

        // Define the root node
        rootNode = new Quad(new Vector2Int(0,0),new Vector2Int(spriteWidthPixels,spriteHeightPixels));
        rootNode.nodeDepth = 1;
        nodeToChange = rootNode;
        nodesToChange = new List<Quad>();

        asteroidCollider = GetComponent<CustomCollider2D>();
        asteroidColliderGroup = new PhysicsShapeGroup2D(1000, 5000);

        // Built the quadtree
        BuildQuad(rootNode);

        // Build the collider
        BuildCollider(rootNode);
        asteroidCollider.SetCustomShapes(asteroidColliderGroup);
    }


    // QUADTREE

    void BuildQuad(Quad node) {
    // Build the quadtree, stopping along the way to set each node's status
    // in terms of pixels contained in the quad as empty, full, or mixed
        if (checkNodeState(node.bottomLeftPix, node.topRightPix) == "empty")
        {
            node.nodeState = "empty";
            node.isLeaf = true;
            return;
        } else if (checkNodeState(node.bottomLeftPix, node.topRightPix) == "full") {
            node.nodeState = "full";
            node.isLeaf = true;
            return;
        } else if (checkNodeState(node.bottomLeftPix, node.topRightPix) == "mixed") {
            node.isLeaf = false;
            node.nodeState = "mixed";
            SplitNode(node);
            BuildQuad(node.bottomLeft);
            BuildQuad(node.bottomRight);
            BuildQuad(node.topLeft);
            BuildQuad(node.topRight);
        }

    }

    private void SplitNode(Quad node)
    // Split a node into four child nodes, and set the node depth of each
    {
        // Figure out the width of the parent node
        int nodeWidth = (node.topRightPix.x - node.bottomLeftPix.x);
        int nodeHeight = (node.topRightPix.y - node.bottomLeftPix.y);
        
        // Create the four child nodes
        Vector2Int botCorner = node.bottomLeftPix;
        Vector2Int topCorner = node.topRightPix;
        node.bottomLeft = new Quad(new Vector2Int(botCorner.x,botCorner.y), new Vector2Int(botCorner.x + nodeWidth/2, botCorner.y + nodeHeight/2));
        node.bottomLeft.nodeDepth = node.nodeDepth * 2;
        node.bottomRight = new Quad(new Vector2Int(botCorner.x + nodeWidth/2,botCorner.y), new Vector2Int(botCorner.x + nodeWidth, botCorner.y + nodeHeight/2));
        node.bottomRight.nodeDepth = node.nodeDepth * 2;
        node.topLeft = new Quad(new Vector2Int(botCorner.x,botCorner.y + nodeHeight/2), new Vector2Int(botCorner.x + nodeWidth/2, botCorner.y + nodeHeight));
        node.topLeft.nodeDepth = node.nodeDepth * 2;
        node.topRight = new Quad(new Vector2Int(botCorner.x + nodeWidth/2,botCorner.y + nodeHeight/2), new Vector2Int(botCorner.x + nodeWidth, botCorner.y + nodeHeight));
        node.topRight.nodeDepth = node.nodeDepth * 2;
    }

    string checkNodeState(Vector2Int bottomLeft, Vector2Int topRight)
    // Determine whether a node is full (all pixels have color value), mixed (some do
    // and some don't), or empty (all are transparent)
    { 
        // Check the status of the first pixel, to then determine
        // whether the node is full, empty, or mixed
        bool firstPixIsEmpty;
        if (asteroidTexture.GetPixel(bottomLeft.x,bottomLeft.y).a > 0)
        {
            firstPixIsEmpty = false;
        } else {
            firstPixIsEmpty = true;
        }

        // If the first pixel is empty, and we find a non-empty
        // pixel, then return mixed. If we don't find any non-empty
        // pixels, then return  empty.
        if (firstPixIsEmpty) {
            for (int x = bottomLeft.x; x < topRight.x; x++) {
                for (int y = bottomLeft.y; y < topRight.y; y++) {
                    if (asteroidTexture.GetPixel(x,y).a > 0)
                    {
                        return "mixed";
                    }
                }
            }
            return "empty";
        } else {
        // And the opposite case, if the first pixel is non-empty, etc.
            for (int x = bottomLeft.x; x < topRight.x; x++) {
                for (int y = bottomLeft.y; y < topRight.y; y++) {
                    if (asteroidTexture.GetPixel(x,y).a == 0)
                    {
                        return "mixed";
                    }
                }
            }
            return "full";
        }
    }


    // COLLIDERS

    private void BuildCollider(Quad node) {
    // Traverse the quad from a given node and build colliders for
    // any leaf nodes that are visible
        if (node.nodeState == "empty") {
            return;
        } else if (node.nodeState == "full") {
            AddCollider(node);
        } else if (node.nodeState == "mixed") {
            BuildCollider(node.bottomLeft);
            BuildCollider(node.bottomRight);
            BuildCollider(node.topLeft);
            BuildCollider(node.topRight);
        }
    }

    private void AddCollider(Quad node) {
    // Add a collider that matches the
    // location and size of the node

        // Create the collider
        // BoxCollider2D nodeCollider = new BoxCollider2D();
        // nodeCollider = gameObject.AddComponent<BoxCollider2D>();

        // Determine and set size of collider
        int nodeWidthPixels = spriteWidthPixels / node.nodeDepth;
        float nodeWidthLocal = nodeWidthPixels * (spriteWidthUnits/spriteWidthPixels);
        // nodeCollider.size = new Vector2(nodeWidthLocal,nodeWidthLocal);
        Vector2 nodeColliderSize = new Vector2(nodeWidthLocal,nodeWidthLocal);
        
        // Determine and set midpoint of the collider
        Vector2 bottomLeftWorld = PixelToLocal(new Vector2Int(node.bottomLeftPix.x, node.bottomLeftPix.y));
        Vector2 topRightWorld = PixelToLocal(new Vector2Int(node.topRightPix.x, node.topRightPix.y));
        Vector2 midPoint = (topRightWorld + bottomLeftWorld) / 2;
        // nodeCollider.offset = midPoint;
        Vector2 nodeColliderOffset = midPoint;

        // Assign the collider to the node
        node.hasCollider = true;
        // node.nodeCollider = nodeCollider;
        node.shapeIndex = asteroidColliderGroup.AddBox(nodeColliderOffset, nodeColliderSize);
    }

    private void RemoveColliders(Quad node) {
    // Traverse the quad starting from a given node,
    // and remove any colliders that are associated with leaf nodes
        if (node.hasCollider && node.isLeaf) {
            // Destroy(node.nodeCollider);
            node.hasCollider = false;
        } else if (!node.isLeaf) {
            RemoveColliders(node.bottomLeft);
            RemoveColliders(node.bottomRight);
            RemoveColliders(node.topLeft);
            RemoveColliders(node.topRight);
        }
    }


    // TERRAIN DESTRUCTION

    void Mine(Vector3 mousePos, int radius)
    {        
        // Loop around the circle, starting from the center
        // and checking for pixels that are non-transparent.
        // If we find any, change them to transparent, and save
        // the pixel in the list.
        Vector2Int digCenter = LocalToPixel(mousePos);
        int px, nx, py, ny, distance;
        for (int i = 0; i < radius; i++)
        {
            distance = Mathf.RoundToInt(Mathf.Sqrt(radius * radius - i * i));
            for (int j = 0; j < distance; j++)
            {   
                px = digCenter.x + i;
                nx = digCenter.x - i;
                py = digCenter.y + j;
                ny = digCenter.y - j;

                if (asteroidTexture.GetPixel(px,py).a > 0) {
                    asteroidTexture.SetPixel(px,py, Color.clear);
                    changedPixels.Add(new Vector2Int(px,py));
                }
                if (asteroidTexture.GetPixel(px,ny).a > 0) {
                    asteroidTexture.SetPixel(px,ny, Color.clear);
                    changedPixels.Add(new Vector2Int(px,ny));
                }
                if (asteroidTexture.GetPixel(nx,py).a > 0) {
                    asteroidTexture.SetPixel(nx,py, Color.clear);
                    changedPixels.Add(new Vector2Int(nx,py));
                }
                if (asteroidTexture.GetPixel(nx,ny).a > 0) {
                    asteroidTexture.SetPixel(nx,ny, Color.clear);
                    changedPixels.Add(new Vector2Int(nx,ny));
                }
            }
        }

        // If there were more than zero changed pixels, then
        // figure out which node is home to each of those
        // changed pixels, and add those to nodesToChange
        // Then loop through nodesToChange, remove any existing
        // collider, build any new child nodes, and build any needed colliders
        // Then finally apply the sprite texture changes, and reset
        // the lists to get ready for the next update
        if (changedPixels.Count > 0) {
            foreach (Vector2Int pixel in changedPixels) {
                DoesNodeContain(pixel, rootNode);
            }
            foreach (Quad node in nodesToChange) {              
                asteroidColliderGroup.DeleteShape(node.shapeIndex);
                node.hasCollider = false;
                AdjustNodeShapeIndexes(ref rootNode, node.shapeIndex);
                BuildQuad(node);
                BuildCollider(node);
                asteroidCollider.SetCustomShapes(asteroidColliderGroup);
            }

            asteroidTexture.Apply();
            MakeSprite(asteroidSprite, asteroidTexture);
            nodesToChange.Clear();
            changedPixels.Clear();
        } 
    }

    void AdjustNodeShapeIndexes(ref Quad node, int aboveShapeIndex)
    {
        if (node.isLeaf)
        {
            if (node.hasCollider && node.shapeIndex > aboveShapeIndex)
            {
                node.shapeIndex--;
            }
            return;
        }
        AdjustNodeShapeIndexes(ref node.bottomLeft, aboveShapeIndex);
        AdjustNodeShapeIndexes(ref node.bottomRight, aboveShapeIndex);
        AdjustNodeShapeIndexes(ref node.topLeft, aboveShapeIndex);
        AdjustNodeShapeIndexes(ref node.topRight, aboveShapeIndex);
    }


    // Find what nodes contain a given pixel, then add them to nodesToChange
    void DoesNodeContain(Vector2Int thisPixel, Quad node) {
        if (    thisPixel.x >= node.bottomLeftPix.x &&
                thisPixel.x <= node.topRightPix.x &&
                thisPixel.y >= node.bottomLeftPix.y &&
                thisPixel.y <= node.topRightPix.y)
        {
            if (node.isLeaf) {
                if (!nodesToChange.Contains(node)) nodesToChange.Add(node);
            } else {
                DoesNodeContain(thisPixel, node.bottomLeft);
                DoesNodeContain(thisPixel, node.bottomRight);
                DoesNodeContain(thisPixel, node.topLeft);
                DoesNodeContain(thisPixel, node.topRight);
            }
        }        
    }



    // UTILITY METHODS

    void MakeSprite(SpriteRenderer spriteRenderer, Texture2D texture)
    // Update the sprite if changes have been made to it
    {
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0,0,texture.width, texture.height), Vector2.one * 0.5f,asteroidSprite.sprite.pixelsPerUnit);
    }

    Vector2Int LocalToPixel(Vector2 localPos)
    // Convert a given Vector2 into a Vector2Int denoting a set of pixel coordinates
    {
        Vector2Int pixelPosition = new Vector2Int();
        pixelPosition.x = Mathf.RoundToInt(0.5f * spriteWidthPixels + localPos.x * (spriteWidthPixels/spriteWidthUnits));
        pixelPosition.y = Mathf.RoundToInt(0.5f * spriteHeightPixels + localPos.y * (spriteHeightPixels/spriteHeightUnits));
        return pixelPosition;
    }

    Vector2 PixelToLocal(Vector2Int pixPos)
    // Convert a given Vector2Int pixel coordinate into a Vector2 denoting local coordinates
    {
        Vector2 localPos = new Vector2(pixPos.x - 0.5f * spriteWidthPixels, pixPos.y - 0.5f * spriteHeightPixels);
        return localPos * new Vector2(spriteWidthUnits/spriteWidthPixels, spriteHeightUnits/spriteHeightPixels);
    }


    // UPDATE

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Return the location of the mouse click in pixel coordinates
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(LocalToPixel(mousePosition));
        }

        if (Input.GetMouseButton(1))
        {
            // Dig a hole centered around the position where the mouse is clicked
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition = transform.InverseTransformPoint(mousePosition);
            Mine(mousePosition, 10);
        }  
    }

}
