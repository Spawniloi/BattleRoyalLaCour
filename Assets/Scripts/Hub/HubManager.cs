using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HubManager : MonoBehaviour
{
    [Header("Panel Config")]
    public PanelConfigManager panelConfig;

    [Header("Panel Options")]
    public OptionsManager panelOptions;

    [Header("Boutons")]
    public Button btnJouer;
    public Button btnPerso;
    public Button btnTuto;
    public Button btnOptions;
    public Button btnQuitter;

    [Header("Titre")]
    public TextMeshProUGUI textTitre;
    public float vitesseEcriture = 0.05f;

    [Header("Audio")]
    public AudioSource sourceMusique;
    public AudioSource sourceSFX;
    public AudioClip musicHub;
    public AudioClip sfxFocus;
    public AudioClip sfxValider;
    public AudioClip sfxRetour;

    // ── Raccourci localisation ────────────────────────────────────────────────
    string L(string cle, params string[] args)
        => LocalisationManager.Instance != null
            ? LocalisationManager.Instance.Get(cle, args)
            : cle;

    void Start()
    {
        // Musique
        if (sourceMusique != null && musicHub != null)
        {
            sourceMusique.clip = musicHub;
            sourceMusique.loop = true;
            sourceMusique.Play();
        }

        // Boutons
        btnJouer?.onClick.AddListener(AllerJouer);
        btnPerso?.onClick.AddListener(AllerPerso);
        btnTuto?.onClick.AddListener(AllerTuto);
        btnOptions?.onClick.AddListener(AllerOptions);
        btnQuitter?.onClick.AddListener(Quitter);

        // Titre animé traduit
        if (textTitre != null)
            StartCoroutine(EcrireTitre(L("hub_titre")));

        // Abonne au changement de langue pour le titre
        if (LocalisationManager.Instance != null)
            LocalisationManager.Instance.onTraductionsChargees
                += MettreAJourTitre;
    }

    void OnDestroy()
    {
        if (LocalisationManager.Instance != null)
            LocalisationManager.Instance.onTraductionsChargees
                -= MettreAJourTitre;
    }

    void MettreAJourTitre()
    {
        if (textTitre != null)
            StartCoroutine(EcrireTitre(L("hub_titre")));
    }

    // ── Titre effet craie ─────────────────────────────────────────────────────
    IEnumerator EcrireTitre(string texte)
    {
        textTitre.text = "";
        foreach (char c in texte)
        {
            textTitre.text += c;
            yield return new WaitForSeconds(vitesseEcriture);
        }
    }

    // ── Navigation ────────────────────────────────────────────────────────────
    public void JouerSFX(AudioClip clip)
    {
        if (sourceSFX != null && clip != null)
            sourceSFX.PlayOneShot(clip);
    }

    void AllerJouer()
    {
        JouerSFX(sfxValider);
        panelConfig?.Ouvrir();
    }

    void AllerPerso()
    {
        JouerSFX(sfxValider);
        SceneManager.LoadScene("Scene_Personnalisation");
    }

    void AllerTuto()
    {
        JouerSFX(sfxValider);
        SceneManager.LoadScene("Scene_Tuto");
    }

    void AllerOptions()
    {
        JouerSFX(sfxValider);
        panelOptions?.Ouvrir();
    }

    void Quitter()
    {
        JouerSFX(sfxValider);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}