using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] GameObject playerPrefab;

    [SerializeField] float gameTimer = 0;
    float remoteGameTimer = 0;
    bool gameTimerEnabled = false;
    public float GameTimeElapsed
    {
        get
        {
            return gameTimer;
        }
    }

    [SerializeField] float timeBetweenSync = 3;

    static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }

    Sakuya sakuya;
    PlayerBehaviour player;

    private void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (!PlayerManager.Instance)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene().name);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                var newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
                player = newPlayer.GetComponent<PlayerBehaviour>();
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        player.OnPlayerDeath += StopGameTimer;
    }

    new void OnEnable()
    {
        base.OnEnable();
        AreaLogic.OnEnterFirstArea += StartGameTimer;

        sakuya = FindObjectOfType<Sakuya>();
        if (sakuya != null)
        {
            sakuya.OnBossDefeat += StopGameTimer;
        }
    }

    new void OnDisable()
    {
        base.OnDisable();
        AreaLogic.OnEnterFirstArea -= StartGameTimer;

        if (sakuya != null)
        {
            sakuya.OnBossDefeat -= StopGameTimer;
        }

        player.OnPlayerDeath -= StopGameTimer;
    }

    public void StartGameTimer()
    {
        gameTimerEnabled = true;

        if (!PhotonNetwork.IsMasterClient)
        {
            InvokeRepeating("SyncRemoteProperties", 0, timeBetweenSync);
        }
    }

    public void StopGameTimer()
    {
        gameTimerEnabled = false;

        // Cancel replication and manually perform it one last time
        if (IsInvoking("SyncRemoteProperties"))
        {
            CancelInvoke("SyncRemoteProperties");
            InvokeRepeating("SyncRemoteProperties", 0, 1);
        }
    }

    private void Update()
    {
        if (gameTimerEnabled)
        {
            gameTimer += Time.deltaTime;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            // We own this player: send the others our data
            stream.SendNext(gameTimer);
        }
        else
        {
            // Network player, receive data
            remoteGameTimer = (float)stream.ReceiveNext();
        }
    }

    void SyncRemoteProperties()
    {
        gameTimer = remoteGameTimer;
    }

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        PhotonNetwork.LoadLevel("Room for 2");
    }
    
    public void LoadScene(int id)
    {
        SceneManager.LoadSceneAsync(id);
    }

    public void ReloadScene()
    {
        PhotonNetwork.LoadLevel(1);
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            //LoadArena();
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            //LoadArena();
            PhotonNetwork.LoadLevel(0);
            LeaveRoom();
        }
    }
}