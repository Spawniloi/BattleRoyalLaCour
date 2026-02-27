using UnityEngine;
using System.Collections;

public class ItemDash : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;

    [Header("Visuel")]
    public SpriteRenderer sr;

    private bool estActif = true;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // Génère un sprite étoile jaune par défaut
        if (sr != null)
            sr.sprite = SpriteFactory.Creer("etoile",
                        new Color(1f, 0.85f, 0.1f));
    }

    void Update()
    {
        if (!estActif) return;

        // Cherche un requin proche
        RacailleController[] racailles =
            FindObjectsByType<RacailleController>(FindObjectsSortMode.None);

        foreach (var r in racailles)
        {
            if (!r.isMayor) continue; // seul le requin peut ramasser

            float dist = Vector2.Distance(
                transform.position, r.transform.position);

            if (dist <= config.itemRayonCollecte)
            {
                Collecter(r);
                return;
            }
        }
    }

    void Collecter(RacailleController requin)
    {
        estActif = false;
        requin.AjouterMunitionDash();

        // Cache l'item
        if (sr != null) sr.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // Respawn après délai
        StartCoroutine(Respawn());

        Debug.Log($"[ItemDash] Ramassé par J{requin.playerID} !");
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(config.itemRespawnDelai);

        // Nouvelle position aléatoire
        ItemDashManager manager =
            FindFirstObjectByType<ItemDashManager>();
        if (manager != null)
            transform.position = manager.GetPositionAleatoire();

        // Réactive
        if (sr != null) sr.enabled = true;
        GetComponent<Collider2D>().enabled = true;
        estActif = true;

        Debug.Log("[ItemDash] Respawn !");
    }
}