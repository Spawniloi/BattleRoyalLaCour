using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    // Valeurs pour Maire Coraille (OrthoSize 5.2)
    public float safeLeft = -8.5f;
    public float safeRight = 8.5f;
    public float safeTop = 4.6f;
    public float safeBottom = -4.6f;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        // Zone visible totale
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(18.6f, 10.4f, 0));
        // Safe area (spawns)
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(15.6f, 7.8f, 0));
    }
}