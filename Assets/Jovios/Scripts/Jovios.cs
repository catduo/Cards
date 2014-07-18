using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using MiniJSON;
using System.Text;
using System;

//TODO List



public class Jovios : MonoBehaviour, IJoviosNetworkListen {
	
	//this is the connection string for the player to type into the controller
	public string gameName{get; set;}

	//this is the networking framework connection
	public JoviosNetworking networking;
	
	//this will call the approriate jovios object creation code, such that it will work properly with Unity
	public static Jovios Create(){
		if(GameObject.Find ("JoviosObject") == null){
			GameObject joviosGameObject = new GameObject();
			joviosGameObject.AddComponent<Jovios>();
			joviosGameObject.AddComponent<LogInBuild>();
			joviosGameObject.GetComponent<Jovios>().networking = joviosGameObject.AddComponent<JoviosNetworking>();
			joviosGameObject.AddComponent<NetworkView>();
			joviosGameObject.name = "JoviosObject";
			joviosGameObject.networkView.stateSynchronization = NetworkStateSynchronization.Off;
			joviosGameObject.GetComponent<JoviosNetworking>().jovios = joviosGameObject.GetComponent<Jovios>();
			joviosGameObject.GetComponent<JoviosNetworking>().parser = joviosGameObject.GetComponent<Jovios>();
			return joviosGameObject.GetComponent<Jovios>();
		}
		else{
			return GameObject.Find ("JoviosObject").GetComponent<Jovios>();
		}
	}
	
	//Players are stored in a list and you can get the player calling GetPlayer(id) with id being either the PlayerNumber or the JoviosUserID
	private List<JoviosPlayer> players = new List<JoviosPlayer>();
	private Dictionary<int, int> deviceIDToPlayerNumber = new Dictionary<int, int>();
	JoviosPlayer IJoviosNetworkListen.GetPlayer(int i){
		return GetPlayer (i);
	}
	public JoviosPlayer GetPlayer(int playerNumber){
		if(playerNumber < players.Count){
			return players[playerNumber];
		}
		else{
			return null;
		}
	}
	JoviosPlayer IJoviosNetworkListen.GetPlayer(JoviosUserID jUID){
		return GetPlayer (jUID);
	}
	public JoviosPlayer GetPlayer(JoviosUserID jUID){
		if(deviceIDToPlayerNumber.ContainsKey(jUID.GetIDNumber())){
			if(players.Count > deviceIDToPlayerNumber[jUID.GetIDNumber()]){
				return players[deviceIDToPlayerNumber[jUID.GetIDNumber()]];
			}
			else{
				return null;
			}
		}
		else{
			return null;
		}
	}
	int IJoviosNetworkListen(){
		return GetPlayerCount ();
	}
	public int GetPlayerCount(){
		if(players != null){
			return players.Count;
		}
		else{
			return 0;
		}
	}
	
	
	//listening to when players connect, disconnect, and change their information
	private List<IJoviosPlayerListener> playerListeners = new List<IJoviosPlayerListener>();
	public void AddPlayerListener(IJoviosPlayerListener listener){
		playerListeners.Add(listener);
	}
	public void RemovePlayerListener(IJoviosPlayerListener listener){
		playerListeners.Remove(listener);
	}
	public void RemoveAllPlayerListeners(){
		playerListeners = new List<IJoviosPlayerListener>();
	}
	
	
	//listening to each player's controller
	public List<IJoviosControllerListener> GetControllerListeners(JoviosUserID jUID){
		return GetPlayer(jUID).GetControllerListeners();
	}
	public List<IJoviosControllerListener> GetControllerListeners(int playerNumber){
		return GetPlayer(playerNumber).GetControllerListeners();
	}
	public List<IJoviosControllerListener> GetControllerListeners(){
		List<IJoviosControllerListener> allControllerListeners = new List<IJoviosControllerListener>();
		foreach(JoviosPlayer player in players){
			foreach(IJoviosControllerListener listener in player.GetControllerListeners()){
				allControllerListeners.Add(listener);
			}
		}
		return allControllerListeners;
	}
	public void AddControllerListener(IJoviosControllerListener listener){
		foreach(JoviosPlayer player in players){
			player.AddControllerListener(listener);
		}
	}
	public void AddControllerListener(IJoviosControllerListener listener, int playerNumber){
		GetPlayer(playerNumber).AddControllerListener(listener);
	}
	public void AddControllerListener(IJoviosControllerListener listener, JoviosUserID jUID){
		if(GetPlayer(jUID) != null){
			GetPlayer(jUID).AddControllerListener(listener);
		}
	}
	public void RemoveControllerListener(IJoviosControllerListener listener){
		foreach(JoviosPlayer player in players){
			player.RemoveControllerListener(listener);
		}
	}
	public void RemoveControllerListener(IJoviosControllerListener listener, int playerNumber){
		GetPlayer(playerNumber).RemoveControllerListener(listener);
	}
	public void RemoveControllerListener(IJoviosControllerListener listener, JoviosUserID jUID){
		GetPlayer(jUID).RemoveControllerListener(listener);
	}
	public void RemoveAllControllerListeners(){
		foreach(JoviosPlayer player in players){
			player.RemoveAllControllerListeners();
		}
	}
	public void RemoveAllControllerListeners(int playerNumber){
		GetPlayer(playerNumber).RemoveAllControllerListeners();
	}
	public void RemoveAllControllerListeners(JoviosUserID jUID){
		GetPlayer(jUID).RemoveAllControllerListeners();
	}
	
