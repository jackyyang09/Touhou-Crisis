using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField] string gameVersion = "1";

    /// <summary>
    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    /// </summary>
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 4;

    [Tooltip("The UI Panel to let the user enter name, connect and play")]
    [SerializeField] OptimizedCanvas controlPanel;

    [SerializeField] TMPro.TextMeshProUGUI progressLabel;

    [SerializeField] OptimizedCanvas lobbyScreen;
    [SerializeField] Image multiplayerButton;

    [SerializeField] TMPro.TMP_InputField inputField;
    [SerializeField] TMPro.TextMeshProUGUI roomCode;

    [SerializeField] TMPro.TextMeshProUGUI player1NameText;
    [SerializeField] TMPro.TextMeshProUGUI player2NameText;

    public bool quickPlay = false;

    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    bool isConnecting;

    private void Awake()
    {
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client
        // and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        RandomizeRoomCode();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    void RandomizeRoomCode()
    {
        inputField.text = Random.Range(0, 9999).ToString("0000");
    }

    public void Connect()
    {
        ShowConnectingText();

        // Check if we are connected or not, join if we are, else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // We need at this point to attempt joining a Random Room.
            // If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
        Debug.Log("Trying to connect...");
        quickPlay = false;
    }

    public void QuickPlay()
    {
        ShowConnectingText();

        // Check if we are connected or not, join if we are, else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // We need at this point to attempt joining a Random Room.
            // If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
        Debug.Log("Trying to connect...");
        quickPlay = true;
    }

    public override void OnConnectedToMaster()
    {
        // we don't want to do anything if we are not attempting to join a room.
        // this case where isConnecting is false is typically when you lost or quit the game,
        // when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isConnecting)
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            // first try to join a potential existing room. If there is, good
            // else, we'll be called back with OnJoinRandomFailed()

            if (inputField.text == string.Empty)
            {
                RandomizeRoomCode();
            }

            if (quickPlay)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                PhotonNetwork.JoinOrCreateRoom(inputField.text, new RoomOptions { MaxPlayers = maxPlayersPerRoom }, TypedLobby.Default);
            }

            isConnecting = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        HideConnectingText();
        controlPanel.Show();
        isConnecting = false;
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        RandomizeRoomCode();
        string name = inputField.text;
        PhotonNetwork.CreateRoom(name, new RoomOptions { MaxPlayers = maxPlayersPerRoom, IsOpen = true });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

        // Only load if we are the first player
        // else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("Loading the 'Room for 1' ");
            player1NameText.text = PhotonNetwork.LocalPlayer.NickName;
            roomCode.text = PhotonNetwork.CurrentRoom.Name;
        }
        else
        {
            player1NameText.text = PhotonNetwork.MasterClient.NickName;
            roomCode.text = PhotonNetwork.CurrentRoom.Name;
        }
        HideConnectingText();
        lobbyScreen.Show();
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            multiplayerButton.raycastTarget = true;
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            multiplayerButton.raycastTarget = false;
            player1NameText.text = PhotonNetwork.LocalPlayer.NickName;
        }
    }

    /// <summary>
    /// Copied from GameManager lol
    /// </summary>
    public void Load2PlayerMode()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("Room for 2");
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    Coroutine connectingTextRoutine;

    void ShowConnectingText()
    {
        progressLabel.enabled = true;
        if (connectingTextRoutine == null)
        {
            connectingTextRoutine = StartCoroutine(AnimateConnectingText());
        }
    }

    void HideConnectingText()
    {
        if (connectingTextRoutine != null)
        {
            StopCoroutine(connectingTextRoutine);
            connectingTextRoutine = null;
            progressLabel.enabled = false;
        }
    }

    IEnumerator AnimateConnectingText()
    {
        int ellipsesCount = 1;
        int maxDots = 4;
        while (progressLabel.enabled)
        {
            progressLabel.text = "Connecting";
            for (int i = 0; i < ellipsesCount; i++)
            {
                progressLabel.text += ".";
            }
            ellipsesCount = (int)Mathf.Repeat(ellipsesCount + 1, maxDots);
            yield return new WaitForSeconds(0.25f);
        }
        connectingTextRoutine = null;
    }
}
