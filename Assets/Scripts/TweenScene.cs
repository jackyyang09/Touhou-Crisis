using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TweenScene : MonoBehaviour
{
    [SerializeField] int sceneNumber = 2;

    void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(sceneNumber);
        }
    }
}
