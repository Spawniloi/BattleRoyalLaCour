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
}
