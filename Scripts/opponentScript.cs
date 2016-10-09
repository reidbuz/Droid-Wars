using UnityEngine;
using System.Collections;

public class opponentScript : MonoBehaviour
{

// **** start of variable declarations

// variables which point to game objects
	private GameObject objPlayer;
	private GameObject objHUDGUI;
// variables used to process and handle input & events
	private Vector3 targetLocation;
	private Vector3 inputMovement;
	private float setAngle = 0;
	private bool doFightDamage = false;
	public bool inSwarmMode = false;
// variables specific to the game object
	public float moveSpeed = 20000f;
	public int maintenancePoints = 20;
	private Vector3 savedTransform;
	private int fightDamage = 2;
	private float fledAtTime = 0f;
// variables used for processing animation
	public enum AnimationLoops
	{
		animationWave, // the ID of the wave animation
		animationWalk, // the ID of the walk animation
		animationFight // the ID of the fight animation
	}
	private AnimationLoops currentAnimation = AnimationLoops.animationWave; // the ID of the initial animation being played
	public float animationFrameRate = 11f; // how many frames to play per second
	private float walkAnimationMin = 39; // the first frame of the walk animation
	private float walkAnimationMax = 64; // the last frame of the walk animation
	private float waveAnimationMin = 1; // the first frame of the wave animation
	private float waveAnimationMax = 25; // the last frame of the wave animation
	private float fightAnimationMin = 27; // the first frame of the turn animation
	private float fightAnimationMax = 38; // the last frame of the turn animation
	private float spriteSheetTotalCols = 8; // the total number of columns of the sprite sheet
	private float spriteSheetTotalRows = 8; // the total number of rows of the sprite sheet
	private float frameNumber = 1; // the current frame being played,
	private float animationTime = 0f; // time to pass before playing next animation
	private Vector2 spriteSheetCount; // the X, Y position of the frame
	private Vector2 spriteSheetOffset; // the offset value of the X, Y coordinate for the texture
	private float spriteScalerY = 0.885f; // due to small variation between the wave and walk/fight animations we need this scaler
	private float spriteFightY = 0f; // due to small variation between the wave and walk/fight animations we need this offset
// variables for tracking droid states
	public enum ControlState
	{
		WavingDroid,
		MovingDroid,
		FleeingDroid,
		FightingDroid
	}
	private ControlState state = ControlState.WavingDroid;

// **** end of variable declarations
	
// Use this for initialization
	void Start ()
	{
		objPlayer = (GameObject)GameObject.FindWithTag ("Player");
		objHUDGUI = (GameObject)GameObject.FindWithTag ("HUD_GUI");
		
		// Initialize control state
		ResetControlState ();
		savedTransform = transform.position;
	}
	
// Update is called once per frame
	void Update ()
	{
		// check to see if this droid should be removed from the stage for maintenance
		if (maintenancePoints <= 0) {
			removeDroid ();
		} else {
			// if the droid has maintenance points, we play with it
			FindAndyinput ();
			HandleAnimation ();
		}
	}
	
	void ResetControlState ()
	{
		// Return to origin state and reset fingers that we are watching
		state = ControlState.WavingDroid;

	}
	
