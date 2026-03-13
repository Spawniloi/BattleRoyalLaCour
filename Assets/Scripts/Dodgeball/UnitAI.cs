using UnityEngine;
using static GameManager;

public class UnitAI : MonoBehaviour
{
    Unit _unit;

    public float patrolSpeed = 1.5f;
    Vector2 targetPosition;

    //Ball Seeking
    private float ballDetectionRadius = 3f;
    Ball targetBall;
    void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    private void Start()
    {
        ChooseNewTarget();
    }

    void Update()
    {
        if (GameManager.Instance.currentState != GameState.Playing) return;

        ClampInsideZone();

        if (_unit.isControlled || !_unit.isAlive) return;

        

        // Seek Ball
        if (!_unit.HasBall)
        {
            targetBall = FindNearbyBall();

            if (targetBall != null)
            {
                GoToBall();
                return;
            }
        }

        if (_unit.HasBall) return;
        Patrol();
    }

    private void Patrol()
    {
        if (Vector2.Distance(transform.position, targetPosition) < 0.3f)
        {
            ChooseNewTarget();
        }

        Vector2 newPos = Vector2.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);

        // Clamp
        if (_unit.zone != null)
        {
            newPos = _unit.zone.ClampPosition(newPos);
        }

        transform.position = newPos;

        // Rotation vers la cible
        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void ChooseNewTarget()
    {
        if (_unit.zone != null)
        {
            targetPosition = _unit.zone.GetRandomPoint();
        }
        else
        {
            // fallback si pas de zone
            targetPosition = (Vector2)transform.position;
        }
    }

    Ball FindNearbyBall()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ballDetectionRadius);

        Ball closest = null;
        float dist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Ball ball = hit.GetComponent<Ball>();

            if (ball == null || ball.isHeld) continue;

            float d = Vector2.Distance(transform.position, ball.transform.position);

            if (d < dist)
            {
                dist = d;
                closest = ball;
            }
        }

        return closest;
    }

    void GoToBall()
    {
        if (targetBall == null) return;

        Vector2 newPos = Vector2.MoveTowards(
            transform.position,
            targetBall.transform.position,
            patrolSpeed * Time.deltaTime
        );

        if (_unit.zone != null)
            newPos = _unit.zone.ClampPosition(newPos);

        transform.position = newPos;

        Vector2 dir = (targetBall.transform.position - transform.position).normalized;

        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void ClampInsideZone()
    {
        if (_unit.zone == null) return;

        Vector2 clamped = _unit.zone.ClampPosition(transform.position);
        transform.position = clamped;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPosition, 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, ballDetectionRadius);
    }
}