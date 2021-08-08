using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;

public class RematchNotification : MonoBehaviour, IReloadable
{
    [SerializeField] RawImage reimuScroll;
    [SerializeField] RawImage marisaScroll;

    [SerializeField] Animator anim;
    [SerializeField] new Animation animation;

    [SerializeField] OptimizedCanvas canvas;
    [SerializeField] LeanLocalizedTextMeshProUGUI text;

    public void Reinitialize()
    {
        anim.enabled = false;
        canvas.Hide();
    }

    private void Start()
    {
        if (Photon.Pun.PhotonNetwork.OfflineMode) return;
        SoftSceneReloader.Instance.AddNewReloadable(this);

        Reinitialize();
    }

    private void OnDisable()
    {
        if (SoftSceneReloader.Instance != null)
        {
            SoftSceneReloader.Instance.RemoveReloadable(this);
        }
    }

    public void OnReceiveRematchRequest()
    {
        LeanLocalization.SetToken("Tokens/OtherPlayer", PlayerManager.Instance.OtherPlayer.Owner.NickName);
        text.UpdateLocalization();
        canvas.Show();
        anim.enabled = true;
        reimuScroll.enabled = Photon.Pun.PhotonNetwork.IsMasterClient;
        marisaScroll.enabled = !reimuScroll.enabled;

        animation.Play("Rematch Notification");
    }
}