	void FindAndyinput ()
	{
		// FOR THE NON-PLAYER: Determine the following critical facts
		//		1) Is there still a player or has he been elimated by Rhubarb! 
		//		2) inputMovement vector for the non-player sprite 
		//		3) If the non-player is close enough to the player, smack with the Rhubarb!
		
		// check to see if the Player has been Rhubarbed off the stage.  
		// If so set the input variables to stop non-player movement and return
		if (objPlayer == null) {
			inputMovement = Vector3.zero;
			transform.eulerAngles = new Vector3 (0, 0, 0);
			transform.position = new Vector3 (transform.position.x, 0, transform.position.z);
			ResetControlState ();
			return;
		}
		
		float dist = Vector3.Distance (objPlayer.transform.position, transform.position);
		
		// the non-player inputMovement vector is toward the player unless it's low on maintenancePoints
		inputMovement = objPlayer.transform.position - transform.position;
		if (maintenancePoints < 10 && dist < (float)Random.Range (20, 50) && !inSwarmMode) {
			inputMovement = -inputMovement;
			if (state != ControlState.FleeingDroid)
				fledAtTime = Time.time;
			state = ControlState.FleeingDroid;
		} else {
			state = ControlState.MovingDroid;
		}
		
		// Is the non-player close enough to fight?
		if (dist < 2.0f) {
			if (state == ControlState.FleeingDroid) {
				// if more than one animation frame has gone by,
				// and this opponent droid hasn't been able to move away from the player
				// then some "if cornered, fight back" behavior is called for!
				if ((Time.time - fledAtTime) > (1 / animationFrameRate) && Vector3.Distance (savedTransform, transform.position) <= 1f) {
					state = ControlState.FightingDroid;
					inputMovement = Vector3.zero;
					if (currentAnimation != AnimationLoops.animationFight) {
						frameNumber = fightAnimationMin + 1;
					}
				}
			} else {
				state = ControlState.FightingDroid;
				inputMovement = Vector3.zero;
				if (currentAnimation != AnimationLoops.animationFight) {
					frameNumber = fightAnimationMin + 1;
				} 
			}
		}
		
		ProcessMovement ();
	}

	void ProcessMovement ()
	{
		// move the sprite by moveSpeed * Time.deltaTime
		rigidbody.AddForce (inputMovement.normalized * moveSpeed * Time.deltaTime);
		
		// turn the sprite to face the direction it is moving on the horizontal x-axis
		// small variations in the x-direction should be ignored in setting the sprites' orientation
		if (inputMovement.x > 0.02f) {
			setAngle = 180;
			if (currentAnimation == AnimationLoops.animationFight) {
				if (objPlayer.transform.position.x < transform.position.x) {
					setAngle = 0;
				}
			}
		} 
		if (inputMovement.x < -0.02f) {
			setAngle = 0;
			if (currentAnimation == AnimationLoops.animationFight) {
				if (objPlayer.transform.position.x > transform.position.x) {
					setAngle = 180;
				}
			}
		}
		transform.eulerAngles = new Vector3 (0, 0, setAngle);
		
		// update the sprite position
		float newX = transform.position.x;
		float newZ = transform.position.z;
		if (newX > 58f)
			newX = 58f;
		if (newX < -58f)
			newX = -58f;
		if (newZ > 11.2f)
			newZ = 11.2f;
		if (newZ < -6.3f)
			newZ = -6.3f;
		transform.position = new Vector3 (newX, 0, newZ);
		savedTransform = transform.position;
	}
	
	void HandleAnimation () // handles all animation
	{
		FindAnimation ();
		ProcessAnimation ();
	}

	void FindAnimation ()
	{
		// Find which animation is currently playing
		
		// one clue is if state is FightingDroid. if it is set the current animation to animationFight
		if (state == ControlState.FightingDroid || currentAnimation == AnimationLoops.animationFight) {
			currentAnimation = AnimationLoops.animationFight;
			return;
		}
		
		// if the inputMovement vector has a positive magnitude, then the sprite is walking
		if (state == ControlState.MovingDroid || inputMovement.magnitude > 0) {
			currentAnimation = AnimationLoops.animationWalk;
		} else {
			// if both these conditions fail, the sprite is standing still waving!
			currentAnimation = AnimationLoops.animationWave;
		}
	}

