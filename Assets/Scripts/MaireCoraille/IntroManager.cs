using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject panneauIntro;     // panel qui couvre tout
    public TextMeshProUGUI labelAnnonce;    // "J2 est le Requin !"
    public TextMeshProUGUI labelCompte;     // 3... 2... 1... JOUEZ !
    public Image fondPanel;       // fond semi-transparent

    [Header("Config")]
    public float dureeAnnonce = 2.0f;  // temps d'affichage du maire
    public float dureeCompte = 1.0f;  // durée de chaque chiffre

    // Appelé par MaireGameManager après avoir choisi le maire
    public IEnumerator LancerIntro(RacailleController maire,
                                   System.Action onFin)
    {
        // Bloque tous les inputs
        panneauIntro?.SetActive(true);

        // ── Annonce le maire ──────────────────────────────────────────────────
        if (labelAnnonce != null)
        {
            // Couleur du joueur
            PlayerData data = GameData.GetJoueur(maire.playerID);
            Color coul = data.GetCouleur();

            labelAnnonce.text = $"<color=#{ColorUtility.ToHtmlStringRGB(coul)}>" +
                                 $"J{maire.playerID}</color>" +
                                 $"\nest le 🦈 Requin !";
            labelAnnonce.gameObject.SetActive(true);
        }

        if (labelCompte != null)
            labelCompte.gameObject.SetActive(false);

        yield return new WaitForSeconds(dureeAnnonce);

        // ── Compte à rebours ──────────────────────────────────────────────────
        if (labelAnnonce != null)
            labelAnnonce.gameObject.SetActive(false);

        if (labelCompte != null)
            labelCompte.gameObject.SetActive(true);

        string[] comptes = { "3", "2", "1", "JOUEZ !" };

        foreach (string texte in comptes)
        {
            if (labelCompte != null)
            {
                labelCompte.text = texte;
                labelCompte.transform.localScale = Vector3.one * 1.5f;
            }

            // Animation scale
            float t = 0f;
            while (t < dureeCompte)
            {
                float scale = Mathf.Lerp(1.5f, 1.0f, t / dureeCompte);
                if (labelCompte != null)
                    labelCompte.transform.localScale = Vector3.one * scale;
                t += Time.deltaTime;
                yield return null;
            }
        }

        // ── Cache le panneau ──────────────────────────────────────────────────
        panneauIntro?.SetActive(false);

        // Lance la partie
        onFin?.Invoke();
    }
}