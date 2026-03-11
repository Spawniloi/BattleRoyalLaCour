using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    TeamZones zone;
    Unit Unit;

    Vector2 moveInput;
    public Vector2 lastDirection = Vector2.right;

    public float speed = 5f;

    void Awake()
    {
        Unit = GetComponent<Unit>();
    }

    private void Start()
    {
        zone = Unit.zone;
    }
    void Update()
    {
        RotateToMovement(moveInput);
        Move();
        
        // LOOK DIRECTION
        if(moveInput != Vector2.zero)
        {
            lastDirection = moveInput.normalized;
        }
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void Move()
    {
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0);
        Vector3 newPos = transform.position + movement * speed * Time.deltaTime;

        // Clamp dans la zone si elle est définie
        if (zone != null)
        {
            newPos = (Vector3)zone.ClampPosition(newPos);
        }

        transform.position = newPos;
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