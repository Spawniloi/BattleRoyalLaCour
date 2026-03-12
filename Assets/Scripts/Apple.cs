using UnityEngine;

public class Apple : MonoBehaviour
{
    public float _respawnRadius = 8f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        SnakeSegment segment = other.GetComponent<SnakeSegment>();

        //if (segment != null)
        //{
        //    segment.TryAddBodySegments();
        //    segment.AddScore(1);
        //    Destroy(gameObject);
        //}
        if (segment == null)
            return;

        if (segment.segmentType != SnakeSegment.SegmentType.Head)
            return;

        SnakePlayer player = segment._owner;
        bool gainedSegment = player.TryAddBodySegments();

        if (!gainedSegment)
        {
            player.AddScore(1);
        }

        Respawn();

    }

    void Respawn()
    {
        Vector2 randomPos = Random.insideUnitCircle * _respawnRadius;
        transform.position = randomPos;
    }

}
