using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab;

    Camera cam;
    float minX;
    float maxX;
    float minY;
    float maxY;

    private float spawnDelay = 2.5f;
    public float mapSize = 20f;

    private float endGameSpawnDelay = 1.25f;
    bool frenzyStarted = false;

    void Start()
    {
        cam = Camera.main;

        float height = cam.orthographicSize;
        float width = height * cam.aspect;

        Vector3 camPos = cam.transform.position;

        minX = camPos.x - width;
        maxX = camPos.x + width;
        minY = camPos.y - height;
        maxY = camPos.y + height;

        InvokeRepeating(nameof(SpawnBall), 2f, spawnDelay);
    }
    private void Update()
    {       
        if (!frenzyStarted && GameManager.Instance.IsEndGamePhase())
        {
            StartFinalMinute();
        }
    }
    private void OnDisable()
    {
        CancelInvoke(nameof(SpawnBall));
    }

    void SpawnBall()
    {
        Vector2 spawnPos = RandomEdgePosition();
        GameObject ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        Vector2 center = Vector2.zero;
        Vector2 dir = (center - spawnPos).normalized;

        ball.GetComponent<Ball>().Initialize(dir);
    }

    void StartFinalMinute()
    {
        frenzyStarted = true;

        CancelInvoke(nameof(SpawnBall));
        InvokeRepeating(nameof(SpawnBall), 0f, endGameSpawnDelay);

        Debug.Log("FINAL MINUTE - BALL FRENZY");
    }
    Vector2 RandomEdgePosition()
    {
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // gauche
                return new Vector2(minX, Random.Range(minY, maxY));

            case 1: // droite
                return new Vector2(maxX, Random.Range(minY, maxY));

            case 2: // haut
                return new Vector2(Random.Range(minX, maxX), maxY);

            default: // bas
                return new Vector2(Random.Range(minX, maxX), minY);
        }
    }
}