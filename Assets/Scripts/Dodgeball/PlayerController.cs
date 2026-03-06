using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerControls controls;

    Vector2 moveInput;
    public Vector2 lastDirection = Vector2.right;

    public float speed = 5f;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        Move();
        
        // FORWARD BY DIRECTION OR MOUSE
        RotateToMovement(moveInput);
        //LookAtMouse();
        
        // LOOK DIRECTION
        if(moveInput != Vector2.zero)
        {
            lastDirection = moveInput.normalized;
        }
    }

    void Move()
    {
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0);
        transform.position += movement * speed * Time.deltaTime;
    }
    
    void RotateToMovement(Vector2 moveInput)
    {
        if(moveInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0,0,angle);
        }
    }
    
    void LookAtMouse()
    {
    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

    Vector2 dir = mousePos - transform.position;

    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

    transform.rotation = Quaternion.Euler(0,0,angle);
    }
     
    // LOOK DIRECTION GIZMOS
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)lastDirection * 3f);
    }

}