using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable, IReloadable
{
    [SerializeField] GameObject hostPrefab = null;
    [SerializeField] GameObject clientPrefab = null;

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

    [SerializeField] GameObject crosshairPrefab = null;

    int rematchRequests = 0;

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
    public static Action OnReceiveRematchRequest;
    public static Action OnReloadScene;
    public static Action OnQuitToMenu;
    
    public void Reinitialize()
    {
        gameTimer = 0;
        remoteGameTimer = 0;
        rematchRequests = 0;
        gameOverRoutine = null;
    }

    private void Start()
    {
        if (hostPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (!PlayerManager.Instance)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene().name);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                GameObject newPlayer;
                if (PhotonNetwork.IsMasterClient)
                {
                    newPlayer = PhotonNetwork.Instantiate(hostPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
                }
                else
                {
                    newPlayer = PhotonNetwork.Instantiate(clientPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
                }
                player = newPlayer.GetComponent<PlayerBehaviour>();
                OnSpawnLocalPlayer?.Invoke(player);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        if (!player)
        {
            player = PlayerManager.Instance.LocalPlayer;
        }

        if (GameplayModifiers.Instance.GameMode == GameplayModifiers.GameModes.Coop)
        {
            PhotonNetwork.Instantiate(crosshairPrefab.name, Vector3.zero, Quaternion.identity);
        }

        player.OnTakeDamage += DamageRemotePlayer;
        player.OnPlayerDeath += SyncLoseSequence;

        Reinitialize();

        SoftSceneReloader.Instance.AddNewReloadable(this);
    }

    new void OnEnable()
    {
        base.OnEnable();
        AreaLogic.OnEnterFirstArea += StartGameTimer;

        sakuya = FindObjectOfType<Sakuya>();
        if (sakuya != null)
        {
            sakuya.OnBossDefeat += SyncWinSequence;
        }
    }

    new void OnDisable()
    {
        base.OnDisable();
        AreaLogic.OnEnterFirstArea -= StartGameTimer;

        if (sakuya != null)
        {
            sakuya.OnBossDefeat -= SyncWinSequence;
        }

        if (player != null)
        {
            player.OnPlayerDeath -= SyncLoseSequence;
        }
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

    Coroutine gameOverRoutine = null;
    void SyncWinSequence()
    {
        if (gameOverRoutine == null)
        {
            gameOverRoutine = StartCoroutine(SyncWinRoutine());
        }
    }

    IEnumerator SyncWinRoutine()
    {
        yield return new WaitForSecondsRealtime(1f);
        photonView.RPC("InitiateWinSequence", RpcTarget.All);
    }

    void SyncLoseSequence()
    {
        if (gameOverRoutine == null)
        {
            gameOverRoutine = StartCoroutine(SyncLoseRoutine());
        }
        else
        {
            Debug.Log("wtf?");
        }
    }

    IEnumerator SyncLoseRoutine()
    {
        yield return new WaitForSecondsRealtime(1f);
        photonView.RPC("InitiateLoseSequence", RpcTarget.All);
    }

    [PunRPC]
    void InitiateWinSequence()
    {
        FindObjectOfType<PlayerUIManager>().WinSequence();
        StopGameTimer();
    }

    [PunRPC]
    void InitiateLoseSequence()
    {
        FindObjectOfType<PlayerUIManager>().LoseSequence();
        StopGameTimer();
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

    public void SyncRequestRematch()
    {
        photonView.RPC("RequestRematch", RpcTarget.All);
    }

    [PunRPC]
    public void RequestRematch()
    {
        rematchRequests++;
        OnReceiveRematchRequest?.Invoke();
        if (rematchRequests == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            ReloadScene();
        }
    }

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        OnQuitToMenu?.Invoke();
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
        OnReloadScene?.Invoke();

        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.MenuButton);

        LoadingScreen loadScreen = FindObjectOfType<LoadingScreen>();

        yield return StartCoroutine(loadScreen.ShowRoutine());

        yield return new WaitForSeconds(1);

        SoftSceneReloader.Instance.ExecuteSoftReload();
        
        yield return StartCoroutine(loadScreen.HideRoutine());

        // Reinitialize doesn't get called by SoftSceneReloader here for some reason?
        Reinitialize();
        reloadSceneRoutine = null;

        //PhotonNetwork.LoadLevel(1);
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
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other player disconnects
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        //    //LoadArena();
        //    PhotonNetwork.LoadLevel(0);
        //    LeaveRoom();
        //}

        // lmao just leave dude
        LeaveRoom();
    }
}