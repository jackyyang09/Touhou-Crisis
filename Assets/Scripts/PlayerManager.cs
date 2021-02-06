using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static PlayerManager Instance;

    public PlayerBehaviour HostPlayer;

    PlayerBehaviour localPlayer;
    public PlayerBehaviour LocalPlayer
    {
        get
        {
            if (localPlayer == null)
            {
                var players = FindObjectsOfType<PlayerBehaviour>();
                for (int i = 0; i < players.Length; i++)
                {
                    var pView = players[i].GetComponent<PhotonView>();
                    if (pView.IsMine)
                    {
                        localPlayer = players[i];
                    }
                }
            }
            return localPlayer;
        }
    }

    PhotonView otherPlayer;
    public PhotonView OtherPlayer
    {
        get
        {
            if (otherPlayer == null)
            {
                var players = FindObjectsOfType<PlayerBehaviour>();
                for (int i = 0; i < players.Length; i++)
                {
                    var pView = players[i].GetComponent<PhotonView>();
                    if (!pView.IsMine) otherPlayer = pView;
                }
            }
            return otherPlayer;
        }
    }

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
                Instance = this;
            }
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);

        var players = FindObjectsOfType<PlayerBehaviour>();
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].GetComponent<PhotonView>().IsMine)
                {
                    HostPlayer = players[i];
                }
            }
        }
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
