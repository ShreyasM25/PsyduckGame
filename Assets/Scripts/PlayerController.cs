using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class PlayerController : MonoBehaviour
{
    
    [SerializeField]
    [Tooltip("Insert Character Controller")]
    private CharacterController controller;
    
    [SerializeField]
    [Tooltip("Insert Main Camera")]
    private Camera mainCamera;
    
    private Vector3 velocity;
    private bool grounded;
    private float gravity = -9.8f;
    private float groundCastDist = 0.05f;

    public float speed = 5f;
    public float jumpHeight = 10f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { 
        // Transform acquisition
        Transform playerTransform = transform;
        Transform cameraTransform = mainCamera.transform;
        
        // Grounded
        grounded = Physics.Raycast(playerTransform.position, Vector3.down, groundCastDist);
        
        // DEBUG - Visualize raycast
        /*
        if (grounded)
        {
            Debug.DrawRay(playerTransform.position, Vector3.down, Color.blue);
        }
        else
        {
            Debug.DrawRay(playerTransform.position, Vector3.down, Color.red);
        }
        */
        
        // Ground movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 movement = (playerTransform.right * x) + (playerTransform.forward * z);
        controller.Move(movement * speed * Time.deltaTime);
        
        // Rotate alongside the camera
        playerTransform.rotation = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up);
        
        // Gravity and jumping
        velocity.y += gravity * Time.deltaTime;
        if (Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight);
        }
        controller.Move(velocity * Time.deltaTime);
        
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
