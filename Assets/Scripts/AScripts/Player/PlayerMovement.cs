using System;
using UnityEngine;

// Time.deltaTime - The seconds passed since the last frame as a float. Without it,
// the fall speed would be tied directly to the computer's frame rate (FPS).
// A faster computer would call the movement code more often.

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    
    [SerializeField] private CharacterController characterController; // Reference to characterController to be seen in Unity
    [SerializeField] private PlayerInputHandler playerInputHandler;  // Reference to playerInputHandler to be exposed in Unity

    [Header("Jump Config")]
    [SerializeField] private float jumpForce = 5.0f;  // How high player jumps
    [SerializeField] private float gravityMultiplier = 1.0f;

    [Header("Sprint Config")]
    [SerializeField] private float sprintMultiplier = 3.0f;

    private Vector3 currentMovement;

    void Update()
    {
        HandleJumping();
        ApplyMovement();
    }

    private void HandleJumping()
    {
        if (characterController.isGrounded)
        {
            currentMovement.y = -0.5f;

            if (playerInputHandler.JumpTriggered)
            {
                currentMovement.y = jumpForce;
            }
        }
        else
        {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime; 
        }
    }

    private void ApplyMovement()
    {
        characterController.Move(currentMovement * Time.deltaTime);
    }
}
