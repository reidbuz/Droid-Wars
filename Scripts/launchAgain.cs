using UnityEngine;
using System.Collections;

public class launchAgain : MonoBehaviour
{
	// droidSplash is assigned in the inspector panel
	public Texture2D droidSplash;
	public Font droidGUIFont;

	// Use this for initialization
	/*void Start ()
	{
	}*/
	
	// OnGUI is called once per frame
	void OnGUI ()
	{
		// set up GUI Styles for the UI
		GUIStyle droidGUIStyle = new GUIStyle (GUI.skin.button);
		droidGUIStyle.fontSize = 30;
		droidGUIStyle.font = droidGUIFont;
		droidGUIStyle.normal.textColor = Color.green;
		droidGUIStyle.hover.textColor = Color.green;
		droidGUIStyle.active.textColor = Color.green;
		
		GUIStyle droidGUIStyleBIG = new GUIStyle (GUI.skin.button);
		droidGUIStyleBIG.fontSize = 72;
		droidGUIStyleBIG.font = droidGUIFont;
		droidGUIStyleBIG.normal.textColor = Color.red;
		droidGUIStyleBIG.hover.textColor = Color.green;
		droidGUIStyleBIG.active.textColor = Color.yellow;
		
		// draw the background image
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), droidSplash);
		
		// draw the text and if-button
		GUI.Box (new Rect (Screen.width * 0.233f, Screen.height * 0.2f, 512, 100), "Press Home Button to Quit", droidGUIStyle);
		
		if (GUI.Button (new Rect (Screen.width * 0.262f, Screen.height * 0.5f, 450, 100), "Play Again?", droidGUIStyleBIG))
			Application.LoadLevel ("droidWarsMain");
		
	
	}
}
