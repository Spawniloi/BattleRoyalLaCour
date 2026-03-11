using UnityEngine;
using System.Collections.Generic;

public class ItemDashManager : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;
    public GameObject itemPrefab;

    [Header("Marges")]
    public float margesBords = 1.5f; // distance min du bord
    public float distanceCoraille = 2.0f; // distance min des corailles

    private Vector2 tailleTerrain;
    private List<ItemDash> items = new List<ItemDash>();

    public void Init(Vector2 taille)
    {
        tailleTerrain = taille;

        for (int i = 0; i < config.itemNombreSimult; i++)
        {
            Vector2 pos = TrouverPositionLibre();
            GameObject go = Instantiate(itemPrefab,
                             new Vector3(pos.x, pos.y, 0f),
                             Quaternion.identity);
            ItemDash item = go.GetComponent<ItemDash>();
            if (item != null)
            {
                item.config = config;
                item.manager = this;
                items.Add(item);
            }
        }
    }

    public Vector2 GetPositionAleatoire() => TrouverPositionLibre();

    Vector2 TrouverPositionLibre()
    {
        float xMin = -tailleTerrain.x / 2f + margesBords;
        float xMax = tailleTerrain.x / 2f - margesBords;
        float yMin = -tailleTerrain.y / 2f + margesBords;
        float yMax = tailleTerrain.y / 2f - margesBords;

        for (int essai = 0; essai < 50; essai++)
        {
            Vector2 pos = new Vector2(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax)
            );

            // Vérifie distance des corailles
            bool tropProche = false;
            Coraille[] corailles =
                FindObjectsByType<Coraille>(FindObjectsSortMode.None);

            foreach (var c in corailles)
            {
                if (Vector2.Distance(pos, c.transform.position)
                    < distanceCoraille)
                {
                    tropProche = true;
                    break;
                }
            }

            if (!tropProche) return pos;
        }

        // Fallback centre de la map
        return Vector2.zero;
    }
}