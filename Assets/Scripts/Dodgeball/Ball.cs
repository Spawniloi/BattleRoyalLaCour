using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 3f;
    public float throwSpeed = 12f;
    public bool isHeld = false;
    public bool isThrown = false;

    Vector2 direction;

    public int OwnerTeam = -1;

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        if (isHeld)
            return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        CheckBounds();
    }

    void CheckBounds()
    {
        float limit = 20f;

        if (Mathf.Abs(transform.position.x) > limit || Mathf.Abs(transform.position.y) > limit)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Unit unit = collision.GetComponent<Unit>();
        if (unit == null) return;

        if (isThrown && unit.isAlive)
        {
            if (unit.teamID == OwnerTeam) return; 
            HitUnit(unit);
            return;
        }

        if (!isThrown && !isHeld && !unit.HasBall)
        {
            PickUp(unit);
        }
    }

    private void PickUp(Unit unit)
    {
        isHeld = true;
        unit.heldBall = this;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // Deactivate Rigidbody's physics 

        transform.SetParent(unit.transform);
        transform.localPosition = new Vector3(0.5f, 0, 0);

        if (GameManager.Instance.teamScores.ContainsKey(unit.teamID)) //SCORE
        {
            GameManager.Instance.teamScores[unit.teamID].ballsPicked++;
        }
    }

    internal void Throw(Vector2 dir)
    {
        isHeld = false;
        isThrown = true;

        transform.SetParent(null);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.simulated = true;

        rb.linearVelocity = dir.normalized * throwSpeed;

    }

    void HitUnit(Unit unit)
    {
        unit.Die();
        if (GameManager.Instance.teamScores.ContainsKey(OwnerTeam)) //SCORE
        {
            GameManager.Instance.teamScores[OwnerTeam].eliminations++;
        }


        Destroy(gameObject);
    }
}