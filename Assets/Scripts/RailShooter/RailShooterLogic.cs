using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RailShooterLogic : MonoBehaviour
{
    [SerializeField] bool offlineMode = false;

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

    public System.Action<Ray> OnShoot;

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

        if (Input.GetMouseButtonDown(0))
        {
            ShootBehaviour();
        }
    }

    void ShootBehaviour()
    {
        Ray ray = new Ray();
        ray = Cam.ScreenPointToRay(Input.mousePosition);

        OnShoot?.Invoke(ray);
    }
}
