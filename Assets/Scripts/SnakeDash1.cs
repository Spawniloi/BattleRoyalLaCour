///*
//Unity 2D Dash
//This program provides a function to implement a dash mechanic in a 2D Unity game.
//The dash mechanic allows the player character to quickly move in a specified direction for a short distance.
//*/
//using UnityEngine;
//public class PlayerController : MonoBehaviour
//{
//    public float dashDistance = 5f; // Distance to dash in units
//    public float dashDuration = 0.5f; // Duration of the dash in seconds
//    public float dashCooldown = 1f; // Cooldown period between dashes in seconds
//    private bool canDash = true; // Flag to check if the player can dash
//    private Vector2 dashDirection; // Direction of the dash

//    private Rigidbody2D rb;

//    private void Start()
//    {
//        rb = GetComponent<Rigidbody2D>();
//    }

//    private void Update()
//    {
//        // Check for dash input
//        if (Input.GetKeyDown(KeyCode.Space) && canDash)
//        {
//            // Get the dash direction based on player input
//            dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

//            // Start the dash coroutine
//            StartCoroutine(Dash());
//        }
//    }

//    private IEnumerator Dash()
//    {
//        // Disable the ability to dash during the dash and cooldown period
//        canDash = false;

//        // Calculate the end position of the dash
//        Vector2 endPosition = rb.position + dashDirection * dashDistance;

//        // Calculate the dash speed
//        float dashSpeed = dashDistance / dashDuration;

//        // Perform the dash
//        while (rb.position != endPosition)
//        {
//            // Move the player towards the end position
//            rb.position = Vector2.MoveTowards(rb.position, endPosition, dashSpeed * Time.deltaTime);

//            yield return null;
//        }

//        // Enable the ability to dash after the cooldown period
//        yield return new WaitForSeconds(dashCooldown);
//        canDash = true;
//    }

//}


using UnityEngine;
using System.Collections; // << obligatoire pour IEnumerator

public class PlayerController : MonoBehaviour
{
    public float dashDistance = 5f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 1f;

    private bool canDash = true;
    private Vector2 dashDirection;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            Debug.Log("Space Pressed");
            dashDirection = Vector2.right;

            // Si aucune direction n'est entrée, dash vers la droite par défaut
            if (dashDirection == Vector2.zero)
                dashDirection = Vector2.right;

            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;

        Vector2 startPos = rb.position;
        Vector2 endPos = startPos + dashDirection * dashDistance;

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            rb.MovePosition(Vector2.Lerp(startPos, endPos, t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(endPos);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
