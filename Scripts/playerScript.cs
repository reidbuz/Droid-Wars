using UnityEngine;
using System.Collections;

public class playerScript : MonoBehaviour
{

// **** start of variable declarations

// variables which point to game objects
	public GameObject preFabOpponent;
	public GameObject objOpponentOnStage;
	private GameObject objPlayer;
	private GameObject objCube;
	private GameObject objCamera;
// variables used to process and handle input & events
	private Vector3 targetLocation;
	private Vector3 inputMovement;
	private float setAngle = 0f;
	private bool doFightDamage = false;
	private bool keyboardInUse = false;
// variables specific to the game object
	public float moveSpeed = 20000f;
	public int maintenancePoints = 40;
	public int opponentCount = 1;
	private int maxOpponents;
	private float opponentSpawnedAtTime = 0f;
	private float minSecondsTweenOpponents = 10f;
	private int fightDamage = 4;
// variables used for calculation
	private Vector3 tempVector;
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
// variables for droid states
	public enum ControlState
	{
		WaitingForATouch,
		MovingPlayer,
		FightingPlayer
	}
	public ControlState state = ControlState.WaitingForATouch;

// **** end of variable declarations
	
// Use this for initialization
	void Start ()
	{
		// initialize the objPlayer variable
		objPlayer = gameObject;
		
		// create the initial opponent
		objOpponentOnStage = (GameObject)Instantiate (preFabOpponent, new Vector3 (0, 0, 0), Quaternion.LookRotation (new Vector3 (((float)Random.Range (1, 3)), 0, 0)));
		objOpponentOnStage.tag = "Opponent01";
		
		// initialize cube variable & get the maxOpponents from it
		objCube = (GameObject)GameObject.FindWithTag ("cube");
		cubeScript ptrcubeScript = (cubeScript)objCube.GetComponent (typeof(cubeScript));
		maxOpponents = ptrcubeScript.maxOpponents;
		
		// initialize camera variables
		objCamera = (GameObject)GameObject.FindWithTag ("MainCamera");
			
		// Initialize control state
		ResetControlState ();
	}
	
// Update is called once per frame
	void Update ()
	{
		// check to see if the player droid should be removed from the stage for maintenance
		if (maintenancePoints <= 0) {
			removeDroid ();
			
		} else {
			// if the droid has maintenance points, we play with it
			FindPlayerInput ();
			HandleAnimation ();
			HandleCamera ();
			moveSpeed = Random.Range (20000f, 21000f);
			
			// Create a new opponent on stage if  
			//    1) we can find an opponent on the stage with less than 12 maintenance points,
			//    2) the total number of opponents on stage is less than maxOpponents, and
			//    3) there has not been an opponent created for the minSecondsTweenOpponents,
			// and set it in place on the stage
			int i;
			opponentScript ptrOppoScript = null;
			for (i = 1; i < maxOpponents; i++) {
				objOpponentOnStage = (GameObject)GameObject.FindWithTag ("Opponent0" + i.ToString ());
				if (objOpponentOnStage != null) {
					ptrOppoScript = (opponentScript)objOpponentOnStage.GetComponent (typeof(opponentScript));
					if (ptrOppoScript.maintenancePoints <= 12)
						break;
				}
			}
			if (objOpponentOnStage != null && opponentCount < maxOpponents && (Time.time - opponentSpawnedAtTime) > minSecondsTweenOpponents) {
				if (ptrOppoScript.maintenancePoints <= 12) {
					if (preFabOpponent != null) {
						GameObject objCreatedOpponent = (GameObject)Instantiate (preFabOpponent, new Vector3 (0, 0, 0), Quaternion.LookRotation (new Vector3 (((float)Random.Range (1, 3)), 0, ((float)Random.Range (0, 3)))));
						for (i = 1; i < maxOpponents; i++) {
							objOpponentOnStage = (GameObject)GameObject.FindWithTag ("Opponent0" + i.ToString ());
							if (objOpponentOnStage == null)
								break;
						}
						objCreatedOpponent.tag = "Opponent0" + i.ToString ();
						ptrOppoScript = (opponentScript)objCreatedOpponent.GetComponent (typeof(opponentScript));
						ptrOppoScript.maintenancePoints = 20;
						opponentCount++;
						// set the time when this opponent was spawned 
						opponentSpawnedAtTime = Time.time;
					}
				}
			}
			
		}
	}
	
	void ResetControlState ()
	{
		// Return to origin state and reset fingers that we are watching
		state = ControlState.WaitingForATouch;

	}

