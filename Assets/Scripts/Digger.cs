using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Digger : MonoBehaviour
{

    void Update()
    {
        // Set the location of the digger to the mouse position
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GetComponent<Rigidbody2D>().position = mousePos;
    }
}
