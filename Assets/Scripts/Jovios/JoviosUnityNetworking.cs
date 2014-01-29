using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;

//move connection items to a different class
//take in array of userids to change player ordering
//versioning with the controller so that it always works with the games (think about how it would work, not actually doing it)
//look into unit testing on unity
//more comments and grouping
//IJovios for API help (list of all public functions)
//Add Button Release
//UI for 2 button and directional swiping
//Add non-relative directional inputs
//Dictionary reset on player disconnect
//list of players instead of array Network and Jovios


public class JoviosUnityNetworking : MonoBehaviour {
	
	private static Jovios jovios;
	private string gameName;
	
	public static Jovios Create(){
		GameObject joviosGameObject = new GameObject();
		joviosGameObject.AddComponent<Jovios>();
		joviosGameObject.AddComponent<JoviosUnityNetworking>();
		joviosGameObject.AddComponent<NetworkView>();
		joviosGameObject.name = "JoviosObject";
		joviosGameObject.networkView.stateSynchronization = NetworkStateSynchronization.Unreliable;
		joviosGameObject.networkView.observed = joviosGameObject.GetComponent<JoviosUnityNetworking>();
		jovios = joviosGameObject.GetComponent<Jovios>();
		return joviosGameObject.GetComponent<Jovios>();
	}
	
    public void StartServer(int maxPlayers = 32, int portNumber = 2501){
		Application.runInBackground = true;
        Network.InitializeServer(32, 25002, !Network.HavePublicAddress());
		if(Application.isWebPlayer){
			Application.ExternalCall("GetGameName");
		}
		else{
			SetGameName("bbbb");
		}
    } 
	
	private string externalIP;
	IEnumerator GetIP(){
		WWW www = new WWW("http://checkip.dyndns.org");
        yield return www;
		if(www.error == null){
			externalIP=www.text;
			externalIP=externalIP.Substring(externalIP.IndexOf(":")+1);
			externalIP=externalIP.Substring(0,externalIP.IndexOf("<"));
		}
		else{
			externalIP = Network.player.externalIP;
		}
        MasterServer.RegisterHost(typeName, gameName + externalIP);
        WWWForm form = new WWWForm();
        form.AddField("action","create");
        form.AddField ("name",gameName);
        WWW post_req = new WWW("http://localhost/foo.php",form);
		if(Application.isWebPlayer){
			Application.ExternalCall("SetGameName", gameName);
		}
		Debug.Log ("game name: " + gameName + ", ipAdress: " + externalIP);
	}
	private WWW wwwData = null;
	private List<NetworkPlayer> networkPlayers = new List<NetworkPlayer>();
	private const string typeName = "Jovios";
	
	void OnApplicationQuit(){
		Network.Disconnect();
	}
	
	private void SetGameName(string newGameName){
		if(newGameName.Length == 4){
			gameName = newGameName;
		}
		else{
            gameName = Guid.NewGuid().ToString().Split('-')[1];
		}
		jovios.SetGameName(gameName);
		StartCoroutine("GetIP");
	}    
	
	public IEnumerator WaitForDownload(){    
        yield return wwwData;
        wwwData.LoadUnityWeb();  
    }
	[RPC] public void NewUrl(string url){
		networkView.RPC("NewGame", RPCMode.Others);
        wwwData = new WWW(url);
        StartCoroutine("WaitForDownload");
	}
	
	void OnPlayerConnected(NetworkPlayer player){
		networkPlayers.Add(player);
		networkView.RPC ("PlayerObjectCreated", player, jovios.GetPlayerCount());
	}
	void OnPlayerDisconnected(NetworkPlayer player){
		jovios.PlayerDisconnected(jovios.GetPlayer(networkPlayers.IndexOf (player)));
		networkPlayers.Remove(player);
	}
	public void SetNetworkPlayer(int playerNumber){
		jovios.GetPlayer(playerNumber).SetNetworkPlayer(networkPlayers[playerNumber]);
	}   
	
	
	
	[RPC] void GetDirection(int userID, float horizontal, float vertical, string side){
		jovios.GetPlayer(new JoviosUserID(userID)).GetInput(side).SetDirection(new Vector2(horizontal, vertical));
	}
	[RPC] void GetAccelerometer(int userID, float gyroW, float gyroX, float gyroY, float gyroZ, float accX, float accY, float accZ){
		jovios.GetPlayer(new JoviosUserID(userID)).GetInput("accelerometer").SetGyro(new Quaternion(-gyroX, -gyroY, gyroZ, gyroW));
		jovios.GetPlayer(new JoviosUserID(userID)).GetInput("accelerometer").SetAcceleration(new Vector3(accX, accZ, accY));
	}
	[RPC] void GetTextResponse(int userID, string buttonPress, string side = "", string action = ""){
		Debug.Log ("text");
		JoviosButtonEvent e = new JoviosButtonEvent(buttonPress, jovios.GetPlayer(new JoviosUserID(userID)).GetControllerStyle(), side, action);
		foreach(IJoviosControllerListener listener in jovios.GetPlayer(new JoviosUserID(userID)).GetControllerListeners()){
			if(listener.ButtonEventReceived(e)){
				return;
			}
		}
	}
	
	[RPC] public void InstantiatePlayerObject(int playerNumber, float primaryR, float primaryG, float primaryB, float secondaryR, float secondaryG, float secondaryB, string playerName, int userID){
		if(jovios.GetPlayer(playerNumber) == null){
			jovios.PlayerConnected(playerNumber, primaryR, primaryG, primaryB, secondaryR, secondaryG, secondaryB, playerName, userID);
		}
		else{
			jovios.PlayerUpdated(playerNumber, primaryR, primaryG, primaryB, secondaryR, secondaryG, secondaryB, playerName, userID);
		}
	}
	
	[RPC] private void SentControls(int accelerometerSetting, string lControls, string lControlsResponse, string lControlsDescription, string rControls, string rControlsResponse, string rControlsDescription){}
	[RPC] private void PlayerObjectCreated(int player){}
	[RPC] private void EndOfRound(int player){}
	[RPC] private void NewGame(){}
	[RPC] private void SentButtons (int accelerometerSetting, string type, string question, string actionWord, string button1, string button2, string button3, string button4, string button5, string button6, string button7, string button8){}
	[RPC] private void SentArbitraryUIElement(int x, int y, int w, int h, string description, string response){}
}