using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RailShooterLogic : MonoBehaviour
{
    [SerializeField] bool offlineMode = false;

    [SerializeField] KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode FireKey { get { return fireKey; } }

    [SerializeField] float fullAutoAssistFireDelay = 0.5f;
    public bool EnableFullAutoAssist = false;
    bool canShoot = true;
    void ReEnableShooting() => canShoot = true;

    [Header("Object References")]

    [SerializeField] Cinemachine.CinemachineVirtualCamera vCam;
    new Camera camera;
    public Camera Cam
    {
        get
        {
            if (camera == null)
            {
                camera = Camera.main;
            }
            return camera;
        }
    }

    [SerializeField] public PhotonView photonView;

    /// <summary>
    /// Ray rayFromCursor, Vector2 cursorPosition
    /// </summary>
    public System.Action<Ray, Vector2> OnShoot;
    public System.Action OnTriggerDown;
    public System.Action OnTriggerUp;

    private void Awake()
    {
        if (offlineMode) return;
        if (PhotonNetwork.IsConnected)
        {
            if (!photonView.IsMine)
            {
                Destroy(vCam.gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!offlineMode && PhotonNetwork.IsConnected)
        {
            if (!photonView.IsMine) return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            ShootBehaviour();
        }
#else
        if (canShoot)
        {
            if (Input.GetKeyDown(fireKey))
            {
                ShootBehaviour();
            }

            if (EnableFullAutoAssist)
            {
                if (Input.GetKey(fireKey) && canShoot)
                {
                    ShootBehaviour();
                }
            }
        }

        if (Input.GetKeyUp(fireKey))
        {
            CancelInvoke(nameof(ReEnableShooting));
            canShoot = true;
        }

        if (Input.GetKey(fireKey))
        {
            OnTriggerDown?.Invoke();
        }
        else if (Input.GetKeyUp(fireKey))
        {
            OnTriggerUp?.Invoke();
        }
#endif
    }

    public Vector2 GetCursorPosition()
    {
        Vector2 screenPoint = Vector2.zero;
#if UNITY_ANDROID && !UNITY_EDITOR
        screenPoint = Input.touches[Input.touchCount - 1].position;
#else
        screenPoint = Input.mousePosition;
#endif
        return screenPoint;
    }

    public Ray FireRay()
    {
        return Cam.ScreenPointToRay(GetCursorPosition());
    }

    /// <summary>
    /// Helper method to be used by external classes
    /// </summary>
    /// <param name="screenPoint"></param>
    /// <returns></returns>
    public Ray GetRayFromScreenPoint(Vector2 screenPoint)
    {
        return Cam.ScreenPointToRay(screenPoint);
    }

    void ShootBehaviour()
    {
        Vector2 screenPoint = GetCursorPosition();
        OnShoot?.Invoke(FireRay(), screenPoint);
        if (EnableFullAutoAssist)
        {
            canShoot = false;
            Invoke(nameof(ReEnableShooting), fullAutoAssistFireDelay);
        }
    }

    public void RebindFireKey(KeyCode newKey)
    {
        fireKey = newKey;
    }
}