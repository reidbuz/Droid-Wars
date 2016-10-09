using UnityEngine;
using System.Collections;

public class HUD_GUI_Script : MonoBehaviour
{
	
	private GameObject objPlayer;
	public int score = 0;
	private	string maintenanceString = "";

	// Use this for initialization
	void Start ()
	{
		objPlayer = (GameObject)GameObject.FindWithTag ("Player");
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		// set HUD GUI info
		if (objPlayer != null) {
			playerScript ptrPlayerScript = (playerScript)objPlayer.GetComponent (typeof(playerScript));
			maintenanceString = ptrPlayerScript.maintenancePoints.ToString ();
		} else {
			maintenanceString = "--";
		}
		GUIText guiText = (GUIText)(GameObject.Find ("title00").GetComponent ("GUIText"));
		guiText.text = "Score: " + score.ToString ();
		guiText = (GUIText)(GameObject.Find ("title01").GetComponent ("GUIText"));
		guiText.text = "Maintenance\nLevel: " + maintenanceString;
	}
}
