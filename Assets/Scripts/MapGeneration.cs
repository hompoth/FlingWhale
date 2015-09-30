using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGeneration : MonoBehaviour {
	public enum Layer0
	{
		Dirt = 0,
		Grass = 1,
		Water = 2,
	}
	public enum Layer1
	{
		Tree = 0,
	}
	public GameObject Tree, Bush, River;
	//private GameObject Bush;
	//private GameObject River;
	
	public int xLength = 5, yLength = 5, pixelRes = 32;
	//private int lastPosX, lastPosY, curPosX, curPosY;
	private Texture2D block; 
	private Texture2D[] tiles;
	private GameObject[,] blockArray;
	private bool follow;
	private bool wander;
	private bool pursuit;
	private bool evade;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	Node[,] grid;
	
	float nodeDiameter;
	int gridSizeX, gridSizeY;
	
	void Awake(){
		//River = Resources.Load ("RiverBlock") as GameObject;
		//Tree = Resources.Load ("Tree") as GameObject;
		//Bush = Resources.Load ("Bush") as GameObject;
		//lastPosX = 0; 
		//lastPosY = 0;
		//curPosX = 0; 
		//curPosY = 0;
		follow = false;
		wander = false;
		pursuit = false;
		evade = false;
		blockArray = new GameObject[9, 9];
		tiles = new Texture2D[18*4];
		Texture2D tile = Resources.Load ("dirt") as Texture2D;
		int k = 0;
		for (int i = 0; i < 6; i++) {
			for (int j = 0; j < 3; j++, k++) {
				tiles[k] = new Texture2D(32, 32);
				tiles[k].SetPixels(tile.GetPixels(j*32, i*32, 32, 32)); 	
			}
		}
		tile = Resources.Load ("dirt-grass") as Texture2D;
		for (int i = 0; i < 6; i++) {
			for (int j = 0; j < 3; j++, k++) {
				tiles[k] = new Texture2D(32, 32);
				tiles[k].SetPixels(tile.GetPixels(j*32, i*32, 32, 32)); 	
			}
		}
		tile = Resources.Load ("dirt-water") as Texture2D;
		for (int i = 0; i < 6; i++) {
			for (int j = 0; j < 3; j++, k++) {
				tiles[k] = new Texture2D(32, 32);
				tiles[k].SetPixels(tile.GetPixels(j*32, i*32, 32, 32)); 	
			}
		}
		tile = Resources.Load ("water-grass") as Texture2D;
		for (int i = 0; i < 6; i++) {
			for (int j = 0; j < 3; j++, k++) {
				tiles[k] = new Texture2D(32, 32);
				tiles[k].SetPixels(tile.GetPixels(j*32, i*32, 32, 32)); 	
			}
		}
		
		nodeDiameter = nodeRadius*2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
	}
	public bool isFollow(){
		return follow;
	}
	public bool isWander(){
		return wander;
	}
	public bool isPursuit(){
		return pursuit;
	}
	public bool isEvade(){
		return evade;
	}
	public void setFollow(bool _follow){
		follow = _follow;
	}
	public void setWander(bool _wander){
		wander = _wander;
	}
	public void setPursuit(bool _pursuit){
		pursuit = _pursuit;
	}
	public void setEvade(bool _evade){
		evade = _evade;
	}
	public void GenerateBlock (int x, int y, int seed, int posX, int posY) {
		//curPosX = posX;
		//curPosY = posY;
		int newSeed = (seed + x + y) ^ (y);
		Random.seed = newSeed;
		
		if (blockArray [(x - posX) + 4, (y - posY) + 4] != null) {
			return;
		}
		
		block = new Texture2D (xLength * pixelRes * 3, yLength * pixelRes * 3);
		
		float xshift = 3 * xLength / 2 - 1f;
		float yshift = 3 * yLength / 2 - 1f;
		
		GameObject child;
		GameObject parent = GameObject.CreatePrimitive(PrimitiveType.Plane);
		parent.transform.position = new Vector3 ((float)(x*3*xLength), 0, (float)(y*3*yLength));
		parent.transform.localScale = new Vector3 (.3f*xLength, 1, .3f*yLength);
		parent.name = "("+x+", "+y+")";
		
		x *= 3*xLength;
		y *= 3*yLength;
		
		Layer0[] arr = new Layer0[xLength*yLength];
		for (int i = 0; i < xLength; ++i) {
			for (int j = 0; j < yLength; ++j) {
				float curVal = Random.value;
				if(curVal<.3){
					arr[i*xLength+j] = Layer0.Dirt;
				} else if(curVal<.5) {
					arr[i*xLength+j] = Layer0.Water;
					child = Instantiate (River, new Vector3 ((float)(i*3-xshift + x), 0.1f, (float)(j*3-yshift + y)), Quaternion.identity) as GameObject;
					child.transform.parent = parent.transform;
				} else {
					arr[i*xLength+j] = Layer0.Grass;
					curVal = Random.value;
					if(curVal<.25) {
						child = Instantiate (Tree, new Vector3 ((float)(i*3-xshift + x), 0.1f, (float)(j*3-yshift + y)), Quaternion.identity) as GameObject;
						child.transform.parent = parent.transform;
					}
					else if(curVal<.5){
						child = Instantiate (Bush, new Vector3 ((float)(i*3-xshift + x), 0.1f, (float)(j*3-yshift + y)), Quaternion.identity) as GameObject;
						child.transform.parent = parent.transform;
					}
					
				}
			}
		}
		createTile(arr, block); 
		block.Apply ();
		parent.renderer.material.mainTexture = block;
		x /= 3*xLength;
		y /= 3*yLength;
		blockArray [(x-posX + 4), (y-posY + 4)] = parent;
		//lastPosX = curPosX;
		//lastPosY = curPosY;
	}
	
	public void shiftBlockArray (int diffX, int diffY){//int _lastPosX, int _lastPosY, int posX, int posY){
		//int diffX = -(lastPosX - curPosX);
		//int diffY = -(lastPosY - curPosY);
		if (diffX == diffY && diffY == 0)
			return;
		int len = 9;
		GameObject[,] saveArray = new GameObject[len, len];
		for (int i = 0; i < len; ++i) {
			for (int j = 0; j < len; ++j) {
				if(j+diffX>=0 && j+diffX<len &&
				   i+diffY>=0 && i+diffY<len){
					saveArray[j+diffX, i+diffY] = blockArray[j,i];
				}
				else {
					Object removeThis = blockArray[j,i];
					blockArray[j,i] = null;
					Destroy(removeThis);
				}
			}
		}
		blockArray = saveArray;
	}
	Color[] getTilePixels (int offset, int tileOrder){
		return tiles[tileOrder+offset].GetPixels(0,0,32,32);
	}
	void setTilePixels(int i, int k, int j, int l, Color[] t){
		block.SetPixels((j*3+l)*32, (i*3+k)*32, 32, 32, t);
	}
	void createTile(Layer0[] arr, Texture2D block){
		for (int i = 0; i < xLength; ++i) {
			for (int j = 0; j < yLength; ++j) {
				if (arr[xLength*yLength-1-(j*xLength+i)] == Layer0.Dirt) {
					int[] tileOrder = { 13, 10, 14, 8, -18, 6, 16, 4, 17};
					for (int k = 0; k < 3; ++k) {
						for (int l = 0; l < 3; ++l) {
							setTilePixels(i, k, j, l, getTilePixels(18, tileOrder[k*3+l]));
						}
					}
				} else if (arr[xLength*yLength-1-(j*xLength+i)] == Layer0.Water) {
					for (int k = 0; k < 3; ++k) {
						for (int l = 0; l < 3; ++l) {
							setTilePixels(i, k, j, l, getTilePixels(18*3, 3+k*3+l));
						}
					}
				} else { // Grass
					for (int k = 0; k < 3; ++k) {
						for (int l = 0; l < 3; ++l) {
							setTilePixels(i, k, j, l, getTilePixels(18, Random.Range (0, 3)));
						}
					}
				}
			}
		}
	}
	
	public void UpdateGrid(int diffX, int diffY, int curPosX, int curPosY) {
		//CreateGrid ();
		diffX *= xLength * 3;
		diffY *= yLength * 3;
		if (diffX == diffY && diffY == 0)
			return;
		Node[,] saveArray = new Node[gridSizeX, gridSizeY];
		for (int i = 0; i < gridSizeX; ++i) {
			for (int j = 0; j < gridSizeY; ++j) {
				if(j-diffX>=0 && j-diffX<gridSizeX &&
				   i-diffY>=0 && i-diffY<gridSizeY){
					saveArray[j,i] = grid[j-diffX,i-diffY];
					saveArray[j,i].gridX=j;
					saveArray[j,i].gridY=i;
				}
				else {
					Vector3 worldPoint = new Vector3 (curPosX*xLength*3 + j -gridSizeX/2 + 1, 0, curPosY*yLength*3 + i -gridSizeY/2 + 1);
					//Debug.Log (j+" "+i+": "+worldPoint);
					bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));
					saveArray[j, i] = new Node(walkable,worldPoint,j,i);
				}
			}
		}	
		/*List<Node> removeThese = new List<Node>();
		foreach(Node node in path){
			node.gridX -= diffX;
			node.gridY -= diffY;
			if(!(node.gridX >=0 && node.gridX<gridSizeX && node.gridY >=0 && node.gridY<gridSizeY)) removeThese.Add(node);
		}
		foreach (Node node in removeThese) {
			path.Remove (node);
		}*/
		Debug.Log ("Update");
		grid = saveArray;
		
	}
	public void CreateGrid(int curPosX, int curPosY){
		grid = new Node[gridSizeX, gridSizeY];
		for (int x = 0; x < gridSizeX; ++x) {
			for (int y = 0; y < gridSizeY; ++y) {
				Vector3 worldPoint = new Vector3 (curPosX*xLength*3 + x -gridSizeX/2 + 1, 0, curPosY*yLength*3 + y -gridSizeY/2 + 1);
				bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));
				grid[x,y] = new Node(walkable,worldPoint, x,y);
			}
		}
	}
	
	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();
		
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;
				
				int checkX = node.gridX + x;
				int checkY = node.gridY + y;
				
				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX,checkY]);
				}
			}
		}
		
		return neighbours;
	}
	
	public Node NodeFromWorldPoint(Vector3 worldPosition, int curPosX, int curPosY) {
		
		
		float percentX = (worldPosition.x + gridWorldSize.x/2 - curPosX * xLength*3) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y/2 - curPosY * yLength*3) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);
		
		int x = (int)((gridSizeX-1) * percentX);
		int y = (int)((gridSizeY-1) * percentY);
		//Debug.Log ("x:"+(int)(worldPosition.x + gridWorldSize.x / 2 - curPosX * xLength * 3));
		//Debug.Log ("y:"+(int)(worldPosition.z + gridWorldSize.y / 2 - curPosY * yLength * 3));
		//Debug.Log (curPosX + ", " + curPosY+":    "+x+", "+y);
		if (!grid [x, y].canWalk) {
			Node current = grid[x,y];
			int i = 0, j = 0;
			for(int k = 0; !current.canWalk; k++){
				int roundedK = k/8+1;
				if((k%8) == 0){
					i=-roundedK;
					j=roundedK;
				} else if((k%8) == 7){
					i=0;
					j=roundedK;
				} else if((k%8) == 6){
					i=roundedK;
					j=roundedK;
				} else if((k%8) == 5){
					i=-roundedK;
					j=0;
				} else if((k%8) == 4){
					i=roundedK;
					j=0;
				} else if((k%8) == 3){
					i=-roundedK;
					j=-roundedK;
				} else if((k%8) == 2){
					i=0;
					j=-roundedK;
				} else if((k%8) == 1){
					i=roundedK;
					j=-roundedK;
				}
				if(x+i<0 || y+j <0 || x+i>=gridSizeX || y+j>=gridSizeY) continue;
				current=grid[x+i,y+j];
				
			}
			return current;
		}
		return grid[x,y];
	}
	
	public List<Node> path;
	void OnDrawGizmos() {
			if (grid != null) {
			foreach (Node n in grid) {
				Gizmos.color = (n.canWalk)?Color.white:Color.red;
				if (path != null){
					if (path.Contains(n)){
						Gizmos.color = Color.black;
					}
				}
				if(n.curPosition == Vector3.zero) Gizmos.color = Color.blue;
				Gizmos.DrawWireCube(n.curPosition, (new Vector3(1, .1f, 1)) * (nodeDiameter-.2f));
			}
		}
	}
}











