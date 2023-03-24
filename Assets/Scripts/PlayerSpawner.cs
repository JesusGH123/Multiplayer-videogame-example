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

    public GameObject spawnEffect;

    public float respawnTime = 5f;

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    public void Die(string damager)
    {
        UIController.instance.deathText.text = "You were killed by " + damager;

        if(player != null)
        {
            StartCoroutine(DieCorroutine());
        }
    }

    public IEnumerator DieCorroutine()
    {
        PhotonNetwork.Instantiate(spawnEffect.name, player.transform.position, Quaternion.identity);

        PhotonNetwork.Destroy(player);
        UIController.instance.deathScreen.SetActive(true);
        yield return new WaitForSeconds(respawnTime);
        UIController.instance.deathScreen.SetActive(false);
        SpawnPlayer();
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
