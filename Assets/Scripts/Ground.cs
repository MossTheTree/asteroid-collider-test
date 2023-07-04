using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{

    [SerializeField] Texture2D srcTexture;
    Texture2D asteroidTexture;
    SpriteRenderer asteroidSprite;
    PolygonCollider2D asteroidCollider;

    float worldWidth, worldHeight;
    int pixelWidth, pixelHeight;

    // What are these?
    bool testCheck = true;
    int boxNumber;
    
    List<Vector2Int> edgePixels;
    List<List<Vector2>> colliderPointsList;

    void Start()
    {
        
        // Initalize our sprite and texture
        asteroidSprite = GetComponent<SpriteRenderer>();
        asteroidTexture = Instantiate(srcTexture);
        asteroidTexture.Apply();
        MakeSprite(asteroidSprite, asteroidTexture);

        // Set the variables to manage pixel to world
        // coordinate transforms
        worldWidth = asteroidSprite.bounds.size.x;
        worldHeight = asteroidSprite.bounds.size.y;
        pixelWidth = asteroidSprite.sprite.texture.width;
        pixelHeight = asteroidSprite.sprite.texture.height;

        // Build the collider
        asteroidCollider = gameObject.AddComponent<PolygonCollider2D>();
        BuildCollider();
        // BuildBoxCollider();

    }

    void MakeSprite(SpriteRenderer spriteRenderer, Texture2D texture)
    {
        
        /*
        Create a new sprite based on a texture passed to this method
        and set its pivot point to the center
        */

        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0,0,texture.width, texture.height), Vector2.one * 0.5f);
    }

    void DigHole(Vector3 mousePos, int radius)
    {        

        /*
        This method sets to transparent a set of pixels in a circle with
        a given radius and position, on the texture attached to the
        sprite on this gameobject.
        */

        // Convert the center of the hole to be dug into pixel coordinates
        Vector2Int digCenter = WorldToPixel(mousePos);

        // Initialize our pixels for the loop
        int px, nx, py, ny, distance;

        // Set up a bool to check if anything changed
        bool pixHasChanged = false;

        // Loop through the pixels to be changed, beginning with the center
        // and working our way outward in a spiral
        for (int i = 0; i < radius; i++)
        {
            distance = Mathf.RoundToInt(Mathf.Sqrt(radius * radius - i * i));
            for (int j = 0; j < distance; j++)
            {   
                // Set our pixel to be changed based on distance from
                // the center of the hole, based on where we are in the loop
                px = digCenter.x + i;
                nx = digCenter.x - i;
                py = digCenter.y + j;
                ny = digCenter.y - j;

                // Set the transparency of each of those pixels to 100%, if it's not transparent already
                // Track if any changes were made, so we don't have to rebuild the collider unnecessarily
                if (asteroidTexture.GetPixel(px,py).a > 0) { asteroidTexture.SetPixel(px,py, Color.clear); pixHasChanged = true; }
                if (asteroidTexture.GetPixel(px,ny).a > 0) { asteroidTexture.SetPixel(px,ny, Color.clear); pixHasChanged = true; }
                if (asteroidTexture.GetPixel(nx,py).a > 0) { asteroidTexture.SetPixel(nx,py, Color.clear); pixHasChanged = true; }
                if (asteroidTexture.GetPixel(nx,ny).a > 0) { asteroidTexture.SetPixel(nx,ny, Color.clear); pixHasChanged = true; }
            }
        }

        // Apply the changes, rebuild the sprite, and then rebuild the collider
        if (pixHasChanged) {
            asteroidTexture.Apply();
            MakeSprite(asteroidSprite, asteroidTexture);
            BuildCollider();
        }
    }

    bool IsPixelAtEdge(Vector2Int thisPix, Texture2D tex)
    {

        /*
        This method checks first to see if a given pixel is transparent,
        and then if so checks the pixels above, below, and to either
        side of a given pixel and  returns true if any one of
        those pixels is non-transparent.
        
        This can be used to infer that this pixel is beside the sprite.

        It takes the x, y pixel coordinates of the pixel to be checked
        and the texture to be checked, and returns true or false.
        */

        int x = thisPix.x;
        int y = thisPix.y;

        if (tex.GetPixel(x,y).a > 0) {
            return false;
        } 
        else if ((tex.GetPixel(x+1,y).a > 0)  ||
                 (tex.GetPixel(x,y+1).a > 0)  ||
                 (tex.GetPixel(x-1,y).a > 0)  ||
                 (tex.GetPixel(x,y-1).a > 0))
        {
            return true;
        }
        else return false;
    }
    

    bool IsPixelBesideSameShape(Vector2Int testPix, Vector2Int currentPix, Texture2D tex)
    {

        /*
        This method is identical to IsPixelAtEdge except it adds
        an additional check, to see if the edge is shared with some
        previous pixel that was also at the edge
        */

        Vector2Int[] adjacentPix2Check = new Vector2Int[] {
            new Vector2Int(0,1),
            new Vector2Int(1,0),
            new Vector2Int(0,-1),
            new Vector2Int(-1,0),
            new Vector2Int(-1,1),
            new Vector2Int(1,1),
            new Vector2Int(1,-1),
            new Vector2Int(-1,-1)
        };
        
        List<Vector2Int> aroundCurrent = new List<Vector2Int>();
        List<Vector2Int> aroundTest = new List<Vector2Int>();

        for (int i = 0; i < adjacentPix2Check.Length; i++) {

            if (tex.GetPixel(currentPix.x + adjacentPix2Check[i].x, currentPix.y + adjacentPix2Check[i].y).a > 0)
            {
                aroundCurrent.Add(new Vector2Int(currentPix.x + adjacentPix2Check[i].x, currentPix.y + adjacentPix2Check[i].y));
            }
            if (tex.GetPixel(testPix.x + adjacentPix2Check[i].x, testPix.y + adjacentPix2Check[i].y).a > 0)
            {
                aroundTest.Add(new Vector2Int(testPix.x + adjacentPix2Check[i].x, testPix.y + adjacentPix2Check[i].y));
            }
        }

        foreach(Vector2Int test in aroundCurrent) {
            if (aroundTest.Contains(test)) {
                return true;
            }
        }

        return false;

    } 

    void BuildBoxCollider()
    {
        edgePixels = new List<Vector2Int>();

        for (int i = 0; i < asteroidTexture.width; i++)
        {
            for (int j = 0; j < asteroidTexture.height; j++)
            {
                if (IsPixelAtEdge(new Vector2Int(i,j),asteroidTexture))
                {
                    BoxCollider2D pixelCollider = gameObject.AddComponent<BoxCollider2D>();
                    pixelCollider.size = new Vector2(0.01f,0.01f);
                    pixelCollider.offset = new Vector2(PixelToWorld(new Vector2Int(i,j)).x, PixelToWorld(new Vector2Int(i,j)).y);
                }
            }
        }
    }

    void BuildCollider()
    {

        /*
        This method scans the texture associated with this game object,
        finds the outline based on which pixels are transparent and beside
        the sprite, then builds a polygon collider based on that shape. It
        will build multiple paths if the sprite has been broken into multiple
        pieces
        */

        // Dump any old data, and turn off the collider
        colliderPointsList = new List<List<Vector2>>();
        edgePixels = new List<Vector2Int>();
        asteroidCollider.enabled = false;     

        // Initalize our variables for the main loop
        List<Vector2> colliderPoints = new List<Vector2>();
        Vector2Int firstPixel = new Vector2Int();
        Vector2Int previousPixel = new Vector2Int();
        Vector2Int previousPreviousPixel = new Vector2Int();
        Vector2Int currentPixel = new Vector2Int();
        Vector2Int nextPixel;

        bool needFirstPixel = true;
        int colliderIndex = 0;

        // Find the total number of edgePixels, which is
        // defined by any pixel that is transparent and above,
        // beside, or below a non-transparent pixel
        for (int i = 0; i < asteroidTexture.width; i++)
        {
            for (int j = 0; j < asteroidTexture.height; j++)
            {
                if (IsPixelAtEdge(new Vector2Int(i,j),asteroidTexture))
                {
                    edgePixels.Add(new Vector2Int(i,j));
                }
            }
        }

        // Define a new variable totalEdgePixels as our
        // loop limit, because edgePixels is going to shrink
        // as we remove items from it each loop
        int totalEdgePixels = edgePixels.Count;
        // Debug.Log(totalEdgePixels);

        // Start looping through totalEdgePixels
        for (int k = 0; k < totalEdgePixels; k++)
        {

            // If we're starting a new path, then initialize
            // Ignore if we're already working on a path
            if (edgePixels.Count > 0 && needFirstPixel) {
                needFirstPixel = false;

                colliderPoints = new List<Vector2>();
                firstPixel = edgePixels[0];
                currentPixel = firstPixel;
                previousPixel = firstPixel;
                previousPreviousPixel = firstPixel;
            }

            // Find the next pixel and check if it makes an angle
            // with the previous and curent, and if it does then
            // add the current pixel to the collider
            nextPixel = FindAdjacentPixel(previousPixel, previousPreviousPixel, currentPixel, asteroidTexture);
            if (Vector2.Angle((nextPixel - currentPixel), (previousPixel - currentPixel)) != 180f)
            {
                colliderPoints.Add(PixelToWorld(currentPixel));
            }

            // Get set up for next loop by removing the currentPixel from the
            // edgePixels, unless it's the firstPixel, which we'll need to
            // complete the path
            if (edgePixels.Contains(currentPixel) && currentPixel != firstPixel)
            {
                edgePixels.Remove(currentPixel);
            }
            previousPreviousPixel = previousPixel;
            previousPixel = currentPixel;
            currentPixel = nextPixel;

            // If we've reached the end of the path
            // then add the path to the colliderPointsList
            // and re-initialize for the next path
            if (currentPixel == firstPixel)
            {
                colliderPoints.Add(PixelToWorld(firstPixel));
                edgePixels.Remove(currentPixel);
                edgePixels.Remove(firstPixel);
                colliderPointsList.Add(colliderPoints);
                if (edgePixels.Count > 0) {
                    colliderIndex++;
                    needFirstPixel = true;
                }
            }
        }

        // The loop is finished, so we should have a collection of paths
        // Start by setting the pathCount to the number of collider lists that we have
        asteroidCollider.pathCount = colliderPointsList.Count;

        // Then add those paths tothe collider one at a time
        for (int l = 0; l < colliderPointsList.Count; l++)
        {
            asteroidCollider.SetPath(l,colliderPointsList[l]);
        }

        // Now we can re-enable the collider and we're good to go
        asteroidCollider.enabled = true;

    }

    Vector2Int FindAdjacentPixel(Vector2Int lastPix, Vector2Int lastLastPix, Vector2Int currentPix, Texture2D tex) {

        /*
        This method looks all around a given pixel within a given texture 
        and returns a pixel that is:
        a) in one of the 8 adjacent spaces
        b) not one of the previous 2 pixels that was added
        c) not the current pixel
        c) transparent and at the edge of the sprite
        */

        Vector2Int adjacentPix = new Vector2Int();
        List<Vector2Int> possibleAdjacentPix = new List<Vector2Int>();

        // Use an array to efficiently check all 8 possible locations
        // Check orthoganal directions first because that seems to be more efficient... ?
        Vector2Int[] adjacentPix2Check = new Vector2Int[] {
            new Vector2Int(0,1),
            new Vector2Int(1,0),
            new Vector2Int(0,-1),
            new Vector2Int(-1,0),
            new Vector2Int(-1,1),
            new Vector2Int(1,1),
            new Vector2Int(1,-1),
            new Vector2Int(-1,-1)
        };

        // Loop through the array and check each pixel for
        // the conditions listed above
        for (int j = 0; j < adjacentPix2Check.Length; j++) {
            Vector2Int testPix = new Vector2Int(currentPix.x + adjacentPix2Check[j].x, currentPix.y + adjacentPix2Check[j].y);
            if (
                IsPixelAtEdge(testPix, tex) &&
                // IsPixelBesideSameShape(testPix, currentPix, tex) &&
                // Does this pixel share an edge with a non-transparent pixel that is next to (8) the previous?
                (testPix != lastPix) &&
                (testPix != lastLastPix) &&
                (testPix != currentPix)
            )


            {
                // If we've found the pixel we want, then exit the loop
                adjacentPix = new Vector2Int(testPix.x, testPix.y);
                break;
            }
        }   

        // Return the pixel we want
        return adjacentPix;
    }

    Vector2Int WorldToPixel(Vector2 pos)
    {

        /*
        Method to convert a given Vector2 into a Vector2Int denoting
        a set of pixel coordinates
        */

        Vector2Int pixelPosition = Vector2Int.zero;
        var dx = pos.x - transform.position.x;
        var dy = pos.y - transform.position.y;
        pixelPosition.x = Mathf.RoundToInt(0.5f * pixelWidth + dx * (pixelWidth/worldWidth));
        pixelPosition.y = Mathf.RoundToInt(0.5f * pixelHeight + dy * (pixelHeight/worldHeight));
        return pixelPosition;
    }

    Vector2 PixelToWorld(Vector2Int pixPos)
    {

        /*
        Method to convert a given Vector2Int into a Vector2 denoting
        a position in world space
        */

        Vector2 worldPosition;
        worldPosition.x = transform.position.x - (worldWidth * 0.5f) + (pixPos.x * (worldWidth/pixelWidth));
        worldPosition.y = transform.position.y - (worldHeight * 0.5f) + (pixPos.y * (worldHeight/pixelHeight));
        return worldPosition;
    }

    void Update()
    {
 
        if (Input.GetMouseButtonDown(0))
        {
            /*
            Return the location of the mouse click in world coordinates
            */

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(WorldToPixel(mousePosition));
        }

        if (Input.GetMouseButton(1))
        {
            /*
            Dig a hole centered around the position where the mouse is clicked
            */

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            DigHole(mousePosition, 10);
        }  
    }
}
