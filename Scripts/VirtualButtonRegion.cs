/// <summary>
/// Modified by Reid Perkins-Buzo from a script by Bruno Xavier L'.
/// http://forum.unity3d.com/threads/116076-VJR-(Virtual-Joystick-Region)-Sample?p=773620#post773620
/// </summary>
using UnityEngine;

public class VirtualButtonRegion : MonoBehaviour
{
	[System.NonSerialized]
	public bool VBRsingleTap;           // Player single tapped this button.

	[System.NonSerialized]
	public bool VBRdoubleTap;           // Player double tapped this button.
	public Color activeColor;           // Button's color when active.
	public Color inactiveColor;         // Button's color when inactive.
	public Texture2D button2D;          // Button's Image.
	public float divvyFactor;             // Button's size related to Screen's scale.
	public bool useDoubleTap;           // Activates Double-Tap functionality.
	private GUITexture button;          // Button GUI.
	private int fingerID;               // ID of finger touching this Joystick.
	private int lastID;                 // ID of last finger touching this Joystick.
	private float tapTimer;             // Double-tap's timer.
	private bool enable;                // VBR external control.
	
	private GameObject objPlayer;
    
	//
    
	public void DisableButton ()
	{
		enable = false;
	}
	
	public void EnableButton ()
	{
		enable = true;
	}
    
	//
    
	private int GetPixelScale (float fraction, int source)
	{
		int percent = source / 100;
		int result = percent * ((int)(fraction * 100));
		return result;
	}
	
	private void SingleTap ()
	{
		VBRsingleTap = true;
		button.color = activeColor;
		// if button has a finger on it, then put the player into fighting mode
		if (objPlayer != null) {
			playerScript ptrPlayerScript = (playerScript)objPlayer.GetComponent (typeof(playerScript));
			ptrPlayerScript.state = playerScript.ControlState.FightingPlayer;
		}
		//Debug.Log ("Tap!");
	}
	
	private void DoubleTap ()
	{
		VBRdoubleTap = true;
		button.color = activeColor;
		//Debug.Log ("Double Tap!");
	}
    
	//
    
	private void Awake ()
	{
		if (divvyFactor == 0) {
			divvyFactor = 1;
		}
		button = gameObject.AddComponent ("GUITexture") as GUITexture;
		button.texture = button2D;
		button.color = inactiveColor;
		transform.position = new Vector3 (0f, 0f, transform.position.z);
		//Debug.Log ("sw = " + Screen.width + "  sh = " + Screen.height);
		button.pixelInset = new Rect (GetPixelScale (transform.position.x, Screen.width), GetPixelScale (transform.position.y, Screen.height), (Screen.width / divvyFactor), (Screen.width / divvyFactor));
		//Debug.Log ("1) " + button.pixelInset);
		if (button.pixelInset.x < (Screen.width / 3) + 25 && button.pixelInset.y < (Screen.height / 3) + 25) {
			button.pixelInset = new Rect ((Screen.width / 3) + 410, -117, (Screen.width / divvyFactor), (Screen.width / divvyFactor));
			//Debug.Log ("2) " + button.pixelInset);
		}
		if (button.pixelInset.x > Screen.width - (button.pixelInset.width + 25) && button.pixelInset.y > Screen.height - (button.pixelInset.height + 25)) {
			button.pixelInset = new Rect (Screen.width - (button.pixelInset.width + 25), Screen.height - (button.pixelInset.height + 25), (Screen.width / divvyFactor), (Screen.width / divvyFactor));
			//Debug.Log ("3) " + button.pixelInset);
		}
		fingerID = -1;
		lastID = -1;
		tapTimer = 0;
		enable = true;
		VBRsingleTap = false;
		VBRdoubleTap = false;
		objPlayer = (GameObject)GameObject.FindWithTag ("Player");
	}
	
	private void Update ()
	{
		if (enable == true) {
			if (fingerID > -1 && fingerID < Input.touchCount) {
				lastID = fingerID;
				fingerID = -1;
				tapTimer = 0.150f;
				button.color = inactiveColor;
			}
			foreach (Touch touch in Input.touches) {
				if (button.pixelInset.Contains (touch.position) && touch.fingerId < 5) {
					fingerID = touch.fingerId;
				}
				if (Input.touchCount > 0 && fingerID > -1 && fingerID < Input.touchCount && button.pixelInset.Contains (touch.position)) {
					if (useDoubleTap) {
						if (tapTimer > 0 && fingerID == lastID && Input.GetTouch (fingerID).phase == TouchPhase.Began) {
							DoubleTap ();
						}
					} else {
						if (Input.GetTouch (fingerID).phase == TouchPhase.Began) {
							SingleTap ();
							//Debug.Log ("touch.position= " + touch.position);
						}
					}
					if (Input.GetTouch (fingerID).phase != TouchPhase.Began) {
						fingerID = -1;
						break;
					}
				}
			}
		}
		if (tapTimer > 0) {
			tapTimer -= Time.deltaTime;
		} else {
			if (VBRdoubleTap) {
				VBRdoubleTap = false;
			}
		}
	}
}