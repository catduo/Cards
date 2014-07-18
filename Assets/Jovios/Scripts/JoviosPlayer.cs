using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;

public class JoviosPlayer{
	//instantiates a player, this can be called by the developer to add players manually
	public JoviosPlayer(int pNumber, JoviosUserID pUserID, string pName, Color pPrimary, Color pSecondary){
		playerNumber = pNumber;
		if(pName != null){
			playerName = pName;
		}
		else{
			playerName = "Hello World";
		}
		primary = pPrimary;
		secondary = pSecondary;
		userID = pUserID;
	}
	//this is an update to player info
	public void NewPlayerInfo(int pNumber, string pName, Color pPrimary, Color pSecondary){
		playerNumber = pNumber;
		if(pName != null){
			playerName = pName;
		}
		else{
			playerName = "Hello World";
		}
		primary = pPrimary;
		secondary = pSecondary;
		Debug.Log(pPrimary.ToString ());
	}
	//this is the list of listeners that will trigger when events come from the controller app
	private List<IJoviosControllerListener> controllerListeners = new List<IJoviosControllerListener>();
	public List<IJoviosControllerListener> GetControllerListeners(){
		return controllerListeners;
	}
	public void AddControllerListener(IJoviosControllerListener listener){
		controllerListeners.Add(listener);
	}
	public void RemoveControllerListener(IJoviosControllerListener listener){
		controllerListeners.Remove(listener);
	}
	public void RemoveAllControllerListeners(){
		controllerListeners = new List<IJoviosControllerListener>();
	}
	//the player number is the number associated with this player object from a compact list of players starting at 0
	private int playerNumber;
	public int GetPlayerNumber(){
		return playerNumber;
	}
	//the status object is not deleted when the player disconnects, it is kept to keep information about the player should they reconnect
	private GameObject statusObject;
	public GameObject GetStatusObject(){
		return statusObject;
	}
	public void SetStatusObject(GameObject newStatusObject){
		statusObject = newStatusObject;
	}
	//this is a list of player objects that will get deleted when the player disconnects
	private List<GameObject> playerObject = new List<GameObject>();
	public GameObject GetPlayerObject(int index = 0){
		if(playerObject.Count > index){
			return playerObject[index];
		}
		else{
			return null;
		}
	}
	public int PlayerObjectCount(){
		return playerObject.Count;
	}
	public int AddPlayerObject(GameObject newPlayerObject){
		playerObject.Add(newPlayerObject);
		return playerObject.Count - 1;
	}
	public int RemovePlayerObject(GameObject newPlayerObject){
		playerObject.Remove(newPlayerObject);
		return playerObject.Count;
	}
	//this is the number associated with the specific device being used
	private JoviosUserID userID;
	public JoviosUserID GetUserID(){
		return userID;
	}
	//this is the name of the player gained from the controller app
	private string playerName;
	public string GetPlayerName(){
		return playerName;
	}
	//this is the network player from the unity networking
	private NetworkPlayer networkPlayer;
	public NetworkPlayer GetNetworkPlayer(){
		return networkPlayer;
	}
	public void SetNetworkPlayer(NetworkPlayer np){
		networkPlayer = np;
	}
	//these colors are the first thing of customization that the controller sends over
	private Color primary;
	private Color secondary;
	public Color GetColor(string color){
		switch(color){
		case "primary":
			return primary;
			break;
		case "secondary":
			return secondary;
			break;
		default:
			Debug.Log("could not find color " + color);
			return Color.clear;
			break;
		}
	}
	//this is the currently set controller style for the controller app
	private JoviosControllerStyle controllerStyle;
	public void SetControllerStyle(JoviosControllerStyle newStyle){
		controllerStyle = newStyle;
	}
	public JoviosControllerStyle GetControllerStyle(){
		return controllerStyle;
	}
}