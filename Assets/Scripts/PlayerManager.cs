using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static PlayerManager LocalPlayerInstance;

    //[Header("Object References")]

    private void Reset()
    {

    }

    private void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (PhotonNetwork.IsConnected)
        {
            if (photonView.IsMine)
            {
                LocalPlayerInstance = this;
            }
        }
        else
        {
            LocalPlayerInstance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    //// Start is called before the first frame update
    //void Start()
    //{
    //    
    //}
    //
    //// Update is called once per frame
    //void Update()
    //{
    //    
    //}
}
