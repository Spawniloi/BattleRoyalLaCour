using UnityEngine;

public class TeamZones : MonoBehaviour
{
    public int TeamID;

    public BoxCollider2D area;

    public Vector2 GetRandomPoint()
    {
        Bounds bounds = area.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector2(x, y);
    }

    public Vector2 GetRandomSpawnPosition()
    {
        Vector2 min = area.bounds.min;
        Vector2 max = area.bounds.max;

        return new Vector2(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y)
        );
    }

    public Vector2 ClampPosition(Vector2 position)
    {
        Bounds bounds = area.bounds;

        float clampedX = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        float clampedY = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);

        return new Vector2(clampedX, clampedY);
    }
}
