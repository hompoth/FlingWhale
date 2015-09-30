using UnityEngine;

public class InGame : Photon.MonoBehaviour
{
	public Transform playerPrefab;
	private GameObject playerObject;
	private ThirdPersonController playerController;
	private int seed;
	private PhotonView photonView;
	private MapGeneration mGen;
	private AStar pFind;
	private bool Loaded;
	private int lastX;
	private int lastY;
	private float time;
	
	public void Awake()
	{
		time = Time.time;
		Loaded = false;
		mGen = GetComponent<MapGeneration> (); //new MapGeneration();
		pFind = GetComponent<AStar> ();
		photonView = GetComponent<PhotonView> ();
		seed = 0;
		if (PhotonNetwork.isMasterClient) {
			seed = Random.seed;
			for (int i = -4; i <= 4; ++i) {
				for(int j = -4; j <= 4; ++j){
					mGen.GenerateBlock (i, j, seed, 0, 0);
				}
			}
			mGen.CreateGrid(0, 0);
			Loaded = true;	
			lastX = 0;
			lastY = 0;
		}
		
		// in case we started this demo with the wrong scene being active, simply load the menu scene
		if (!PhotonNetwork.connected)
		{
			Application.LoadLevel(Menu.SceneNameMenu);
			return;
		}
		// we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
		playerObject = PhotonNetwork.Instantiate(this.playerPrefab.name, transform.position, Quaternion.identity, 0);
		playerController = playerObject.GetComponent<ThirdPersonController> ();
	}
	public void Update(){
		float v = Input.GetAxisRaw ("Vertical");
		float h = Input.GetAxisRaw ("Horizontal");
		if (v != 0 || h != 0){
			mGen.setFollow (false);
			mGen.setWander (false);
			mGen.setPursuit (false);
			mGen.setEvade (false);
		}
		if (seed != 0 && Loaded == false) {
			Loaded = true;
			lastX = 0;
			lastY = 0;
			for (int i = -4; i <= 4; ++i) {
				for(int j = -4; j <= 4; ++j){
					mGen.GenerateBlock (i, j, seed, 0, 0);
				}
			}
			mGen.CreateGrid(0, 0);
		} else if(Loaded == true){
			int x = (int)(playerObject.transform.position.x/15);
			int y = (int)(playerObject.transform.position.z/15);
			//Debug.Log(x+" and "+y);
			if(lastX!=x||lastY!=y){
				mGen.shiftBlockArray((lastX-x), (lastY-y));
				//if(Mathf.Abs((lastX+lastY)-(x+y))>2){
				//	Debug.Log("Moving too fast");
				for (int i = -1; i < 2; ++i) {
					for(int j = -1; j < 2; ++j){
						mGen.GenerateBlock (i + x, j + y, seed, x, y);
					}
				}
				/*}
				else {
					if(x-lastX>0){ 
						if(y-lastY>0){ // Top right corner
							Debug.Log ("Corner-TR");
							for(int i = -1; i < 2; ++i){
								mGen.GenerateBlock (x + i, y + 1, seed, x, y);
							}
							for(int j = -1; j < 1; ++j){
								mGen.GenerateBlock (x + 1, y + j, seed, x, y);
							}
						} else if(y-lastY<0){ // Bottom right corner
							Debug.Log ("Corner-BR");
							for(int i = -1; i < 2; ++i){
								mGen.GenerateBlock (x + i, y - 1, seed, x, y);
							}
							for(int j = 0; j < 2; ++j){
								mGen.GenerateBlock (x + 1, y + j, seed, x, y);
							}
						}
						else { // To the right
							Debug.Log ("Right");
							for(int j = -1; j < 2; ++j){
								mGen.GenerateBlock (x + 1, y + j, seed, x, y);
							}
						}
					}else if(x-lastX<0){
						if(y-lastY>0){ // Top left corner
							Debug.Log ("Corner-TL");
							for(int i = -1; i < 2; ++i){
								mGen.GenerateBlock (x + i, y + 1, seed, x, y);
							}
							for(int j = -1; j < 1; ++j){
								mGen.GenerateBlock (x - 1, y + j, seed, x, y);
							}
						} else if(y-lastY<0){ // Bottom left corner
							Debug.Log ("Corner-BL");
							for(int i = -1; i < 2; ++i){
								mGen.GenerateBlock (x + i, y - 1, seed, x, y);
							}
							for(int j = 0; j < 2; ++j){
								mGen.GenerateBlock (x - 1, y + j, seed, x, y);
							}
						}
						else { // To the left
							Debug.Log ("Left");
							for(int j = -1; j < 2; ++j){
								mGen.GenerateBlock (x - 1, y + j, seed, x, y);
							}
						}
					}
					else {
						if(y-lastY>0){ // To the up
							Debug.Log ("Up");
							for(int i = -1; i < 2; ++i){
								mGen.GenerateBlock (x + i, y + 1, seed, x, y);
							}
						} else if(y-lastY<0){ // To the down
							Debug.Log ("Down");
							for(int i = -1; i < 2; ++i){
								mGen.GenerateBlock (x + i, y - 1, seed, x, y);
							}
						}
						else {
							Debug.Log ("Impossiblé!!");
						}
					}
				}*/
				mGen.UpdateGrid(lastX-x, lastY-y, x, y);
				//pFind.refreshPath ();
				//pFind.refreshTarget ();
				lastX=x;
				lastY=y;
			}
		}	
		//pFind.refreshTarget ();
		if(time + .5 < Time.time) {
			time = Time.time;
			if(mGen.isPursuit()){
				float dist=float.MaxValue;
				Vector3 newTarget = Vector3.zero;
				GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
				foreach(GameObject newPlayer in players){
					if(newPlayer.Equals(playerObject)) continue;
					float newDist = (newPlayer.transform.position-playerObject.transform.position).magnitude;
					if(newDist<dist){
						dist = newDist;
						//float T = (newPlayer.transform.position-playerObject.transform.position).magnitude/2;
						newTarget = newPlayer.transform.position;// + newPlayer.transform.rotation.eulerAngles.normalized*T;
					}
				}
				AStar pf = GetComponent<AStar>();
			
				if(pf!=null){
					pf.setTarget(newTarget);
				}
			
			} else if(mGen.isEvade()){
				float dist=float.MaxValue;
				Vector3 newTarget = Vector3.zero;
				GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
				foreach(GameObject newPlayer in players){
					if(newPlayer.Equals(playerObject)) continue;
					Vector3 newDist = (playerObject.transform.position-newPlayer.transform.position);
					if(newDist.magnitude<dist){
						dist = newDist.magnitude;
						//float T = newDist.magnitude/2;
						newTarget = newPlayer.transform.position+newDist.normalized*10;// + newPlayer.transform.rotation.eulerAngles.normalized*T;
					}
				}
				AStar pf = GetComponent<AStar>();
				
				if(pf!=null){
					pf.setTarget(newTarget);
				}
			}
		}
		if (mGen.isFollow() || mGen.isWander() || mGen.isPursuit() || mGen.isEvade()){
			pFind.refreshPath ();
			if(mGen.path !=null && mGen.path.Count != 0) {
				playerController.setSpeed ();
				playerController.setDirection (mGen.path [0].curPosition - playerObject.transform.position);
			}
			else {
				if(mGen.isWander()){
					float x = UnityEngine.Random.Range(-10,10), z = UnityEngine.Random.Range(-10,10);
					Vector3 newTarget = new Vector3(x, 0, z);
					
					AStar pf = GetComponent<AStar>();
					
					if(pf!=null){
						pf.setTarget(newTarget);
					}
				} 
				else {
					mGen.setFollow (false);
					//mGen.setPursuit(false);
					//mGen.setEvade(false);
					
				}
			}
		}
	}
	
