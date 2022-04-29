﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordWrapper : MonoBehaviour
{
    public long APP_ID = 969259375556452405;

    static Discord.Discord discordInstance;

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
        DontDestroyOnLoad(transform.root.gameObject);

        if (discordInstance == null) discordInstance = new Discord.Discord(APP_ID, (ulong)Discord.CreateFlags.Default);
    }

    // Update is called once per frame
    void Update()
    {
        discordInstance.RunCallbacks();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state">The user's current party status</param>
    /// <param name="details"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
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
        var a = new Discord.Activity
        {
            State = state, Details = details,
            Timestamps = new Discord.ActivityTimestamps { Start = startTime, End = endTime },
            Assets = new Discord.ActivityAssets { LargeImage = largeImageKey, LargeText = largeImageText, SmallImage = smallImageKey, SmallText = smallImageText },
            Party = new Discord.ActivityParty { Id = partyID, Size = new Discord.PartySize { CurrentSize = partySize, MaxSize = partyMax } },
            Secrets = new Discord.ActivitySecrets { Join = joinSecret }
        };

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
    }

    private void OnApplicationQuit()
    {
        discordInstance.Dispose();
    }
}