using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerNameInputField : MonoBehaviour
{
    [SerializeField]
    public const string playerNamePrefKey = "PlayerName";

    [SerializeField]
    TMPro.TMP_InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        string playerName = "Player";
        if (PlayerPrefs.HasKey(playerNamePrefKey))
        {
            playerName = PlayerPrefs.GetString(playerNamePrefKey);
        }
        inputField.text = playerName;

        PhotonNetwork.NickName = playerName;
    }

    public void SetPlayerName(string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            PhotonNetwork.NickName = newName;

            PlayerPrefs.SetString(playerNamePrefKey, newName);
        }
    }
}
