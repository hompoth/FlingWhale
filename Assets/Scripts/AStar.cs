using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStar : MonoBehaviour {
	
	public Vector3 seeker, target;
	MapGeneration grid;
	InGame ingame;
	//GameObject player;
	
	void Start() {
		grid = GetComponent<MapGeneration>();
		ingame = GetComponent<InGame> ();
		seeker = Vector3.zero;//player.transform.position;
	}
	public void refreshTarget(){
		setTarget (target);
	}
	public void setTarget(Vector3 _target){
		target = _target;
		FindPath(seeker,target);
	}
	public void refreshPath(){
		if (grid.path == null)
			return;		
		if (ingame != null) {
			seeker = ingame.getPlayer ().transform.position;
		}
		if(grid.path.Count <= 2 && (seeker-target).magnitude>5){
			refreshTarget();
			return;
		}
		if (grid.path.Count == 0)
			return;
		Node path = grid.path [0];
		if ((seeker - path.curPosition).magnitude < 1)
			grid.path.Remove (path);
		
	}
	void FindPath(Vector3 startPos, Vector3 targetPos) {
		if (ingame != null) {
			seeker = ingame.getPlayer ().transform.position;
		}
		Node startNode = grid.NodeFromWorldPoint(startPos, (int)((seeker.x/15)), (int)((seeker.z/15)));
		Node targetNode = grid.NodeFromWorldPoint(targetPos, (int)((seeker.x/15)), (int)((seeker.z/15)));
		
		List<Node> openSet = new List<Node>();
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);
		
		while (openSet.Count > 0) {
			Node currentNode = openSet[0];
			for (int i = 1; i < openSet.Count; i ++) {
				if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) {
					currentNode = openSet[i];
				}
			}
			
			openSet.Remove(currentNode);
			closedSet.Add(currentNode);
			
			if (currentNode == targetNode) {
				RetracePath(startNode,targetNode);
				return;
			}
			
			foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
				if (!neighbour.canWalk || closedSet.Contains(neighbour)) {
					continue;
				}
				
				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;
					
					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
				}
			}
			if(openSet.Count <= 1) 
				return;
		}
	}
	
	void RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;
		
		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse();
		
		grid.path = path;
		
	}
	
	int GetDistance(Node nodeA, Node nodeB) {
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
		
		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}
}