	public GameObject getPlayer(){
		return playerObject;
	}
	
	public void OnGUI()
	{
		if (GUILayout.Button("Return to Lobby"))
		{
			PhotonNetwork.LeaveRoom();  // we will load the menu level when we successfully left the room
		}
	}
	
	public void OnMasterClientSwitched(PhotonPlayer player)
	{
		Debug.Log("OnMasterClientSwitched: " + player);
		
		string message;
		InRoomChat chatComponent = GetComponent<InRoomChat>();  // if we find a InRoomChat component, we print out a short message
		if (chatComponent != null)
		{
			// to check if this client is the new master...
			if (player.isLocal)
			{
				message = "You are Master Client now.";
			}
			else
			{
				message = player.name + " is Master Client now.";
			}
			
			
			chatComponent.AddLine(message); // the Chat method is a RPC. as we don't want to send an RPC and neither create a PhotonMessageInfo, lets call AddLine()
		}
	}
	
	public void OnLeftRoom()
	{
		Debug.Log("OnLeftRoom (local)");
		
		// back to main menu        
		Application.LoadLevel(Menu.SceneNameMenu);
	}
	
	public void OnDisconnectedFromPhoton()
	{
		Debug.Log("OnDisconnectedFromPhoton");
		
		// back to main menu        
		Application.LoadLevel(Menu.SceneNameMenu);
	}
	
	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		Debug.Log("OnPhotonInstantiate " + info.sender);    // you could use this info to store this or react
	}
	
	public void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		if (PhotonNetwork.isMasterClient) {
			photonView.RPC ("sendSeed", PhotonTargets.OthersBuffered, seed);
		}
		Debug.Log("OnPhotonPlayerConnected: " + player);
	}
	
	public void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		Debug.Log("OnPlayerDisconneced: " + player);
	}
	
	public void OnFailedToConnectToPhoton()
	{
		Debug.Log("OnFailedToConnectToPhoton");
		
		// back to main menu        
		Application.LoadLevel(Menu.SceneNameMenu);
	}
	
	[RPC]
	public void sendSeed(int seed){
		this.seed = seed;
	}
}
