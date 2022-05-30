using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class WebsiteOpenerWebGL : MonoBehaviour 
{
	public void OpenLink(string link)
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		openWindow(link);
#else
		Application.OpenURL(link);
#endif
		JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);
	}

	[DllImport("__Internal")]
	private static extern void openWindow(string url);

}