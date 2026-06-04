using System;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

// Time.deltaTime - The seconds passed since the last frame as a float. Without it,
// the fall speed would be tied directly to the computer's frame rate (FPS).
// A faster computer would call the movement code more often.

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    
    [SerializeField] private CharacterController characterController; // Reference to characterController to be seen in Unity
    [SerializeField] private PlayerInputHandler playerInputHandler;  // Reference to playerInputHandler to be exposed in Unity
    [SerializeField] private Camera playerCamera;


    [Header("Jump Config")]
    [SerializeField] private float jumpForce = 5.0f;  // How high player jumps
    [SerializeField] private float gravityMultiplier = 1.0f;
    [SerializeField] private int jumpMax = 2;
    private int jumpCount;

    [Header("Movement Config")]
    [SerializeField] private float walkSpeed = 3.0f; // How fast player moves
    [SerializeField] private float sprintMultiplier = 2.0f;

    [Header("Rotation Config")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float verticalViewRange = 80f;

    [Header("Inventory Config")]
    [SerializeField] private string selected;

    private Vector3 currentMovement;
    private float verticalRotation;

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleJumping();
        ApplyMovement();
    }

    private void HandleRotation()
    {
        float mouseXRotation = playerInputHandler.RotateVector.x * mouseSensitivity;
        float mouseYRotation = playerInputHandler.RotateVector.y * mouseSensitivity;

        ApplyHorizontalRotation(mouseXRotation);
        ApplyVerticalRotation(mouseYRotation);
    }

    private void ApplyVerticalRotation(float mouseYRotation)
    {
        verticalRotation = Mathf.Clamp(verticalRotation - mouseYRotation, -verticalViewRange, verticalViewRange);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void ApplyHorizontalRotation(float mouseXRotation)
    {
        transform.Rotate(0, mouseXRotation, 0);
    }

    private void HandleMovement()
    {
        Vector3 worldDirection = CalculateWorldDirection();
        float currentSpeed = walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultiplier : 1.0f); // If sprint is triggered, multiply walk speed by sprint multiplier
        currentMovement.x = worldDirection.x * currentSpeed;
        currentMovement.z = worldDirection.z * currentSpeed;
    }

    private Vector3 CalculateWorldDirection()
    {
        Vector3 inputDirection = new Vector3(playerInputHandler.MovementVector.x, 0, playerInputHandler.MovementVector.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);

        return worldDirection.normalized;
    }

    private void HandleJumping()
    {
        if (characterController.isGrounded)
        {
            currentMovement.y = -0.5f;
            jumpCount = 0;

            if (playerInputHandler.JumpTriggered && jumpMax > jumpCount)
            {
                currentMovement.y = jumpForce;
                jumpCount++;
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
      

    void playerPickUp ()
    {

    }
}
