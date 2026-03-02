using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 3f;
    public bool isHeld = false;
    public bool isThrown = false;

    Vector2 direction;

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
}