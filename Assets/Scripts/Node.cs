using UnityEngine;
using System.Collections;

public class Node {
	public int gCost;
	public int hCost;
	public Node parent;

	public bool canWalk;
	public Vector3 curPosition;
	public int gridX;
	public int gridY;
	
	public Node(bool _canWalk, Vector3 _curPosition, int _gridX, int _gridY) {
		canWalk = _canWalk;
		curPosition = _curPosition;
		gridX = _gridX;
		gridY = _gridY;
	}
	
	public int fCost {
		get {
			return gCost + hCost;
		}
	}
}