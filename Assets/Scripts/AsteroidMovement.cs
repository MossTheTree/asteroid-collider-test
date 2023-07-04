using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidMovement : MonoBehaviour
{
    [SerializeField] float asteroidRotationSpeed = 1f;

    void Start() {    
    }


    void FixedUpdate()
    {
         transform.Rotate(Vector3.forward * Time.deltaTime * asteroidRotationSpeed);
    }
}
