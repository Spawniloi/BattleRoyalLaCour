using UnityEngine;

public static class SpriteFactory
{
    // Génère un sprite selon la forme et la couleur
    public static Sprite Creer(string forme, Color couleur)
    {
        return forme switch
        {
            "carre" => CreerCarre(couleur),
            "triangle" => CreerTriangle(couleur),
            "etoile" => CreerEtoile(couleur),
            "losange" => CreerLosange(couleur),
            _ => CreerCercle(couleur), // cercle par défaut
        };
    }

    // ── Cercle ────────────────────────────────────────────────────────────────
    static Sprite CreerCercle(Color couleur)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;

        Vector2 centre = new Vector2(size / 2f, size / 2f);
        float rayon = size / 2f - 2f;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), centre);
                tex.SetPixel(x, y, dist <= rayon ? couleur : Color.clear);
            }

        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 32f);
    }

    // ── Carré ─────────────────────────────────────────────────────────────────
    static Sprite CreerCarre(Color couleur)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                bool dedans = x >= 4 && x <= size - 4 &&
                              y >= 4 && y <= size - 4;
                tex.SetPixel(x, y, dedans ? couleur : Color.clear);
            }

        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 32f);
    }

    // ── Triangle ──────────────────────────────────────────────────────────────
    static Sprite CreerTriangle(Color couleur)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                // Triangle pointé vers le haut
                float progress = (float)y / size;
                float largeur = progress * (size - 8f);
                float centreX = size / 2f;
                bool dedans = y >= 4 &&
                              x >= centreX - largeur / 2f &&
                              x <= centreX + largeur / 2f;
                tex.SetPixel(x, y, dedans ? couleur : Color.clear);
            }

        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 32f);
    }

    // ── Losange ───────────────────────────────────────────────────────────────
    static Sprite CreerLosange(Color couleur)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;

        Vector2 centre = new Vector2(size / 2f, size / 2f);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dx = Mathf.Abs(x - centre.x) / (size / 2f - 2f);
                float dy = Mathf.Abs(y - centre.y) / (size / 2f - 2f);
                tex.SetPixel(x, y, (dx + dy) <= 1f ? couleur : Color.clear);
            }

        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 32f);
    }

    // ── Etoile ────────────────────────────────────────────────────────────────
    static Sprite CreerEtoile(Color couleur)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;

        Vector2 centre = new Vector2(size / 2f, size / 2f);
        float rayonExt = size / 2f - 2f;
        float rayonInt = rayonExt * 0.45f;
        int branches = 5;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                Vector2 p = new Vector2(x - centre.x, y - centre.y);
                float angle = Mathf.Atan2(p.y, p.x);
                float dist = p.magnitude;

                // Calcule le rayon de l'étoile à cet angle
                float angleNorm = (angle / (2 * Mathf.PI) * branches + 0.25f) % 1f;
                float t = Mathf.Abs(angleNorm * 2f - 1f);
                float rayonEtoile = Mathf.Lerp(rayonInt, rayonExt, 1f - t);

                tex.SetPixel(x, y, dist <= rayonEtoile ? couleur : Color.clear);
            }

        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 32f);
    }
}