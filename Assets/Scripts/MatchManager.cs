using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MatchManager : MonoBehaviour
{
    public static MatchManager instance;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);  //The number of scene to load (menu)

        }
    }

    void Update()
    {
        
    }
}
