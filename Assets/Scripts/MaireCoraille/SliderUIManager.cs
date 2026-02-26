using UnityEngine;

public class SliderUIManager : MonoBehaviour
{
    [Header("Sliders — places manuellement dans la scene")]
    public SliderUI slider1;
    public SliderUI slider2;
    public SliderUI slider3;
    public SliderUI slider4;

    public void InitSliders(int nbJoueurs)
    {
        SliderUI[] sliders = { slider1, slider2, slider3, slider4 };

        for (int i = 0; i < 4; i++)
        {
            if (sliders[i] == null) continue;

            bool actif = i < nbJoueurs;
            sliders[i].gameObject.SetActive(actif);

            if (!actif) continue;

            // Assigne playerID et couleur seulement — pas de position
            sliders[i].playerID = i + 1;

            PlayerData data = GameData.GetJoueur(i + 1);
            sliders[i].SetCouleurJoueur(data.GetCouleur());
        }

        Debug.Log($"[SliderUI] {nbJoueurs} sliders initialisés");
    }
}