	void ProcessAnimation ()
	{
		// iteration int i
		int i;
		
		// the animation framerate is managed by setting animationTime initially to 1/framerate
		// then subtracting the gameplay framerate passed since the last call to ProcessAnimation
		// When animationTime eventually becomes negative after enough subtractions, it is time
		// to display a new frame of animation. Then, after the new animation frame is displayed, we 
		// reset animationTime to 1/framerate, so we can begin the process again
		// This essentially creates a Flash-like local timeline for this sprite using the gameplay framerate
		animationTime -= Time.deltaTime;
		// if the game is running at 30 frames per second 
		// animationTime will subtract 0.033 of a second (1/30)
		if (animationTime <= 0) {
			frameNumber += 1;
			// manage the animationFight animation
			if (currentAnimation == AnimationLoops.animationFight) {
				frameNumber = Mathf.Clamp (frameNumber, fightAnimationMin, fightAnimationMax + 1);
				spriteScalerY = 0.9150f;
				spriteFightY = -0.009f;
				if (frameNumber > fightAnimationMax) {
					// once we have finished the fight animation, detect if we have hit the player again
					doFightDamage = false;
					if (state == ControlState.FightingDroid) {
						// when looping the fight animation it looks better to start at fightAnimationMin + 4
						frameNumber = fightAnimationMin + 4;
					} else {
						currentAnimation = AnimationLoops.animationWalk;
						frameNumber = walkAnimationMin;
					}
				}
				// if this droid is within 2 units of the player and if we are at least 4 frames into the animation, and we haven't attacked yet
				if (state == ControlState.FightingDroid && frameNumber > fightAnimationMin + 4 && doFightDamage == false) {
					// damage the player
					doFightDamage = true;
					if (gameObject != null && objPlayer != null) {
						playerScript ptrPlayerScript = (playerScript)objPlayer.GetComponent (typeof(playerScript));
						ptrPlayerScript.maintenancePoints -= fightDamage;
						//print ("Attacked by " + gameObject.tag);
					}
					
				}
			}
			// manage the animationWave animation
			if (currentAnimation == AnimationLoops.animationWave) {
				frameNumber = Mathf.Clamp (frameNumber, waveAnimationMin, waveAnimationMax + 1);
				spriteScalerY = 0.8918f;
				spriteFightY = 0f;
				if (frameNumber > waveAnimationMax) {
					frameNumber = waveAnimationMin;
				}
			}
			// manage the animationWalk animation
			if (currentAnimation == AnimationLoops.animationWalk) {
				frameNumber = Mathf.Clamp (frameNumber, walkAnimationMin, walkAnimationMax + 1);
				spriteScalerY = 0.9150f;
				spriteFightY = 0f;
				if (frameNumber > walkAnimationMax) {
					frameNumber = walkAnimationMin;
				}
			}
			// reset the animationTime to 1/framerate, so we can begin the local timeline process again
			animationTime += (1 / animationFrameRate); 
		}
		
		// The remaining code finds the image for the animation frame on the sprite sheet, 
		// then displays it on the surface of the droid's plane model as a texture		
		// First, find the number of frames down the animation is on the sprite sheet and set the y coordinate accordingly
		spriteSheetCount.y = 0;
		for (i=(int)frameNumber; i > (int)spriteSheetTotalRows; i-=(int)spriteSheetTotalRows) {
			spriteSheetCount.y += 1;
		}
		
		// Second, find the number of frames down the animation is on the sprite sheet and set the x coordinate accordingly
		spriteSheetCount.x = i - 1;
		
		// Third, calculate the exact X and Y pixel coordinate on the sprite sheet of the frame to display
		double calcY = spriteScalerY * (spriteSheetCount.y / spriteSheetTotalRows) + spriteFightY;
		double calcX = 0.9150 * (spriteSheetCount.x / spriteSheetTotalCols) + 0.033;
		spriteSheetOffset = new Vector2 ((float)(1 - calcX), (float)(1 - calcY));
		
		// Last, offset the texture to display the correct frame
		renderer.material.SetTextureOffset ("_MainTex", spriteSheetOffset); 
	}
		
	// removes the droid
	void removeDroid ()
	{
		// As we remove this opponent droid, we update the player opponentCount variable to be one less
		// and we add one to the score variable which is kept in script for the HUD 
		if (objPlayer != null) {
			playerScript ptrPlayerScript = (playerScript)objPlayer.GetComponent (typeof(playerScript));
			ptrPlayerScript.opponentCount--;
			HUD_GUI_Script ptrHUDGUIScript = (HUD_GUI_Script)objHUDGUI.GetComponent (typeof(HUD_GUI_Script));
			ptrHUDGUIScript.score++;
		}
		
		Destroy (gameObject);
	}
}
