using UnityEngine;

public class CorailleCoteDetector : MonoBehaviour
{
    public Coraille coraille;
    public bool estCoteA;

    void OnTriggerEnter2D(Collider2D other)
    {
        RacailleController poisson =
            other.GetComponent<RacailleController>();
        if (poisson == null) return;

        coraille.TenterAccrochage(poisson, estCoteA);
    }
}