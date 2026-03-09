using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InputTestManager : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;

    [Header("Prefab")]
    public GameObject racaillePrefab;

    [Header("Positions spawn")]
    public Transform[] spawnPoints;

    [Header("Bouton retour")]
    public UnityEngine.UI.Button btnRetour;

    private List<RacailleController> joueurs
        = new List<RacailleController>();

    void Start()
    {
        btnRetour?.onClick.AddListener(RetourMenu);
        SpawnerJoueurs();
    }

    void SpawnerJoueurs()
    {
        int nb = GameData.nombreJoueurs;

        for (int i = 0; i < nb; i++)
        {
            Vector3 pos = spawnPoints[i].position;

            GameObject go = Instantiate(
                racaillePrefab, pos, Quaternion.identity);
            go.SetActive(false);

            RacailleController rc = go.GetComponent<RacailleController>();
            rc.playerID = i + 1;
            rc.config = config;
            rc.gameManager = null; // pas de gameManager

            InputHandler ih = go.GetComponent<InputHandler>();
            if (ih != null)
            {
                ih.playerID = i + 1;
                ih.config = config;
            }

            // Applique visuels
            RacailleVisuel rv = go.GetComponent<RacailleVisuel>();
            PlayerData data = GameData.GetJoueur(i + 1);
            rv?.AppliquerData(data);
            rv?.SetRoleVisuel(false);

            go.name = $"Racaille_J{i + 1}";
            go.SetActive(true);

            joueurs.Add(rc);
        }
    }

    void RetourMenu()
        => SceneManager.LoadScene("Scene_Menu");
}
