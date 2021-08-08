using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class CrosshairOnline : MonoBehaviourPun, IPunObservable
{
    [SerializeField] float interpSpeedMultiplier = 1;

    [SerializeField] UnityEngine.UI.Image image = null;

    [SerializeField] bool inGame = false;

    RailShooterLogic shooterLogic = null;

    RailShooterEffects shooterEffects = null;

    PlayerBehaviour player = null;
    PlayerUIManager uiManager = null;

    Vector2 remotePosition;

    float distance;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine) image.enabled = false;
    }

    private void OnEnable()
    {
        if (!photonView.IsMine) return;

        if (inGame)
        {
            player = PlayerManager.Instance.LocalPlayer;
            player.OnShotFired += SpawnRemoteEffectGame;

            shooterLogic = FindObjectOfType<RailShooterLogic>();
            shooterLogic.OnShoot += SpawnRemoteEffect;
        }
        else
        {
            shooterLogic = FindObjectOfType<RailShooterLogic>();
            shooterEffects = FindObjectOfType<RailShooterEffects>();
            shooterLogic.OnShoot += SpawnRemoteEffect;
        }
    }

    private void OnDisable()
    {
        if (!photonView.IsMine) return;

        if (inGame)
        {
            player.OnShotFired -= SpawnRemoteEffectGame;
            shooterLogic.OnShoot -= SpawnRemoteEffect;
        }

        if (shooterLogic)
        {
            shooterLogic.OnShoot -= SpawnRemoteEffect;
        }
    }

    private void OnDestroy()
    {
        Destroy(transform.parent.gameObject);
    }

    private void SpawnRemoteEffectGame(bool hit, Vector2 hitPosition)
    {
        hitPosition = new Vector2(hitPosition.x / Screen.width, hitPosition.y / Screen.height);
        photonView.RPC("SpawnEffectGame", RpcTarget.Others, new object[] { hit, hitPosition });
    }

    private void SpawnRemoteEffect(Ray arg1, Vector2 arg2)
    {
        arg2 = new Vector2(arg2.x / Screen.width, arg2.y / Screen.height);
        photonView.RPC("SpawnEffect", RpcTarget.Others, new object[] { arg2 });
    }

    [PunRPC]
    void SpawnEffectGame(bool hit, Vector2 hitPosition)
    {
        if (!uiManager)
        {
            uiManager = FindObjectOfType<PlayerUIManager>();
        }
        if (uiManager)
        {
            uiManager.SpawnMuzzleFlash(hit, new Vector2(hitPosition.x * Screen.width, hitPosition.y * Screen.height));
        }
    }

    [PunRPC]
    void SpawnEffect(Vector2 normalizedPos)
    {
        if (!shooterEffects)
        {
            shooterEffects = FindObjectOfType<RailShooterEffects>();
        }
        shooterEffects.PlayEffect(new Ray(), new Vector2(normalizedPos.x * Screen.width, normalizedPos.y * Screen.height));
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.OfflineMode && PhotonNetwork.IsConnected)
        {
            if (!photonView.IsMine)
            {
                transform.position = Vector3.MoveTowards(transform.position, remotePosition, distance * (1.0f / PhotonNetwork.SerializationRate) * interpSpeedMultiplier);
                return;
            }
        }

        // Why this works: 
        // https://answers.unity.com/questions/849117/46-ui-image-follow-mouse-position.html?_ga=2.45598500.148015968.1612849553-1895421686.1612849553
#if UNITY_ANDROID && !UNITY_EDITOR
        transform.position = Input.touches[Input.touchCount - 1].position;
#else
        transform.position = Input.mousePosition;
#endif
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            float x = Input.mousePosition.x / Screen.width;
            float y = Input.mousePosition.y / Screen.height;
            stream.SendNext(new Vector2(x, y));
        }
        else
        {
            // Network player, receive data
            Vector2 normalizedPosition = (Vector2)stream.ReceiveNext();
            remotePosition = new Vector2(normalizedPosition.x * Screen.width, normalizedPosition.y * Screen.height);
            distance = Vector2.Distance(transform.position, remotePosition);
        }
    }
}
