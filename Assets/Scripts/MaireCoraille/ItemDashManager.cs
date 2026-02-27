using UnityEngine;

public class ItemDashManager : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;
    public GameObject itemPrefab;

    private Vector2 tailleTerrain;

    public void Init(Vector2 taille)
    {
        tailleTerrain = taille;

        for (int i = 0; i < config.itemNombreSimult; i++)
        {
            Vector3 pos = new Vector3(
                GetPositionAleatoire().x,
                GetPositionAleatoire().y,
                0f
            );
            GameObject go = Instantiate(itemPrefab, pos, Quaternion.identity);
            ItemDash item = go.GetComponent<ItemDash>();
            if (item != null) item.config = config;
        }

        Debug.Log($"[ItemDashManager] {config.itemNombreSimult} items spawnés");
    }

    public Vector2 GetPositionAleatoire()
    {
        return new Vector2(
            Random.Range(-tailleTerrain.x / 2f + 1f,
                          tailleTerrain.x / 2f - 1f),
            Random.Range(-tailleTerrain.y / 2f + 1f,
                          tailleTerrain.y / 2f - 1f)
        );
    }
}