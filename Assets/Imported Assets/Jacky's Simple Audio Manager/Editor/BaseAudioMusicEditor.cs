﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JSAM
{
    public abstract class BaseAudioMusicEditor : BaseAudioEditor
    {
        protected new void OnEnable()
        {
            if (target == null)
            {
                OnEnable();
                return;
            }

            PopulateMusicList();

            Setup();
        }

        protected void PopulateMusicList()
        {
            if (AudioManager.Instance)
            {
                var music = AudioManager.Instance.Library.music;
                for (int i = 0; i < music.Count; i++)
                {
                    options.Add(music[i].SafeName);
                }
            }
        }

        /// <summary>
        /// Is here to prevent the strange errors the moment you finish compiling
        /// </summary>
        protected void TryPopulateMusicList()
        {
            if (attempts < MAX_ATTEMPTS)
            {
                attempts++;
            }
            PopulateMusicList();
        }

        /// <summary>
        /// </summary>
        /// <param name="musicProperty">This is passed by reference, thanks Unity!</param>
        protected void DrawMusicDropdown(SerializedProperty musicProperty)
        {
            if (!AudioManager.Instance)
            {
                EditorGUILayout.HelpBox("Could not find Audio Manager in the scene! This component needs AudioManager " +
                    "in order to function!", MessageType.Error);
            }

            EditorGUILayout.LabelField("Specify Music to Play", EditorStyles.boldLabel);

            GUIContent musicDesc = new GUIContent("Music", "Music that will be played");

            AudioFileMusicObject audioObject = (AudioFileMusicObject)musicProperty.objectReferenceValue;
            int selected = 0;
            if (audioObject != null) selected = options.IndexOf(audioObject.SafeName);
            if (selected == -1) selected = 0;
            if (options.Count > 0)
            {
                selected = EditorGUILayout.Popup(musicDesc, selected, options.ToArray());
                musicProperty.objectReferenceValue = AudioManager.Instance.Library.music[selected];
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.Popup(musicDesc, selected, new string[] { "<None>" });
                }
            }
        }
    }
}