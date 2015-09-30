using UnityEngine;
using System.Collections;

public class SortOrder : MonoBehaviour {

	private SpriteRenderer tempRend;
	void Awake(){
		tempRend = GetComponent<SpriteRenderer>();
	}
	void LateUpdate(){
		tempRend.sortingOrder = (int)(Camera.main.WorldToScreenPoint(tempRend.bounds.min).y * -1);
	}
}
