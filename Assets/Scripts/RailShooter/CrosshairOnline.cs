using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CrosshairOnline : MonoBehaviourPun
{
    [SerializeField] UnityEngine.UI.Image image = null;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine) image.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.OfflineMode && PhotonNetwork.IsConnected)
        {
            if (!photonView.IsMine) return;
        }

        // Why this works: 
        // https://answers.unity.com/questions/849117/46-ui-image-follow-mouse-position.html?_ga=2.45598500.148015968.1612849553-1895421686.1612849553
#if UNITY_ANDROID && !UNITY_EDITOR
        transform.position = Input.touches[Input.touchCount - 1].position;
#else
        transform.position = Input.mousePosition;
#endif
    }
}
