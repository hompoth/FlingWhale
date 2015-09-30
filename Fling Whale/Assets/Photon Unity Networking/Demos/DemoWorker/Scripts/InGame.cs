using UnityEngine;

public class InGame : Photon.MonoBehaviour
{
    public Transform playerPrefab;
	private GameObject playerObject;
	private int seed;
	private PhotonView photonView;
	private MapGeneration mGen;
	private bool Loaded;
	private int lastX;
	private int lastY;

    public void Awake()
	{
		Loaded = false;
		mGen = gameObject.AddComponent<MapGeneration> (); //new MapGeneration();
		photonView = GetComponent<PhotonView> ();
		seed = 0;
		if (PhotonNetwork.isMasterClient) {
			seed = Random.seed;
			for (int i = -1; i < 2; ++i) {
				for(int j = -1; j < 2; ++j){
					mGen.GenerateBlock (i, j, seed);
				}
			}
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
    }

	public void Update(){
		if (seed != 0 && Loaded == false) {
			Loaded = true;
			lastX = 0;
			lastY = 0;
			for (int i = -1; i < 2; ++i) {
				for(int j = -1; j < 2; ++j){
					mGen.GenerateBlock (i, j, seed);
				}
			}
		} else {
			float x1 = playerObject.transform.position.x/15;
			float y1 = playerObject.transform.position.z/15;
			int x = (int)x1, y = (int)y1;
			//Debug.Log(x1 + " and " + y1 +";"+x+" and "+y);
			if(lastX!=x||lastY!=y){
				if(Mathf.Abs((lastX+lastY)-(x+y))>2){
					Debug.Log("Moving too fast");
					for (int i = -1; i < 2; ++i) {
						for(int j = -1; j < 2; ++j){
							mGen.GenerateBlock (i + x, j + y, seed);
						}
					}
				}
				else {
					if(x-lastX>0){ 
						if(y-lastY>0){ // Top right corner
							Debug.Log ("Corner-TR");
						} else if(y-lastY<0){ // Bottom right corner
							Debug.Log ("Corner-BR");
						}
						else { // To the right
							Debug.Log ("Right");
							for(int j = -1; j < 2; ++j){
								mGen.GenerateBlock (x + 1, y + j, seed);
							}
						}
					}else if(x-lastX<0){
						if(y-lastY>0){ // Top left corner
							Debug.Log ("Corner-TL");
						} else if(y-lastY<0){ // Bottom left corner
							Debug.Log ("Corner-BL");
						}
						else { // To the left
							Debug.Log ("Left");
							for(int j = -1; j < 2; ++j){
								mGen.GenerateBlock (x - 1, y + j, seed);
							}
						}
					}
					else {
						if(y-lastY>0){ // To the up
							Debug.Log ("Up");
							for(int i = -1; i < 2; ++i){
								mGen.GenerateBlock (x + i, y + 1, seed);
							}
						} else if(y-lastY<0){ // To the down
							Debug.Log ("Down");
							for(int i = -1; i < 2; ++i){
								mGen.GenerateBlock (x + i, y - 1, seed);
							}
						}
						else {
							Debug.Log ("Impossiblé!!");
						}
					}
				}
				lastX=x;
				lastY=y;
			}
		}
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
