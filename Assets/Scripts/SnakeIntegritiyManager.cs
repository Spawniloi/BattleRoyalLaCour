using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnakeIntegritiyManager : MonoBehaviour
{
    public float invincibilityDuration = 100f;
    private bool isInvincible = false;

    private SnakeBodyFollow bodyFollow;
    private SnakePlayer snakePlayer;

    void Awake()
    {
        bodyFollow = GetComponent<SnakeBodyFollow>();
        snakePlayer = GetComponent<SnakePlayer>();  
    }

    void Update()
    {
        RebuildSegmentChain();
    }

    void RebuildSegmentChain()
    {
        bodyFollow._segments.RemoveAll(segment =>  segment == null);
    }

    public void OnSegmentLost()
    {
        if (!isInvincible)
            StartCoroutine(InvincibilityCoroutine());
    }
     
    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        Collider2D headCollider = bodyFollow._head.GetComponent<Collider2D>();
        headCollider.enabled = false;

        yield return new WaitForSecondsRealtime(invincibilityDuration);

        headCollider.enabled = true;
        isInvincible = false;
    }



}
