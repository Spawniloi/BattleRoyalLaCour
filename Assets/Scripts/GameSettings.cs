using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;
    public int playerCount = 1;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
