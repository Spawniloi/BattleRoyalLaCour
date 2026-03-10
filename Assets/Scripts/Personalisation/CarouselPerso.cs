using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarouselPerso : MonoBehaviour
{
    [Header("Contenu")]
    public RectTransform contenu;       // enfant du ScrollRect
    public List<GameObject> items = new List<GameObject>();

    [Header("Tailles")]
    public float tailleNormal = 80f;
    public float tailleSelecte = 120f;
    public float espacement = 20f;
    public float vitesseAnim = 8f;

    [Header("Couleurs")]
    public Color couleurNormal = new Color(1f, 1f, 1f, 0.5f);
    public Color couleurSelecte = Color.white;
    public Color couleurGrise = new Color(0.4f, 0.4f, 0.4f, 0.3f);

    private int indexActuel = 0;
    private float cibleX = 0f;
    private bool enAnimation = false;
    private ScrollRect scrollRect;

    // Callback quand sélection change
    public System.Action<int> onSelectionChange;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            // Désactive le scroll manuel — contrôlé par code
            scrollRect.horizontal = false;
            scrollRect.vertical = false;
            scrollRect.scrollSensitivity = 0f;
        }
    }

    public void Init(int indexDepart = 0)
    {
        indexActuel = indexDepart;
        PlacerItemsInstant();
        MettreAJourVisuels();
    }

    // ── Place tous les items sans animation ───────────────────────────────────
    void PlacerItemsInstant()
    {
        float largeurTotale = CalculerLargeurTotale();
        contenu.sizeDelta = new Vector2(largeurTotale, contenu.sizeDelta.y);

        float x = 0f;
        for (int i = 0; i < items.Count; i++)
        {
            RectTransform rt = items[i].GetComponent<RectTransform>();
            float taille = (i == indexActuel)
                ? tailleSelecte : tailleNormal;

            rt.sizeDelta = new Vector2(taille, taille);
            rt.anchoredPosition = new Vector2(x + taille * 0.5f, 0f);
            x += taille + espacement;
        }

        CentrerSurIndex(indexActuel, false);
    }

    float CalculerLargeurTotale()
    {
        float total = 0f;
        for (int i = 0; i < items.Count; i++)
        {
            total += (i == indexActuel)
                ? tailleSelecte : tailleNormal;
            if (i < items.Count - 1)
                total += espacement;
        }
        return total;
    }

    // ── Navigation ────────────────────────────────────────────────────────────
    public void Suivant()
    {
        if (enAnimation) return;
        int nouveau = (indexActuel + 1) % items.Count;
        AllerVers(nouveau);
    }

    public void Precedent()
    {
        if (enAnimation) return;
        int nouveau = (indexActuel - 1 + items.Count) % items.Count;
        AllerVers(nouveau);
    }

    void AllerVers(int index)
    {
        indexActuel = index;
        MettreAJourVisuels();
        StartCoroutine(AnimerVers(index));
        onSelectionChange?.Invoke(index);
    }

    // ── Animation smooth ──────────────────────────────────────────────────────
    IEnumerator AnimerVers(int index)
    {
        enAnimation = true;

        // Recalcule positions avec nouveau sélectionné
        float x = 0f;
        float cibleXItem = 0f;

        for (int i = 0; i < items.Count; i++)
        {
            RectTransform rt = items[i].GetComponent<RectTransform>();
            float taille = (i == index)
                ? tailleSelecte : tailleNormal;

            // Anime la taille
            StartCoroutine(AnimerTaille(rt, taille));

            if (i == index)
                cibleXItem = x + taille * 0.5f;

            x += taille + espacement;
        }

        // Recalcule largeur totale
        float nouvelleLargeur = x - espacement;
        contenu.sizeDelta = new Vector2(
            nouvelleLargeur, contenu.sizeDelta.y);

        // Centre sur l'item sélectionné
        CentrerSurIndex(index, true);

        yield return new WaitForSeconds(0.3f);
        enAnimation = false;
    }

    IEnumerator AnimerTaille(RectTransform rt, float cible)
    {
        float depart = rt.sizeDelta.x;
        float timer = 0f;
        float duree = 0.2f;

        while (timer < duree)
        {
            float p = timer / duree;
            float smooth = Mathf.SmoothStep(0f, 1f, p);
            float val = Mathf.Lerp(depart, cible, smooth);
            rt.sizeDelta = new Vector2(val, val);
            timer += Time.deltaTime;
            yield return null;
        }

        rt.sizeDelta = new Vector2(cible, cible);
    }

    void CentrerSurIndex(int index, bool anime)
    {
        if (scrollRect == null) return;

        // Position de l'item dans le contenu
        RectTransform rt = items[index].GetComponent<RectTransform>();
        float itemX = rt.anchoredPosition.x;
        float viewW = scrollRect.viewport.rect.width;
        float contentW = contenu.rect.width;

        // Calcule le scroll normalisé pour centrer l'item
        float scroll = (itemX - viewW * 0.5f) / (contentW - viewW);
        scroll = Mathf.Clamp01(scroll);

        if (anime)
            StartCoroutine(AnimerScroll(scroll));
        else
            scrollRect.horizontalNormalizedPosition = scroll;
    }

    IEnumerator AnimerScroll(float cible)
    {
        float depart = scrollRect.horizontalNormalizedPosition;
        float timer = 0f;
        float duree = 0.25f;

        while (timer < duree)
        {
            float p = timer / duree;
            float smooth = Mathf.SmoothStep(0f, 1f, p);
            scrollRect.horizontalNormalizedPosition =
                Mathf.Lerp(depart, cible, smooth);
            timer += Time.deltaTime;
            yield return null;
        }

        scrollRect.horizontalNormalizedPosition = cible;
    }

    // ── Visuels ───────────────────────────────────────────────────────────────
    void MettreAJourVisuels()
    {
        for (int i = 0; i < items.Count; i++)
        {
            Image img = items[i].GetComponent<Image>();
            if (img == null) continue;

            img.color = i == indexActuel
                ? couleurSelecte : couleurNormal;
        }
    }

    // Grise un item — pris par un autre joueur
    public void SetGrise(int index, bool grise)
    {
        if (index < 0 || index >= items.Count) return;

        Image img = items[index].GetComponent<Image>();
        if (img == null) return;

        img.color = grise ? couleurGrise
            : (index == indexActuel ? couleurSelecte : couleurNormal);
    }

    public int GetIndex() => indexActuel;

    public void SetIndex(int index)
    {
        indexActuel = Mathf.Clamp(index, 0, items.Count - 1);
        PlacerItemsInstant();
        MettreAJourVisuels();
    }
}