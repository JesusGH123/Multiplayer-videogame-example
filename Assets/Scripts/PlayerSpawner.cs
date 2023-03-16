using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//Spawn players, kill them or respawn them
public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;

    public GameObject playerPrefab;
    private GameObject player;

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //Spawn the player only if the player is connected to the network
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    void Update()
    {
        
    }
}
