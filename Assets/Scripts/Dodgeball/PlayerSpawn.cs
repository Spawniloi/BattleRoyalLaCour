using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawn : MonoBehaviour
{
    [Header("Player Prefabs")]
    public GameObject playerPrefab; 

    [Header("Team Zones")]
    public TeamZones[] teamZones; 
    public Transform[] spawnPoints; 

    private int nextTeamIndex = 0;

    public void SpawnPlayer(InputDevice device)
    {
        if (nextTeamIndex >= teamZones.Length)
        {
            Debug.LogWarning("Toutes les équipes sont déjà assignées");
            return;
        }

        GameObject playerGO = Instantiate(playerPrefab);

        PlayerInput playerInput = playerGO.GetComponent<PlayerInput>();
        playerInput.SwitchCurrentControlScheme(device);

        PlayerManager pm = playerGO.GetComponent<PlayerManager>();

        pm.teamID = nextTeamIndex;

        TeamZones zone = teamZones[nextTeamIndex];
        Vector3 spawnPos = (Vector3)zone.GetRandomSpawnPosition();
        playerGO.transform.position = spawnPos;

        foreach (Unit unit in pm.units)
        {
            unit.teamID = nextTeamIndex;
            unit.transform.position = (Vector3)zone.GetRandomSpawnPosition();
        }

        nextTeamIndex++;
    }
}