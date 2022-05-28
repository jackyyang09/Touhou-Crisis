using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordWrapper : MonoBehaviour
{
    public long APP_ID = 969259375556452405;

#if UNITY_STANDALONE
    static Discord.Discord discordInstance;
#endif

    public static DiscordWrapper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DiscordWrapper>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(DiscordWrapper).Name;
                    instance = obj.AddComponent<DiscordWrapper>();
                }
            }
            return instance;
        }
    }
    static DiscordWrapper instance;

    /// <summary>
    /// Use this for initialization.
    /// </summary>
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        transform.SetParent(null);

        DontDestroyOnLoad(transform.root.gameObject);
#if UNITY_STANDALONE
        if (discordInstance == null) discordInstance = new Discord.Discord(APP_ID, (ulong)Discord.CreateFlags.NoRequireDiscord);
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_STANDALONE
        discordInstance.RunCallbacks();
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state">The user's current party status</param>
    /// <param name="details">What the player is currently doing</param>
    /// <param name="startTime">Epoch seconds for game start - including will show time as "elapsed"</param>
    /// <param name="endTime">Epoch seconds for game end - including will show time as "remaining"</param>
    /// <param name="largeImageKey"></param>
    /// <param name="largeImageText"></param>
    /// <param name="smallImageKey"></param>
    /// <param name="smallImageText"></param>
    /// <param name="partyID"></param>
    /// <param name="partySize"></param>
    /// <param name="partyMax"></param>
    /// <param name="joinSecret"></param>
    public void UpdateActivity(
        string state = "", string details = "", 
        long startTime = 0, long endTime = 0, 
        string largeImageKey = "", string largeImageText = "", 
        string smallImageKey = "", string smallImageText = "",
        string partyID = "", int partySize = 0, int partyMax = 0,
        string joinSecret = ""
        )
    {
#if UNITY_STANDALONE
        var a = new Discord.Activity
        {
            State = state, Details = details,
            Timestamps = new Discord.ActivityTimestamps { Start = startTime, End = endTime },
            Assets = new Discord.ActivityAssets { LargeImage = largeImageKey, LargeText = largeImageText, SmallImage = smallImageKey, SmallText = smallImageText },
            Party = new Discord.ActivityParty { Id = partyID, Size = new Discord.PartySize { CurrentSize = partySize, MaxSize = partyMax } },
            Secrets = new Discord.ActivitySecrets { Join = joinSecret }
        };

        if (discordInstance == null)
        {
            Debug.LogWarning("Discord is not running!");
            return;
        }

        discordInstance.GetActivityManager().UpdateActivity(a, (result) =>
        {
            if (result == Discord.Result.Ok)
            {
                Debug.Log("Successfully updated Discord Activity!");
            }
            else
            {
                Debug.Log("Failed to update Discord Activity");
            }
        }
        );
#endif
    }

#if UNITY_STANDALONE
    private void OnApplicationQuit()
    {
        Debug.Log("Disposing of Discord instance...");
        if (discordInstance != null) discordInstance.Dispose();
    }
#endif
}
