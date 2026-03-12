using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnakeHeadController : MonoBehaviour
{
    public float _moveSpeed = 5f;
    public string _horizontalInput = "Horizontal";
    public string _verticalInput = "Vertical";

    private Rigidbody2D rb;
    private Vector2 _movement;

    //[Header("Dash")]
    //public float dashMultiplier = 3f;
    //public float dashDuration = 0.25f;
    //public float dashCooldown = 1.5f;

    //private bool canDash = true;
    //private float baseSpeed;

    //private Vector2 lastDirection = Vector2.right;

    void Awake()
    {
        rb=GetComponent<Rigidbody2D>();
        //baseSpeed = _moveSpeed;
    }

    void Update()
    {
        
        _movement.x = Input.GetAxis(_horizontalInput);
        _movement.y = Input.GetAxis(_verticalInput);


        //if(Input.GetKeyDown(KeyCode.Escape) && canDash)
        //{
        //    Debug.Log("Space pressed!");
        //}
        //    StartCoroutine(Dash());
    }

    private void FixedUpdate()
    {

        //if(_movement.sqrMagnitude > 0.01f)
        //{
        //    rb.MovePosition(rb.position + _movement.normalized * _moveSpeed * Time.fixedDeltaTime);
        //    lastDirection = _movement.normalized;
        //}
        //Vector2 moveDir = _movement.sqrMagnitude > 0.01f
        //    ? _movement.normalized

        //    : lastDirection;

        //rb.MovePosition(
        //    rb.position +
        //    moveDir * _moveSpeed * Time.fixedDeltaTime
        //);

        rb.MovePosition(rb.position + _movement * _moveSpeed * Time.fixedDeltaTime);
        //Debug.Log(_movement);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        SnakeSegment segment = other.GetComponent<SnakeSegment>();
        if (segment == null)
            return;

        if (segment.segmentType == SnakeSegment.SegmentType.Head)
            return;
        SnakePlayer otherPlayer = segment._owner;
        SnakePlayer player = GetComponentInParent<SnakePlayer>();

        if (otherPlayer == player) 
            return;
        otherPlayer.RemoveBodySegment(segment, player);

        //SnakePlayer myPlayer = GetComponentInParent<SnakePlayer>();

        //if (segment._owner == myPlayer)
        //    return;

        //if (segment.segmentType != SnakeSegment.SegmentType.Body)
        //    return;

        //SnakePlayer otherPlayer = segment._owner;
        //otherPlayer.RemoveBodySegment(segment, snakePlayer);
   
    }

    
    //IEnumerator Dash()
    //{
    //    //Debug.Log("Coroutine running");
    //    //yield return null;

    //    canDash = false;

    //    Vector2 dashDir = lastDirection;
    //    float originalSpeed = _moveSpeed;

    //    _moveSpeed = originalSpeed * dashMultiplier;

    //    //rb.MovePosition(rb.position + dashDir * _moveSpeed * dashDuration);
    //    //_moveSpeed = baseSpeed * dashMultiplier; 
    //    yield return new WaitForSeconds(dashDuration);
    //    _moveSpeed = originalSpeed;
    //    yield return new WaitForSeconds(dashCooldown);
    //    canDash = true;
    //}


}