	public void SetControls(JoviosUserID jUID, string presetController){
		if(controllerStyles.ContainsKey(presetController)){
			JoviosControllerStyle jcs = new JoviosControllerStyle();
			GameObject go = controllerStyles[presetController];
			for(int i = 0; i < go.transform.childCount; i++){
				go.transform.GetChild(i).GetComponent<JoviosControllerConstructor>().AddControllerComponent(jcs);
			}
			SetControls(jUID, jcs);
		}
		else{
			Debug.Log ("wrong key: " + presetController);
		}
	}
	
	//this will set the controlls of a given player
	public void SetControls(JoviosUserID jUID, JoviosControllerStyle controllerStyle){
		networking.SetControls (jUID, controllerStyle);
	}
	
	Dictionary<string, GameObject> controllerStyles = new Dictionary<string, GameObject>();
	Texture2D[] atlasList = new Texture2D[1];
	Rect[] atlasRects = new Rect[1];
	Texture2D atlas = new Texture2D(8192,8192);
	string atlasDef = "";
	string atlasNames = "";
	Texture2D backgroundTexture = new Texture2D(1,1);
	public void StartServer(List<GameObject> setControllerStyles, List<Texture2D> setExportTextures, string thisGameName, Texture2D setBackgroundTexture){
		networking = GameObject.Find ("JoviosObject").GetComponent<JoviosNetworking> ();
		networking.parser = this;
		if(setBackgroundTexture != null){
			backgroundTexture = setBackgroundTexture;
		}
		controllerStyles = new Dictionary<string, GameObject>();
		foreach(GameObject go in setControllerStyles){
			controllerStyles.Add(go.name, go);
		}
		StartServer (thisGameName);
	}
	
	//this starts the unity server and udp broadcast to local network
	public string iconURL{get; set;}
	public string gameCode{get; private set;}
	string IJoviosNetworkListen.GetUserName(){
		return "0";
	}
	string IJoviosNetworkListen.GetGameCode(){
		return gameCode;
	}
	public void StartServer(string thisGameName = ""){
		networking.NodeLinux();
		networking.UnityNetworking ();
		networking.UDPBroadcast ();
		Application.runInBackground = true;
		SetGameName(thisGameName);
	}

	void IJoviosNetworkListen.JoinedNodeLinux(string packet){
		Debug.Log (packet);
	}
	
