using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class InRoomChat : Photon.MonoBehaviour 
{
	public Rect GuiRect = new Rect(0,0, 250,300);
	public bool IsVisible = true;
	public bool AlignBottom = false;
	public List<string> messages = new List<string>();
	private string inputLine = "";
	private Vector2 scrollPos = Vector2.zero;
	
	public static readonly string ChatRPC = "Chat";
	public void Start()
	{
		if (this.AlignBottom)
		{
			this.GuiRect.y = Screen.height - this.GuiRect.height;
		}
	}
	
	public void OnGUI()
	{
		if (!this.IsVisible || PhotonNetwork.connectionStateDetailed != PeerState.Joined)
		{
			return;
		}
		
		if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
		{
			if (!string.IsNullOrEmpty(this.inputLine))
			{
				this.photonView.RPC("Chat", PhotonTargets.All, this.inputLine);
				this.inputLine = "";
				GUI.FocusControl("");
				return; // printing the now modified list would result in an error. to avoid this, we just skip this single frame
			}
			else
			{
				GUI.FocusControl("ChatInput");
			}
		}
		
		GUI.SetNextControlName("");
		GUILayout.BeginArea(this.GuiRect);
		
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.FlexibleSpace();
		for (int i = messages.Count - 1; i >= 0; i--)
		{
			GUILayout.Label(messages[i]);
		}
		GUILayout.EndScrollView();
		
		GUILayout.BeginHorizontal();
		GUI.SetNextControlName("ChatInput");
		inputLine = GUILayout.TextField(inputLine);
		if (GUILayout.Button("Send", GUILayout.ExpandWidth(false)))
		{
			this.photonView.RPC("Chat", PhotonTargets.All, this.inputLine);
			this.inputLine = "";
			GUI.FocusControl("");
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	[RPC]
	public void Chat(string newLine, PhotonMessageInfo mi)
	{
		string senderName = "anonymous";
		
		if (mi != null && mi.sender != null)
		{
			if (!string.IsNullOrEmpty(mi.sender.name))
			{
				senderName = mi.sender.name;
			}
			else
			{
				senderName = "player " + mi.sender.ID;
			}
		}
		
		this.messages.Add(senderName +": " + newLine);
		
		
		if (!mi.sender.isLocal)
			return;
		newLine = newLine + "       ";
		if (newLine.Substring (0, 5) == "/goto") {
			float x, z;
			string s = newLine.Substring(5);
			string [] values = s.Split(',');
			if(values.Length == 2){ 
				x=Convert.ToSingle(values[0]);
				z=Convert.ToSingle(values[1]);
				
				Vector3 newTarget = new Vector3(x, 0, z);
				
				MapGeneration mg = GetComponent<MapGeneration>();
				AStar pf = GetComponent<AStar>();
				
				if(mg!=null && pf!=null){
					pf.setTarget(newTarget);
					mg.setFollow(true);
					mg.setWander(false);
					mg.setPursuit(false);
					mg.setEvade(false);
				}
			}
		}
		else if (newLine.Substring (0, 5) == "/stop") {
			MapGeneration mg = GetComponent<MapGeneration>();
			
			if(mg!=null){
				mg.setFollow(false);
				mg.setWander(false);
				mg.setPursuit(false);
				mg.setEvade(false);
			}
		}
		else if (newLine.Substring (0, 6) == "/clear") {
			MapGeneration mg = GetComponent<MapGeneration>();
			
			if(mg!=null){
				mg.path.Clear ();
			}
		}
		else if (newLine.Substring (0, 7) == "/wander") {
			MapGeneration mg = GetComponent<MapGeneration>();
			
			if(mg!=null){
				mg.setFollow(false);
				mg.setWander(true);
				mg.setPursuit(false);
				mg.setEvade(false);
			}
			
		}
		else if (newLine.Substring (0, 8) == "/pursuit") {
			MapGeneration mg = GetComponent<MapGeneration>();
			
			if(mg!=null){
				mg.setFollow(false);
				mg.setWander(false);
				mg.setPursuit(true);
				mg.setEvade(false);
			}
			
		}
		else if (newLine.Substring (0, 6)=="/evade") {
			MapGeneration mg = GetComponent<MapGeneration>();
			
			if(mg!=null){
				mg.setFollow(false);
				mg.setWander(false);
				mg.setPursuit(false);
				mg.setEvade(true);
			}
		}
	}
	
	public void AddLine(string newLine)
	{
		this.messages.Add(newLine);
	}
}
