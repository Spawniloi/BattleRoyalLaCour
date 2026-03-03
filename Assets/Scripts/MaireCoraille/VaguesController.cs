using UnityEngine;

public class VaguesController : MonoBehaviour
{
    public float vitesseVagues = 1.0f;   // Vitesse de défilement
    public float opaciteMin = 0.3f;      // Opacité minimale
    public float opaciteMax = 0.9f;      // Opacité maximale
    public float variationOpaciteVitesse = 1.0f; // Vitesse de variation de l'opacité

    private GameObject[] vagues;        // Tableau pour les 3 sprites de vagues
    private float[] opaciteCibles;      // Opacité cible pour chaque vague
    private float[] opaciteActuelle;    // Opacité actuelle pour chaque vague
    private float spriteWidth;          // Largeur d'un sprite de vagues (à définir manuellement)

    void Start()
    {
        // Initialiser les vagues (doit correspondre à tes GameObjects dans la scène)
        vagues = new GameObject[3];
        vagues[0] = transform.Find("Vague_1").gameObject;
        vagues[1] = transform.Find("Vague_2").gameObject;
        vagues[2] = transform.Find("Vague_3").gameObject;

        // Largeur d'un sprite (à ajuster manuellement selon ton sprite)
        spriteWidth = 10.24f; // Exemple pour un sprite de 1024 pixels avec Pixels Per Unit = 100

        opaciteCibles = new float[3];
        opaciteActuelle = new float[3];

        for (int i = 0; i < 3; i++)
        {
            opaciteCibles[i] = Random.Range(opaciteMin, opaciteMax);
            opaciteActuelle[i] = opaciteCibles[i];

            SpriteRenderer sr = vagues[i].GetComponent<SpriteRenderer>();
            Color couleur = sr.color;
            couleur.a = opaciteActuelle[i];
            sr.color = couleur;
        }
    }

    void Update()
    {
        AnimerVagues();
        AnimerOpacite();
    }

    void AnimerVagues()
    {
        float deplacement = vitesseVagues * Time.deltaTime;

        for (int i = 0; i < vagues.Length; i++)
        {
            vagues[i].transform.Translate(new Vector3(-deplacement, 0f, 0f));

            // Si un sprite sort complètement à gauche, le replacer à droite
            if (vagues[i].transform.localPosition.x < -spriteWidth)
            {
                vagues[i].transform.localPosition = new Vector3(
                    2 * spriteWidth, // Position à droite des autres
                    vagues[i].transform.localPosition.y,
                    vagues[i].transform.localPosition.z
                );
            }
        }
    }

    void AnimerOpacite()
    {
        for (int i = 0; i < vagues.Length; i++)
        {
            SpriteRenderer sr = vagues[i].GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            // Si on a atteint la cible, en choisir une nouvelle
            if (Mathf.Approximately(opaciteActuelle[i], opaciteCibles[i]))
            {
                opaciteCibles[i] = Random.Range(opaciteMin, opaciteMax);
            }

            // Interpolation linéaire vers la nouvelle cible
            opaciteActuelle[i] = Mathf.Lerp(
                opaciteActuelle[i],
                opaciteCibles[i],
                variationOpaciteVitesse * Time.deltaTime
            );

            // Appliquer l'opacité
            Color couleur = sr.color;
            couleur.a = opaciteActuelle[i];
            sr.color = couleur;
        }
    }
}
