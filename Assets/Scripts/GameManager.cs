using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] GameObject playerPrefab = null;

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
    [SerializeField] int shotsFired = 0;
    [SerializeField] int shotsHit = 0;

    [SerializeField] GameObject crosshairPrefab = null;

    Sakuya sakuya;
    PlayerBehaviour player;

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

    public static Action<PlayerBehaviour> OnSpawnLocalPlayer;

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
                OnSpawnLocalPlayer?.Invoke(player);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        PhotonNetwork.Instantiate(crosshairPrefab.name, Vector3.zero, Quaternion.identity);
        player.OnTakeDamage += DamageRemotePlayer;
        player.OnPlayerDeath += SyncLoseSequence;
        player.OnPlayerDeath += StopGameTimer;
        player.OnBulletFired += CountPlayerShot;
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

        player.OnPlayerDeath -= SyncLoseSequence;
        player.OnPlayerDeath -= StopGameTimer;
        player.OnBulletFired -= CountPlayerShot;
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

    void CountPlayerShot(bool miss, Vector2 hitPosition)
    {
        if (!miss) shotsHit++;
        shotsFired++;
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

    Coroutine loseRoutine = null;
    void SyncLoseSequence()
    {
        loseRoutine = StartCoroutine(SyncLoseRoutine());
    }

    IEnumerator SyncLoseRoutine()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        photonView.RPC("InitiateLoseSequence", RpcTarget.All);
    }

    [PunRPC]
    void InitiateLoseSequence()
    {
        FindObjectOfType<PlayerUIManager>().LoseSequence();
    }

    private void DamageRemotePlayer(DamageType obj)
    {
        photonView.RPC("RPCDamageRemotePlayer", RpcTarget.Others);
    }
    
    [PunRPC]
    void RPCDamageRemotePlayer()
    {
        player.TakeDamageRemote();
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
        PhotonNetwork.LoadLevel(2);
    }
    
    public void LoadScene(int id)
    {
        SceneManager.LoadSceneAsync(id);
    }

    public void ReloadScene()
    {
        photonView.RPC("RemoteReloadScene", RpcTarget.All);
    }

    Coroutine reloadSceneRoutine;
    [PunRPC]
    void RemoteReloadScene()
    {
        if (reloadSceneRoutine != null) return;
        reloadSceneRoutine = StartCoroutine(ReloadSceneRoutine());
    }

    IEnumerator ReloadSceneRoutine()
    {
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.MenuButton);

        LoadingScreen loadScreen = FindObjectOfType<LoadingScreen>();

        yield return StartCoroutine(loadScreen.ShowRoutine());

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
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        //    //LoadArena();
        //    PhotonNetwork.LoadLevel(0);
        //    LeaveRoom();
        //}

        // lmao just leave dude
        LeaveRoom();
        SceneManager.LoadScene(0);
    }
}