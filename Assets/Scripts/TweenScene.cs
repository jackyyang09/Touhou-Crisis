using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TweenScene : MonoBehaviourPunCallbacks
{
    [SerializeField] int sceneNumber = 2;

    void Awake()
    {
        if (PhotonNetwork.OfflineMode)
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneNumber);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // You're going to get here first, initialize hashtable
                var hashTable = new ExitGames.Client.Photon.Hashtable();
                hashTable.Add("MasterReady", true);
                PhotonNetwork.CurrentRoom.SetCustomProperties(hashTable);
            }
            else
            {
                InvokeRepeating("ClientConnectionProcedure", 1, 1);
            }
        }
    }

    void ClientConnectionProcedure()
    {
        var hashTable = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!hashTable.ContainsKey("MasterReady")) return;
        hashTable.Add("ClientReady", true);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hashTable);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (propertiesThatChanged.ContainsKey("MasterReady") && propertiesThatChanged.ContainsKey("ClientReady"))
        {
            if ((bool)propertiesThatChanged["MasterReady"] && (bool)propertiesThatChanged["ClientReady"])
            {
                PhotonNetwork.LoadLevel(sceneNumber);
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable());
            }
        }
    }
}