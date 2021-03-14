using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RailShooterLogic : MonoBehaviour
{
    [SerializeField] bool offlineMode = false;

    [SerializeField] KeyCode fireKey = KeyCode.Mouse0;

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

    public System.Action<Ray, Vector2> OnShoot;

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

        //fireKey = (KeyCode)PlayerPrefs.GetInt(JSAM.PauseMenu.FireInputKey);
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //}

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
        if (Input.GetKeyDown(fireKey))
        {
            ShootBehaviour();
        }
#endif
    }

    void ShootBehaviour()
    {
        Ray ray = new Ray();

        Vector2 screenPoint = Vector2.zero;
#if UNITY_ANDROID && !UNITY_EDITOR
        screenPoint = Input.touches[Input.touchCount - 1].position;
#else
        screenPoint = Input.mousePosition;
#endif
        ray = Cam.ScreenPointToRay(screenPoint);

        OnShoot?.Invoke(ray, screenPoint);
    }

    public void RebindFireKey(KeyCode newKey)
    {
        fireKey = newKey;
    }
}
