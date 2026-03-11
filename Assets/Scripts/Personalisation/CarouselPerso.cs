using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarouselPerso : MonoBehaviour
{
    [Header("Contenu")]
    public RectTransform contenu;
    public List<GameObject> items = new List<GameObject>();

    [Header("Tailles")]
    public float tailleNormal = 80f;
    public float tailleSelecte = 120f;
    public float espacement = 20f;

    [Header("Couleurs")]
    public Color couleurNormal = new Color(1f, 1f, 1f, 0.5f);
    public Color couleurSelecte = Color.white;
    public Color couleurGrise = new Color(0.4f, 0.4f, 0.4f, 0.3f);

    public System.Action<int> onSelectionChange;

    private int indexActuel = 0;
    private bool enAnimation = false;
    private ScrollRect scrollRect;

    // Garde en mémoire les couleurs de base (peau/dossard)
    private List<Color> couleursBase = new List<Color>();

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = false;
            scrollRect.scrollSensitivity = 0f;
        }
    }

    public void Init(int indexDepart = 0)
    {
        indexActuel = Mathf.Clamp(indexDepart, 0, items.Count - 1);

        // Mémorise couleurs de base assignées dans l'Inspector
        couleursBase.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            Image img = items[i].GetComponent<Image>();
            couleursBase.Add(img != null ? img.color : Color.white);
        }

        PlacerItemsInstant();
        MettreAJourVisuels();
    }

    // ── Place tous les items instantanément ───────────────────────────────────
    void PlacerItemsInstant()
    {
        if (items.Count == 0) return;

        float x = 0f;
        for (int i = 0; i < items.Count; i++)
        {
            RectTransform rt = items[i].GetComponent<RectTransform>();
            float taille = (i == indexActuel)
                ? tailleSelecte : tailleNormal;

            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(taille, taille);
            rt.anchoredPosition = new Vector2(x + taille * 0.5f, 0f);
            x += taille + espacement;
        }

        contenu.sizeDelta = new Vector2(x - espacement, contenu.sizeDelta.y);

        Canvas.ForceUpdateCanvases();
        CentrerSurIndex(indexActuel, false);
    }

    // ── Recalcule toutes les positions (après anim taille) ────────────────────
    void RecalculerPositions()
    {
        float x = 0f;
        for (int i = 0; i < items.Count; i++)
        {
            RectTransform rt = items[i].GetComponent<RectTransform>();
            float taille = rt.sizeDelta.x; // taille actuelle
            rt.anchoredPosition = new Vector2(x + taille * 0.5f, 0f);
            x += taille + espacement;
        }
        contenu.sizeDelta = new Vector2(x - espacement, contenu.sizeDelta.y);
    }

    // ── Centrage ──────────────────────────────────────────────────────────────
    void CentrerSurIndex(int index, bool anime)
    {
        if (scrollRect == null || items.Count == 0) return;

        Canvas.ForceUpdateCanvases();

        RectTransform rt = items[index].GetComponent<RectTransform>();
        float viewW = scrollRect.viewport.rect.width;
        float contentW = contenu.rect.width;

        if (contentW <= viewW)
        {
            if (!anime)
                scrollRect.horizontalNormalizedPosition = 0.5f;
            return;
        }

        float itemCenterX = rt.anchoredPosition.x;
        float scroll = (itemCenterX - viewW * 0.5f)
                          / (contentW - viewW);
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
        float duree = 0.2f;

        while (timer < duree)
        {
            float p = Mathf.SmoothStep(0f, 1f, timer / duree);
            scrollRect.horizontalNormalizedPosition =
                Mathf.Lerp(depart, cible, p);
            timer += Time.deltaTime;
            yield return null;
        }

        scrollRect.horizontalNormalizedPosition = cible;
        enAnimation = false;
    }

    IEnumerator AnimerTaille(RectTransform rt, float cible, float duree)
    {
        float depart = rt.sizeDelta.x;
        float timer = 0f;

        while (timer < duree)
        {
            float p = Mathf.SmoothStep(0f, 1f, timer / duree);
            float val = Mathf.Lerp(depart, cible, p);
            rt.sizeDelta = new Vector2(val, val);

            // Recalcule positions à chaque frame pendant l'anim
            RecalculerPositions();
            timer += Time.deltaTime;
            yield return null;
        }

        rt.sizeDelta = new Vector2(cible, cible);
        RecalculerPositions();
    }

    // ── Navigation ────────────────────────────────────────────────────────────
    public void Suivant()
    {
        if (enAnimation) return;
        AllerVers((indexActuel + 1) % items.Count);
    }

    public void Precedent()
    {
        if (enAnimation) return;
        AllerVers((indexActuel - 1 + items.Count) % items.Count);
    }

    void AllerVers(int index)
    {
        enAnimation = true;

        RectTransform rtAncien = items[indexActuel]
            .GetComponent<RectTransform>();
        StartCoroutine(AnimerTaille(rtAncien, tailleNormal, 0.15f));

        indexActuel = index;

        RectTransform rtNouveau = items[indexActuel]
            .GetComponent<RectTransform>();
        StartCoroutine(AnimerTaille(rtNouveau, tailleSelecte, 0.15f));

        MettreAJourVisuels();

        // Centre après que l'anim de taille soit finie
        StartCoroutine(CentrerApresAnim());
        onSelectionChange?.Invoke(indexActuel);
    }

    IEnumerator CentrerApresAnim()
    {
        // Attend la fin de l'anim taille
        yield return new WaitForSeconds(0.15f);
        RecalculerPositions();
        Canvas.ForceUpdateCanvases();
        CentrerSurIndex(indexActuel, true);
    }

    // ── Visuels ───────────────────────────────────────────────────────────────
    void MettreAJourVisuels()
    {
        for (int i = 0; i < items.Count; i++)
        {
            Image img = items[i].GetComponent<Image>();
            if (img == null) continue;

            if (i == indexActuel)
            {
                // Sélectionné — couleur de base mais alpha 1
                Color c = couleursBase.Count > i
                    ? couleursBase[i] : couleurSelecte;
                c.a = 1f;
                img.color = c;
            }
            else
            {
                // Non sélectionné — couleur de base mais alpha 0.5
                Color c = couleursBase.Count > i
                    ? couleursBase[i] : couleurNormal;
                c.a = 0.4f;
                img.color = c;
            }
        }
    }

    // ── Grisé (dossard pris) ──────────────────────────────────────────────────
    public void SetGrise(int index, bool grise)
    {
        if (index < 0 || index >= items.Count) return;
        Image img = items[index].GetComponent<Image>();
        if (img == null) return;

        if (grise)
        {
            img.color = couleurGrise;
        }
        else
        {
            Color c = couleursBase.Count > index
                ? couleursBase[index] : couleurNormal;
            c.a = index == indexActuel ? 1f : 0.4f;
            img.color = c;
        }
    }

    public int GetIndex() => indexActuel;

    public void SetIndex(int index)
    {
        if (enAnimation) return;
        indexActuel = Mathf.Clamp(index, 0, items.Count - 1);
        PlacerItemsInstant();
        MettreAJourVisuels();
    }
}