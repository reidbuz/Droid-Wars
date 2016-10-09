using UnityEngine;
using System.Collections;

public class cubeScript : MonoBehaviour
{
	public float timePlayerRemovedAt = 0f;
	private GameObject objPlayer;
	public GameObject objOpponentOnStage;
	public int maxOpponents = 5;
	private float poweredAtTime = 0f;
	private float swarmedAtTime = 0f;
	private float stopSwarmTime = 0f;
	private float minSecondsTweenPowerUps = 5f;
	private bool inSwarmMode = false;
	
	// Use this for initialization
	void Start ()
	{
		objPlayer = (GameObject)GameObject.FindWithTag ("Player");
		transform.eulerAngles = new Vector3 (0, 0, 0);
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		// rotate the cube
		transform.Rotate (5 * Time.deltaTime, 5 * Time.deltaTime, 5 * Time.deltaTime);
		
		// power-up the player if he is with 2 units of the cube (max is around 40 points)
		if (objPlayer != null && (Time.time - poweredAtTime) > minSecondsTweenPowerUps) {
			if (Vector3.Distance (objPlayer.transform.position, transform.position) < 2.0f) {
				playerScript ptrPlayerScript = (playerScript)objPlayer.GetComponent (typeof(playerScript));
				ptrPlayerScript.maintenancePoints += ((ptrPlayerScript.maintenancePoints > 35) ? 0 : 10);
				poweredAtTime = Time.time;
			}
		}
		
		// Should the opponent droids go into swarm mode?
		checkForSwarmMode ();
		
		// if the player has been removed from the stage 
		// and the opponent droids have been given suffficient time to gloat,
		// then load the game relaunch scene
		if (objPlayer == null && (Time.time - timePlayerRemovedAt) > 5f) {
			Application.LoadLevel ("droidWarsLoader");
		}
	}
	
	void checkForSwarmMode ()
	{
		if (! inSwarmMode && (Time.time - stopSwarmTime) > 30f) {
			// if the opponent droids are not presently in swarm mode, and 
			// more than 30 seconds have passed since the last swarm,
			// check to see if there's a new swarm at hand
			// trigger swarm mode 30% of the time
			if (Random.Range (0, 99) > 65) {
				inSwarmMode = true;
				// need to communicate with all the opponent scripts and put them into swarm mode here
				int i;
				opponentScript ptrOppoScript = null;
				for (i = 1; i < maxOpponents; i++) {
					objOpponentOnStage = (GameObject)GameObject.FindWithTag ("Opponent0" + i.ToString ());
					if (objOpponentOnStage != null) {
						ptrOppoScript = (opponentScript)objOpponentOnStage.GetComponent (typeof(opponentScript));
						ptrOppoScript.inSwarmMode = true;
					}
				}
				swarmedAtTime = Time.time;
			}
		} else if (inSwarmMode && (Time.time - swarmedAtTime) > 10f) {
			// if the opponent droids are presently in swarm mode, and 
			// more than 10 seconds have passed since swarm mode began,
			// stop swarm mode
			inSwarmMode = false;
			// need to communicate with all the opponent scripts and get them out of swarm mode here
			int i;
			opponentScript ptrOppoScript = null;
			for (i = 1; i < maxOpponents; i++) {
				objOpponentOnStage = (GameObject)GameObject.FindWithTag ("Opponent0" + i.ToString ());
				if (objOpponentOnStage != null) {
					ptrOppoScript = (opponentScript)objOpponentOnStage.GetComponent (typeof(opponentScript));
					ptrOppoScript.inSwarmMode = false;
				}
			}
			stopSwarmTime = Time.time;
		}
	}
}