	void IJoviosNetworkListen.SetGameCode(string gameID){
		gameCode = gameID;
	}

	//this sets the gamename
	void SetGameName(string newGameName){
		if(newGameName != ""){
			gameName = newGameName;
		}
		else{
			gameName = "New Game";
		}
	} 

	//this parses the incoming packets
	void IJoviosNetworkListen.ParsePacket(string packet){
		Dictionary<string, object> myJSON = Json.Deserialize(packet) as Dictionary<string, object>;
		Dictionary<string, object> packetJSON = (Dictionary<string, object>)myJSON ["packet"];
		if(packetJSON.ContainsKey("response")){
			List<object> responseJSON = (List<object>)packetJSON ["response"];
			for(int i = 0; i < responseJSON.Count; i++){
				Dictionary<string, object> thisResponse = (Dictionary<string, object>) responseJSON[i];
				switch((string)thisResponse["type"]){
				case "button":
					ButtonPress(int.Parse(Json.Serialize(myJSON["deviceID"])), Json.Serialize(thisResponse));
					break;
					
				case "direction":
					switch((string)thisResponse["action"]){
					case "hold":
						if(GetPlayer(new JoviosUserID(int.Parse(Json.Serialize(myJSON["deviceID"])))).GetControllerStyle().GetDirection((string)thisResponse["direction"]) != null){
							GetPlayer(new JoviosUserID(int.Parse(Json.Serialize(myJSON["deviceID"])))).GetControllerStyle().GetDirection((string)thisResponse["direction"]).SetDirection(new Vector2(float.Parse(Json.Serialize(((List<object>)thisResponse["position"])[0])), float.Parse(Json.Serialize(((List<object>)thisResponse["position"])[1]))));
						}
						break;
						
					case "press":
						ButtonPress(int.Parse(Json.Serialize(myJSON["deviceID"])), Json.Serialize(thisResponse));
						break;
						
					case "release":
						ButtonPress(int.Parse(Json.Serialize(myJSON["deviceID"])), Json.Serialize(thisResponse));
						break;
						
					default:
						break;
					}
					break;
					
				case "accelerometer":
					var accInfo = thisResponse;
					GetPlayer(new JoviosUserID(int.Parse(Json.Serialize(myJSON["deviceID"])))).GetControllerStyle().GetAccelerometer().SetGyro(new Quaternion(-float.Parse(Json.Serialize(accInfo["gyroX"])), -float.Parse(Json.Serialize(accInfo["gyroY"])), float.Parse(Json.Serialize(accInfo["gyroZ"])), float.Parse(Json.Serialize(accInfo["gyroW"]))));
					GetPlayer(new JoviosUserID(int.Parse(Json.Serialize(myJSON["deviceID"])))).GetControllerStyle().GetAccelerometer().SetAcceleration(new Vector3(float.Parse(Json.Serialize(accInfo["accx"])), float.Parse(Json.Serialize(accInfo["accy"])), float.Parse(Json.Serialize(accInfo["accZ"]))));
					break;
					
				default:
					Debug.Log ("wrong response type");
					break;
				}
			}
		}
		if(packetJSON.ContainsKey("playerConnected")){
			PlayerConnectedInterpret.Add(Json.Serialize (myJSON));
		}
		if(packetJSON.ContainsKey("playerUpdated")){
			Dictionary<string, object> playerUpdatedJSON = (Dictionary<string, object>) packetJSON["playerUpdated"];
			PlayerUpdated(int.Parse(Json.Serialize(playerUpdatedJSON["deviceID"])), float.Parse(Json.Serialize(playerUpdatedJSON["primaryR"])), float.Parse(Json.Serialize(playerUpdatedJSON["primaryG"])), float.Parse(Json.Serialize(playerUpdatedJSON["primaryB"])), float.Parse(Json.Serialize(playerUpdatedJSON["secondaryR"])), float.Parse(Json.Serialize(playerUpdatedJSON["secondaryG"])), float.Parse(Json.Serialize(playerUpdatedJSON["secondaryB"])), (string)playerUpdatedJSON["playerName"]);
		}
	}

