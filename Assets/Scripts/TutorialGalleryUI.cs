using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

public class TutorialGalleryUI : MonoBehaviour
{
    [SerializeField] OptimizedCanvas canvas;
    [SerializeField] TutorialSlideshow slideShow;

#if UNITY_EDITOR
    [SerializeField] GameObject imagePrefab;

    [SerializeField] Transform galleryTransform;

    [ContextMenu(nameof(GenerateTutorialGallery))]
    void GenerateTutorialGallery()
    {
        for (int i = 0; i < slideShow.Graphics.Length; i++)
        {
            var image = Instantiate(imagePrefab, galleryTransform);

            var renderer = image.GetComponent<UnityEngine.UI.Image>();
            renderer.sprite = slideShow.Graphics[i];

            var eventTrigger = image.GetComponent<UnityEngine.EventSystems.EventTrigger>();

            var eventEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            eventEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;

            UnityEventTools.AddIntPersistentListener(eventEntry.callback, ShowTutorialAtIndex, i);

            eventTrigger.triggers.Add(eventEntry);
        }
    }

#endif
    public void ShowTutorialAtIndex(int id)
    {
        JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);
        canvas.Hide();
        slideShow.ShowWithSlide(id);
    }
}
