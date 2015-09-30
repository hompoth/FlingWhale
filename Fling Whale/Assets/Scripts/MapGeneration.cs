using UnityEngine;
using System.Collections;

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
	private Object Tree = Resources.Load ("Tree");
	private Object Bush = Resources.Load ("Bush");

	//void Start(){}

	private int xLength = 5, yLength = 5;
	public void GenerateBlock (int x, int y, int seed) {
		GameObject parent = new GameObject ();
		GameObject child;
		parent.name = "("+x+", "+y+")";
		int newSeed = (seed + x + y) ^ (y);
		x *= 3*xLength;
		y *= 3*yLength;
		float shift = xLength/2*3-0.5f;
		Random.seed = newSeed;
		Layer0[] arr = new Layer0[xLength*yLength];
		for (int i = 0; i < xLength; ++i) {
			for (int j = 0; j < yLength; ++j) {
				float curVal = Random.value;
				if(curVal<.3){
					arr[i*xLength+j] = Layer0.Dirt;
				} else if(curVal<.5) {
					arr[i*xLength+j] = Layer0.Water;
				} else {
					arr[i*xLength+j] = Layer0.Grass;
					curVal = Random.value;
					if(curVal<.25) {
						child = Instantiate (Tree, new Vector3 ((float)(i*3+1-shift + x), 0, (float)(j*3+1-shift + y)), Quaternion.Euler(new Vector3(30,0,0))) as GameObject;
						child.transform.parent = parent.transform;
					}
					else if(curVal<.5){
						child = Instantiate (Bush, new Vector3 ((float)(i*3+1-shift + x), 0, (float)(j*3+1-shift + y)), Quaternion.Euler(new Vector3(30,0,0))) as GameObject;
						child.transform.parent = parent.transform;
					}

				}
				createTile(arr[i*xLength+j], i*3 - shift + x, j*3 - shift + y, parent); 
			}
		}
	}
	
	void createTile(Layer0 type, float x, float y, GameObject parent){
		Object tile;
		GameObject child;
		if (type == Layer0.Dirt) {
			tile = Resources.Load ("Dirt");
		} else if (type == Layer0.Water) {
			tile = Resources.Load ("Water");
		} else {
			tile = Resources.Load ("Grass");
		}
		for (int i = 0; i < 3; ++i) {
			for (int j = 0; j < 3; ++j) {
				child = Instantiate (tile, new Vector3 ((float)(x + i), 0, (float)(y + j)), Quaternion.identity) as GameObject;
				child.transform.parent = parent.transform;
			}
		}
	}
}