	void FindPlayerInput ()
	{
		// FOR THE PLAYER: Find the input device and determine the inputMovement vector
		int touchCnt = Input.touchCount;
		if (touchCnt == 0) {
			// test to see if the user is providing input via the keyboard
			Event evnt = Event.current;
			if (evnt.isKey != null) {
				// this code is used for keyboard & mouse interaction. We don't use it for mobile devices
				keyboardInUse = true;
				inputMovement = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
				// input mouse position gives us 2D coordinates, so we set the Y coordinate to the Z coordinate 
				// in tempVector and the Y coordinate to 0, so that tempVector will read the input along 
				// the X (left and right of screen) and Z (up and down screen) axes, and not the X and Y (in and out of screen) axes
				tempVector = Input.mousePosition;
				tempVector.z = tempVector.y; 
				tempVector.y = 0;
				// if the mouse is down switch player to fighting state!
				if (Input.GetMouseButtonDown (0)) {
					state = ControlState.FightingPlayer;
					
				} else if (Input.GetMouseButtonUp (0)) {
					// if the mouse button is released, exit fighting state
					ResetControlState ();
				}
				ProcessMovement ();
			}
		} else {
			// this next code is used for touchscreen devices
			keyboardInUse = false;
			Touch touch;
			Touch[] touches = Input.touches;	
		
			// Check if we got a finger down
			if (state == ControlState.WaitingForATouch) {				
				// UnityRemote inherently introduces latency into the touch input received
				// because the data is being passed back over WiFi. Sometimes you will get 
				// a TouchPhase.Moved event before you have even seen a TouchPhase.Began. 
				// The following takes this into account to improve the feedback loop when using UnityRemote.
				int i;
				for (i = 0; i < touchCnt; i++) {
					touch = touches [i];
					if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled) {
						state = ControlState.MovingPlayer;
						break;
					}
				}
			}
			// if the user uses multi-touch (2 or more touches simultaneously), put the player into fight mood
			// this allows the player to be controlled with only one hand
			if (state == ControlState.MovingPlayer && touchCnt > 0) {
				// if there's only one touch on the Joystick, then use the VJRvector to set the inputMovement
				inputMovement = VirtualJoystickRegion.VJRvector;
				inputMovement.z = inputMovement.y;
				inputMovement.y = 0f;
				ProcessMovement ();
				// after the player has moved, reset the state to WaitingForATouch
				ResetControlState ();
			} 
			
			if (state == ControlState.FightingPlayer && touchCnt > 1) {
				inputMovement = Vector3.zero;
				transform.eulerAngles = new Vector3 (0, 0, setAngle);
			} else {
				ResetControlState ();
			}
		}
			
	}

	void ProcessMovement ()
	{
		// move the sprite by moveSpeed * Time.deltaTime
		rigidbody.AddForce (inputMovement.normalized * moveSpeed * Time.deltaTime);
		
		// turn the sprite to face the direction it is moving on the horizontal x-axis
		// small variations in the x-direction should be ignored in setting the sprites' orientation
		// using a variable setAngle to store the z-axis rotation keeps the sprite always oriented correctly
		if (inputMovement.x > 0.02f) {
			setAngle = 180;

		} 
		if (inputMovement.x < -0.02f) {
			setAngle = 0;
		}
		transform.eulerAngles = new Vector3 (0, 0, setAngle);
		
		// if the keyboard is in use turn the Player sprite to face the direction it is moving on the vertical z-axis
		// we don't use this for touch screen input since it looks funny on a touch screen
		// small variations in the z-direction should be ignored in setting the sprites' orientation
		if (keyboardInUse && inputMovement.z > 0.02f) {
			transform.eulerAngles = new Vector3 (0, 90, 0);
		}
		if (keyboardInUse && inputMovement.z < -0.02f) {
			transform.eulerAngles = new Vector3 (0, -90, 0);
		}
		
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
	}
	
	void HandleCamera ()
	{
		// keep the camera interest on the player
		objCamera.transform.position = new Vector3 (transform.position.x, 15, transform.position.z);
		objCamera.transform.eulerAngles = new Vector3 (90, 0, 0);
	}
	
	void HandleAnimation () // handles all animation
	{
		FindAnimation ();
		ProcessAnimation ();
	}

	void FindAnimation ()
	{
		// Find which animation is currently playing
		
		// one clue is if state is FightingPlayer. if it is set the current animation to animationFight
		if (state == ControlState.FightingPlayer || currentAnimation == AnimationLoops.animationFight) {
			currentAnimation = AnimationLoops.animationFight;
			return;
		}
		
		// if the inputMovement vector has a positive magnitude, then the sprite is walking
		if (inputMovement.magnitude > 0) {
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
					// once we have finished the fight animation, detect if we are still in FightingPlayer state
					doFightDamage = false;
					if (state == ControlState.FightingPlayer) {
						// when looping the fight animation it looks better to start at fightAnimationMin + 4
						frameNumber = fightAnimationMin + 4;
					} else {
						currentAnimation = AnimationLoops.animationWalk;
						frameNumber = walkAnimationMin;
					}
				}
				// if the player is within 2 units of the opponent and 
				// we are at least 4 frames into the animation, and we haven't attacked yet
				if (state == ControlState.FightingPlayer && frameNumber > fightAnimationMin + 4 && doFightDamage == false) {
					// damage the droid
					doFightDamage = true;
					for (i = 1; i <= maxOpponents; i++) {
						objOpponentOnStage = (GameObject)GameObject.FindWithTag ("Opponent0" + i.ToString ());
						if (objOpponentOnStage != null) {
							if (Vector3.Distance (objPlayer.transform.position, objOpponentOnStage.transform.position) < 2.0f) {
								opponentScript ptrOppoScript = (opponentScript)objOpponentOnStage.GetComponent (typeof(opponentScript));
								ptrOppoScript.maintenancePoints -= fightDamage;
								//print ("Opponent0" + i.ToString () + " damaged");
								break;
							}
						}
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
		// Second, find the number of frames across the animation is on the sprite sheet and set the x coordinate accordingly
		spriteSheetCount.x = i - 1;
		
		// Third, calculate the exact X and Y pixel coordinate on the sprite sheet of the frame to display
		double calcY = spriteScalerY * (spriteSheetCount.y / spriteSheetTotalRows) + spriteFightY;
		double calcX = 0.9150 * (spriteSheetCount.x / spriteSheetTotalCols) + 0.033;
		spriteSheetOffset = new Vector2 ((float)(1 - calcX), (float)(1 - calcY));
		
		// Last, offset the texture to display the correct frame
		renderer.material.SetTextureOffset ("_MainTex", spriteSheetOffset); 
	}
		
	// removes the player droid
	void removeDroid ()
	{
		// update the objCube so it will know what time the player is being removed at
		cubeScript ptrcubeScript = (cubeScript)objCube.GetComponent (typeof(cubeScript));
		ptrcubeScript.timePlayerRemovedAt = Time.time;
		
		Destroy (gameObject);
	}
}
