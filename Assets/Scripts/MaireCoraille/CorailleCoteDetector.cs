using UnityEngine;

// Ce script est sur chaque collider de côté (A ou B)
public class CorailleCoteDetector : MonoBehaviour
{
    public Coraille coraille;  // référence au Coraille parent
    public bool estCoteA;      // true = côté A, false = côté B

    void OnTriggerEnter2D(Collider2D other)
    {
        RacailleController poisson = other.GetComponent<RacailleController>();
        if (poisson == null) return;

        coraille.TenterAccrochage(poisson, estCoteA);
    }
}