	public void ButtonPress(int player, string buttonJSON){
		Dictionary<string, object> myJSON = Json.Deserialize (buttonJSON) as Dictionary<string, object>;
		switch((string)myJSON["type"]){
		case "button":
			JoviosButtonEvent e = new JoviosButtonEvent((string)myJSON["button"], GetPlayer(new JoviosUserID(player)).GetControllerStyle(), (string)myJSON["action"]);
			foreach(IJoviosControllerListener listener in GetPlayer(new JoviosUserID(player)).GetControllerListeners()){
				if(listener.ButtonEventReceived(e)){
					return;
				}
			}
			if(GetPlayer(new JoviosUserID(player)).GetControllerStyle().GetButton((string)myJSON["button"]) != null){
				if((string)myJSON["action"] == "press"){
					GetPlayer(new JoviosUserID(player)).GetControllerStyle().GetButton((string)myJSON["button"]).is_pressed = true;
				}
				else{
					GetPlayer(new JoviosUserID(player)).GetControllerStyle().GetButton((string)myJSON["button"]).is_pressed = false;
				}
			}
			break;
			
		case "direction":
			JoviosButtonEvent e1 = new JoviosButtonEvent((string)myJSON["direction"], GetPlayer(new JoviosUserID(player)).GetControllerStyle(), (string)myJSON["action"]);
			foreach(IJoviosControllerListener listener in GetPlayer(new JoviosUserID(player)).GetControllerListeners()){
				if(listener.ButtonEventReceived(e1)){
					return;
				}
			}
			if(GetPlayer(new JoviosUserID(player)).GetControllerStyle().GetButton((string)myJSON["direction"]) != null){
				if(myJSON["action"] == "press"){
					GetPlayer(new JoviosUserID(player)).GetControllerStyle().GetDirection((string)myJSON["direction"]).is_pressed = true;
				}
				else{
					GetPlayer(new JoviosUserID(player)).GetControllerStyle().GetDirection((string)myJSON["direction"]).is_pressed = false;
				}
			}
			break;
			
		default:
			break;
		}
	}

	void IJoviosNetworkListen.OnLeave(string packet){
		PlayerDisconnectedInterpret.Add(int.Parse(packet));
	}
	
	void IJoviosNetworkListen.OnAdd(string packet){

	}

	public List<string> PlayerConnectedInterpret = new List<string>();
	public List<int> PlayerDisconnectedInterpret = new List<int>();
	Dictionary<string, int> setInts = new Dictionary<string, int>();
	void FixedUpdate(){
		foreach(KeyValuePair<string, int> kvp in setInts){
			PlayerPrefs.SetInt(kvp.Key, kvp.Value);
		}
		setInts = new Dictionary<string, int> ();
		if(PlayerConnectedInterpret.Count > 0){
			for(int i = 0; i < PlayerConnectedInterpret.Count; i++){
				Dictionary<string, object> myJSON = Json.Deserialize(PlayerConnectedInterpret[i]) as Dictionary<string, object>;
				Dictionary<string, object> playerConnectedPacketJSON = (Dictionary<string, object>)myJSON["packet"];
				Dictionary<string, object> playerConnected = (Dictionary<string, object>)playerConnectedPacketJSON["playerConnected"];
				PlayerConnected((string)playerConnected["ip"], (string)playerConnected["networkType"], int.Parse(Json.Serialize(playerConnected["playerNumber"])), float.Parse(Json.Serialize(playerConnected["primaryR"])), float.Parse(Json.Serialize(playerConnected["primaryG"])), float.Parse(Json.Serialize(playerConnected["primaryB"])), float.Parse(Json.Serialize(playerConnected["secondaryR"])), float.Parse(Json.Serialize(playerConnected["secondaryG"])), float.Parse(Json.Serialize(playerConnected["secondaryB"])), (string)playerConnected["playerName"], int.Parse(Json.Serialize(playerConnected["deviceID"])));
			}
			PlayerConnectedInterpret = new List<string>();
		}
		for(int i = 0; i < PlayerDisconnectedInterpret.Count; i++){
			PlayerDisconnected(GetPlayer(new JoviosUserID(PlayerDisconnectedInterpret[i])));
		}
		PlayerDisconnectedInterpret = new List<int>();
	}
	
