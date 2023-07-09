using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidMovement : MonoBehaviour
{
    [SerializeField] float asteroidRotationSpeed = 1f;
    private Rigidbody2D m_Rigidbody;
    
    void Start()
    {
        
        // m_Rigidbody = GetComponent<Rigidbody2D>();
    }


    void FixedUpdate()
    {
         transform.Rotate(Vector3.forward * Time.deltaTime * asteroidRotationSpeed);
         // m_Rigidbody.MoveRotation(m_Rigidbody.rotation + Time.deltaTime * asteroidRotationSpeed);
    }
}