	//This will be called by the connection scripts and will manage player connections
	public string assetBundleURL{get; set;}
	public void PlayerConnected(string ip, string networkType, int playerNumber, float primaryR, float primaryG, float primaryB, float secondaryR, float secondaryG, float secondaryB, string playerName, int deviceID){
		players.Add(new JoviosPlayer(GetPlayerCount(), new JoviosUserID(deviceID), playerName, new Color(primaryR, primaryG, primaryB, 1), new Color(secondaryR, secondaryG, secondaryB, 1)));
		if(!deviceIDToPlayerNumber.ContainsKey(deviceID)){
			deviceIDToPlayerNumber.Add(deviceID, players.Count - 1);
			networking.PlayerConnected(deviceID, playerNumber, networkType);
			if(assetBundleURL != "" && assetBundleURL != null){
				networking.AddToPacket(new JoviosUserID(deviceID), "'assetBundle':'"+assetBundleURL+"'");
			}
		}
		else{
			deviceIDToPlayerNumber[deviceID] = playerNumber;
		}
		foreach(IJoviosPlayerListener listener in playerListeners){
			if(listener.PlayerConnected(GetPlayer(new JoviosUserID(deviceID)))){
				break;
			}
		}
	}

	// this will be triggered when information about a player is updated, like colors or names
	public void PlayerUpdated(int deviceID, float primaryR, float primaryG, float primaryB, float secondaryR, float secondaryG, float secondaryB, string playerName){
		GetPlayer(new JoviosUserID(deviceID)).NewPlayerInfo(deviceIDToPlayerNumber[deviceID], playerName, new Color(primaryR, primaryG, primaryB, 1), new Color(secondaryR, secondaryG, secondaryB, 1));
		foreach(IJoviosPlayerListener listener in playerListeners){
			if(listener.PlayerUpdated(GetPlayer(new JoviosUserID(deviceID)))){
				break;
			}
		}
	}
	
	// this will trigger when a player disconnects,
	public void PlayerDisconnected(JoviosPlayer p){
		if(players.Contains(p)){
			players.Remove(p);
		}
		networking.PlayerDisconnected (p);
		if(p != null){
			if(deviceIDToPlayerNumber.ContainsKey(p.GetUserID().GetIDNumber())){
				deviceIDToPlayerNumber.Remove(p.GetUserID().GetIDNumber());
				for(int i = 0; i < deviceIDToPlayerNumber.Count; i++){
					deviceIDToPlayerNumber[GetPlayer(i).GetUserID().GetIDNumber()] = i;
					players[i].NewPlayerInfo(i, players[i].GetPlayerName(), players[i].GetColor("primary"), players[i].GetColor("secondary"));
				}
				for(int i = 0; i < p.PlayerObjectCount(); i++){
					Destroy(p.GetPlayerObject(i));
				}
				foreach(IJoviosPlayerListener listener in playerListeners){
					if(listener.PlayerDisconnected(p)){
						break;
					}
				}
			}
		}
	}

	void IJoviosNetworkListen.Login(string loginInfo){
		Debug.Log (loginInfo);
	}
	
	void IJoviosNetworkListen.SetDeviceID(int deviceID){
		setInts.Add ("deviceID", deviceID);
	}
	
	string IJoviosNetworkListen.GetDeviceID(){
		return "0";
	}
	
	void IJoviosNetworkListen.SetPlayerID(int playerID){
		setInts.Add("playerID", playerID);
	}
}