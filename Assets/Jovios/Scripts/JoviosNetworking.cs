using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
#if !UNITY_WEBPLAYER
using SimpleJson;
using WebSocket4Net;
using SocketIOClient;
using pomeloUnityClient;
#endif
using MiniJSON;
using System.Text;
using System;

public class JoviosNetworking: MonoBehaviour {

	public static bool CheckForServerConnection()
	{
		try{
			using (var client = new WebClient())
			using (var stream = client.OpenRead("http://54.186.22.19:3014")){
				return true;
			}
		}
		catch{
			return false;
		}
	}
	
	//this will set the controlls of a given player
	public void SetControls(JoviosUserID jUID, JoviosControllerStyle controllerStyle){
		parser.GetPlayer(jUID).SetControllerStyle(controllerStyle);
		List<string> controllerStyleJSON = controllerStyle.GetJSON();
		for(int i = 0; i < controllerStyleJSON.Count; i++){
			AddToPacket(jUID, controllerStyleJSON[i]);
		}
	}
	
	//When a controller connects it will check the version so that it can know if the controller is out of date.  If the game is out of date the controller should still work with it (only 1.0.0 and greater)
	private string version = "0.0.0";
	public string GetVersion(){
		return version;
	}
	[RPC] public void CheckVersion(string controllerVersion, int deviceID){
		if(controllerVersion == version){
			Debug.Log ("versions Match");
		}
		else if(int.Parse(version.Split('.')[0])<=int.Parse(controllerVersion.Split('.')[0]) && int.Parse(version.Split('.')[1])<=int.Parse(controllerVersion.Split('.')[1]) && int.Parse(version.Split('.')[2])<=int.Parse(controllerVersion.Split('.')[2])){
			Debug.Log ("controller more advanced version");
		}
		else{
			Debug.Log ("controller out of date");
		}
	}

	void AddMessage(string message) {
		mainThreadInterpret.Add(message);
	}
	
	
	void StartUnity(){
		UnityNetworkConnect();
	}
	void UnityNetworkConnect(){
		Network.InitializeServer(32, unityPort, !Network.HavePublicAddress());
	}
	private WWW wwwData = null;
	private Dictionary<int, NetworkPlayer> networkPlayers = new Dictionary<int, NetworkPlayer>();
	private int networkPlayerCount = 0;
	private const string typeName = "Jovios";
	
	//this is the unity newtorkign connection and disconnection information
	void OnPlayerConnected(NetworkPlayer player){
		string playerJSON;
		playerJSON = "{'packet':{'playerNumber':"+networkPlayerCount.ToString()+"}}";
		networkView.RPC ("SendPacket",player,playerJSON);
		networkPlayers.Add(networkPlayerCount, player);
		networkPlayerCount ++;
	}
	
	//this is the unity newtorkign connection and disconnection information
	void OnPlayerDisconnected(NetworkPlayer player){
		for(int i = 0; i < parser.GetPlayerCount(); i++){
			if(parser.GetPlayer(i).GetNetworkPlayer() == player){
				PlayerDisconnected(parser.GetPlayer(i));
			}
		}
	}
	public void SetNetworkPlayer(int deviceID, int playerNumber){
		if(networkPlayers.Count >= playerNumber && playerNumber >= 0){
			parser.GetPlayer(new JoviosUserID(deviceID)).SetNetworkPlayer(networkPlayers[playerNumber]);
		}
	}
	
	Thread t;
	public void StartUPDListening(){
		udpPort = 24000;
		udpClient = new UdpClient(udpPort);
		udpEndpoint = new IPEndPoint(IPAddress.Broadcast, udpPort);
		t = new Thread(new ThreadStart(UDPListening));
		t.Start();
	}
	
	public void UDPListening(){
		while(true){
			byte[] receivingBytes = udpClient.Receive(ref udpEndpoint);
			mainThreadInterpret.Add( Encoding.ASCII.GetString(receivingBytes));
		}
	}
	
	int webServerPort = 8080;
	NetworkStream sWeb;
	StreamReader srWeb;
	StreamWriter swWeb;		
	
	UdpClient udpClient = new UdpClient();
	IPEndPoint udpEndpoint;
	IPEndPoint udpBroadcastEndpoint;
	
	
	
	private Dictionary<int, List<string>> packetJSON = new Dictionary<int, List<string>>();
	private Dictionary<int, JoviosNetworkingState> networkingStates = new Dictionary<int, JoviosNetworkingState>();
	List<string> mainThreadInterpret = new List<string>();
	string udpThreadInterpret = "";
	//this sends out the packets as they are generated
	void FixedUpdate(){
		if(mainThreadInterpret.Count > 0){
			int packetCount = mainThreadInterpret.Count;
			for(int i = 0; i < packetCount; i++){
				mainThreadInterpret[0] = mainThreadInterpret[0].Replace('\'','"');
				if(mainThreadInterpret[0][0] != '{'){
					mainThreadInterpret[0] = "{" + mainThreadInterpret[0] + "}";
				}
				parser.ParsePacket(mainThreadInterpret[0]);
				mainThreadInterpret.Remove(mainThreadInterpret[0]);
			}
		}
		Send ();
	}
	public void AddToPacket(JoviosUserID jUID, string addition){
		if(packetJSON.ContainsKey(jUID.GetIDNumber())){
			packetJSON[jUID.GetIDNumber()].Add("{'deviceID':"+parser.GetDeviceID()+",'packet':{" + addition + "}}");
		}
		else{
			packetJSON.Add(jUID.GetIDNumber(), new List<string>());
			packetJSON[jUID.GetIDNumber()].Add("{'deviceID':"+parser.GetDeviceID()+",'packet':{" + addition + "}}");
		}
	}
	public string externalIP;
	public void UnityNetworking(){
		externalIP = Network.player.externalIP;
		unityPort = 25007;
		StartUnity();
	}

	public void UDPBroadcast(){
		externalIP = Network.player.externalIP;
		udpPort = 24000;
		udpBroadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, udpPort);
		StartCoroutine("BroadcastPresence");
	}
	
	public int udpPort {get; private set;}
	public int unityPort {get; private set;}
	public IEnumerator BroadcastPresence(){
		if(parser.GetGameCode() != "" && parser.GetGameCode() != null){
			string toSend = "'packet':{'session':'"
				+jovios.gameName+";"
					+Network.player.ipAddress
					+";"+parser.GetGameCode()
					+";"+unityPort
					+";"+jovios.iconURL+"'}";
			byte[] sendBytes = Encoding.ASCII.GetBytes(toSend);
			udpClient.Send(sendBytes, sendBytes.Length, udpBroadcastEndpoint);
		}
		yield return new WaitForSeconds(3);
		StartCoroutine("BroadcastPresence");
	}
	
	//this picks up the Unity Networking connections
	public IJoviosNetworkListen parser;
	[RPC] void SendPacket(string packet){
		packet = packet.Replace("'","\"");
		parser.ParsePacket(packet);
	}

	public void Send(){
		foreach(int key in packetJSON.Keys){
			while(packetJSON[key].Count > 0){
				switch(networkingStates[key]){
				case JoviosNetworkingState.Unity:
					networkView.RPC("SendPacket", parser.GetPlayer(new JoviosUserID(key)).GetNetworkPlayer(), packetJSON[key][0]);
					break;
					
				case JoviosNetworkingState.WebServer:
					SendNodeLinux(packetJSON[key][0], key.ToString());
					break;
					
				default:
					Debug.Log("error in networking states");
					break;
				}
				packetJSON[key].Remove(packetJSON[key][0]);
			}
		}
	}

	[RPC] void SendImage(string imageRects, string imageNames, byte[] imageData, byte[] backgroundImage){
		
	}

	public void PlayerConnected(int deviceID, int playerNumber, string networkType){
		if (!packetJSON.ContainsKey (deviceID)){
		packetJSON.Add(deviceID, new List<string>());
		}
		switch(networkType){
		case "unity":
			SetNetworkPlayer(deviceID, playerNumber);
			if(networkingStates.ContainsKey(deviceID)){
				networkingStates[deviceID] = JoviosNetworkingState.Unity;
			}
			else{
				networkingStates.Add(deviceID, JoviosNetworkingState.Unity);
			}
			break;
		case "udp":
			SetNetworkPlayer(deviceID, playerNumber);
			if(networkingStates.ContainsKey(deviceID)){
				networkingStates[deviceID] = JoviosNetworkingState.Unity;
			}
			else{
				networkingStates.Add(deviceID, JoviosNetworkingState.Unity);
			}
			break;
		case "webServer":
			networkingStates.Add(deviceID, JoviosNetworkingState.WebServer);
			break;
		default:
			Debug.Log ("unknown networking state");
			SetNetworkPlayer(deviceID, playerNumber);
			if(networkingStates.ContainsKey(deviceID)){
				networkingStates[deviceID] = JoviosNetworkingState.Unity;
			}
			else{
				networkingStates.Add(deviceID, JoviosNetworkingState.Unity);
			}
			break;
		}
	}

	public void PlayerDisconnected(JoviosPlayer p){
		if(p != null){
			if(packetJSON.ContainsKey(p.GetUserID().GetIDNumber())){
				packetJSON.Remove(p.GetUserID().GetIDNumber());
			}
			if(networkingStates.ContainsKey(p.GetUserID().GetIDNumber())){
				networkingStates.Remove(p.GetUserID().GetIDNumber());
			}
			parser.OnLeave (p.GetUserID().GetIDNumber().ToString());

		}
	}

	public void Disconnect(){
		packetJSON = new Dictionary<int, List<string>> ();
		networkingStates = new Dictionary<int, JoviosNetworkingState> ();
		ClosePomelo ();
	}

	void ClosePomelo(){
		#if !UNITY_WEBPLAYER
		try{
			pclient.disconnect();
			pclient = null;
		}
		catch(Exception e){
			Debug.Log (e.ToString());
		}
		#endif
	}

	void CloseUDP(){
		try{
			t.Abort();
			udpClient.Close();
		}
		catch(Exception e){
			Debug.Log (e.ToString());
		}
	}

	//this disconnects when the application quits
	public void OnApplicationQuit(){
		Network.Disconnect();
		ClosePomelo ();
		CloseUDP ();
	}

	public Jovios jovios;
#if !UNITY_WEBPLAYER
	public PomeloClient pclient;
#endif
	public bool has_internet = false;
	//Login the chat application and new PomeloClient.
	
#if UNITY_WEBPLAYER
	public void NodeLinux() {
		string url = "54.186.22.19:3014";
		Application.ExternalCall ("nodeLinux", url, parser.GetUserName(), "0");
	}

	public void JoinNodeLinux(string data){
		parser.SetGameCode(data);
		parser.JoinedNodeLinux(data);
	}
	
	public void SendNodeLinux(string content, string target = "*"){
		Application.ExternalCall ("sendNodeLinux", content, target, parser.GetUserName());
	}
	
	public void OnChat(string data){
		AddMessage(data);
	}
	public void OnAdd(string data){
		parser.OnAdd(data);
	}
	public void OnLeave(string data){
		parser.OnLeave(data);
	}


	public void RegisterEmail(string email, string hash){
		Application.ExternalCall ("registerEmail", email, hash);
	}
	
	public void LoginEmail(string email, string hash){
		Application.ExternalCall ("loginEmail", email, hash);
	}

	public void OnUserInfo(string data){
		parser.SetPlayerID(int.Parse(data));
	}

	public void GetDeviceID(){
		Application.ExternalCall ("getDeviceID");
	}
	public void OnDeviceID(int deviceID){
		parser.SetDeviceID (deviceID);
	}

	
	public void SaveGame(string gameName, int gameID, string saveData){

	}
	
	public void LoadGame(string gameID){

	}

	public void SaveGameStat(string gameName, int gameID, string statData, string statDomain = "", string statKingdom = "", string statPhylum = "", string statOrder = "", string statFamily = "", string statClass = "", string statGenus = ""){		

	}

	public void SaveSystemStat(string statData, string statDomain = "", string statKingdom = "", string statPhylum = "", string statOrder = "", string statFamily = "", string statClass = "", string statGenus = ""){

	}

	public void OnData(string data){

	}

	public void Test(){

	}
		
#else
	public void NodeLinux(){
		string url = "54.186.22.19:3014";
		if(!has_internet){
			has_internet = CheckForServerConnection();
			if (!has_internet) {
				Debug.Log ("no internet connection");
				return;
			}
		}
		if(pclient != null){
			Disconnect();
		}
		pclient = new PomeloClient(url);
		pclient.init();
		JsonObject userMessage = new JsonObject();
		userMessage.Add("uid", parser.GetDeviceID());
		pclient.request("gate.gateHandler.queryEntry", userMessage, (data)=>{
			System.Object code = null;
			if(data.TryGetValue("code", out code)){
				if(Convert.ToInt32(code) == 500) {
					return;
				} else {
					pclient.disconnect();
					pclient = null;
					System.Object host, port;
					if (data.TryGetValue("host", out host) && data.TryGetValue("port", out port)) {
						pclient = new PomeloClient("http://" + "54.186.22.19" + ":" + port.ToString());
						pclient.init();
						pclient.On("onAdd", (data3)=> {
							Dictionary<string,object> packet = Json.Deserialize (data3.ToString()) as Dictionary<string,object>;
							Dictionary<string,object> body = (Dictionary<string,object>) packet["body"];
							parser.OnAdd((string) body["user"]);
						});
						pclient.On("onChat", (data2)=> {
							Dictionary<string,object> packet = Json.Deserialize (data2.ToString()) as Dictionary<string,object>;
							Dictionary<string,object> body = (Dictionary<string,object>) packet["body"];
							AddMessage((string) body["msg"]);
						});
						pclient.On("onLeave", (data4)=> {
							Dictionary<string,object> packet = Json.Deserialize (data4.ToString()) as Dictionary<string,object>;
							Dictionary<string,object> body = (Dictionary<string,object>) packet["body"];
							parser.OnLeave((string) body["user"]);
						});
						pclient.On("onError", (data5)=> {
							System.Object nodeLinuxError;
							data5.TryGetValue("body", out nodeLinuxError);
							Debug.Log(nodeLinuxError.ToString());
						});
						if(parser.GetGameCode() == "" || parser.GetGameCode() == null){
							NewGame();
						}
						else{
							JoinNodeLinux();
						}
					}
				} 
			}
		});
	}

	public void NewGame(){
		JsonObject userMessage = new JsonObject();
		userMessage.Add("gameName", "test");
		if (pclient != null) {
			pclient.request("db.dbHandler.newGame", userMessage, (data)=>{
				Dictionary<string,object> packet = Json.Deserialize (data.ToString()) as Dictionary<string,object>;
				Dictionary<string,object> msg = (Dictionary<string,object>) packet["msg"];
				parser.SetGameCode(Json.Serialize(msg["insertId"]));
				JoinNodeLinux();
			});
		}
	}
	
	//Entry chat application.
	public void JoinNodeLinux(){
		JsonObject userMessage = new JsonObject();
		userMessage.Add("username", parser.GetDeviceID());
		userMessage.Add("rid", parser.GetGameCode());
		if (pclient != null) {
			pclient.request("connector.entryHandler.enter", userMessage, (data)=>{
				parser.JoinedNodeLinux(data.ToString());
			});
		}
	}
	
	public void SendNodeLinux(string content, string target = "*"){
		JsonObject message = new JsonObject();
		message.Add("rid", parser.GetGameCode());
		message.Add("content", content);
		message.Add("from", parser.GetDeviceID());
		message.Add("target", target);	
		pclient.request("chat.chatHandler.send", message, (data) => {

		});
	}
	
	public void SaveGame(string gameName, int gameID, string saveData){
		JsonObject message = new JsonObject();
		message.Add("rid", parser.GetGameCode());
		message.Add("gameCode", parser.GetGameCode());
		message.Add("gameID", gameID);
		message.Add("gameName", gameName);
		message.Add("deviceID", 0);
		message.Add("gameState", saveData);
		message.Add("from", "0");
		pclient.request("db.dbHandler.saveGame", message, (data) => {

		});
	}
	
	public void LoadGame(string gameID){
		JsonObject message = new JsonObject();
		message.Add("rid", parser.GetGameCode());
		message.Add("gameID", gameID);
		message.Add("from", "0");
		pclient.request("db.dbHandler.loadGame", message, (data) => {

		});
	}
	
	public void SaveGameStat(string gameName, int gameID, string statData, string statDomain = "", string statKingdom = "", string statPhylum = "", string statOrder = "", string statFamily = "", string statClass = "", string statGenus = ""){		
		if(pclient == null){
			NodeLinux();
		}
		JsonObject message = new JsonObject();
		message.Add("rid", parser.GetGameCode());
		message.Add("gameCode", parser.GetGameCode());
		message.Add("gameID", gameID);
		message.Add("gameName", gameName);
		message.Add("deviceID", parser.GetUserName());
		message.Add("species", statData);
		message.Add("domain", statDomain);
		message.Add("kingdom", statKingdom);
		message.Add("phylum", statPhylum);
		message.Add("order", statOrder);
		message.Add("family", statFamily);
		message.Add("statClass", statClass);
		message.Add("genus", statGenus);
		message.Add("from", "0");
		pclient.request("db.dbHandler.saveGamesStat", message, (data) => {
			
		});
	}
	
	public void LoadGameStat(string gameName, int gameID, string where){		
		if(pclient == null){
			NodeLinux();
		}
		JsonObject message = new JsonObject();
		message.Add("rid", parser.GetGameCode());
		message.Add("gameCode", parser.GetGameCode());
		message.Add("gameID", gameID);
		message.Add("gameName", gameName);
		message.Add("where", where);
		message.Add("from", "0");
		pclient.request("db.dbHandler.loadGamesStat", message, (data) => {
			Debug.Log (data);
		});
	}

	public void SaveSystemStat(string statData, string statDomain = "", string statKingdom = "", string statPhylum = "", string statOrder = "", string statFamily = "", string statClass = "", string statGenus = ""){
		if(pclient == null){
			NodeLinux();
		}
		JsonObject message = new JsonObject();
		message.Add("deviceID", parser.GetUserName());
		message.Add("species", statData);
		message.Add("domain", statDomain);
		message.Add("kingdom", statKingdom);
		message.Add("phylum", statPhylum);
		message.Add("order", statOrder);
		message.Add("family", statFamily);
		message.Add("statClass", statClass);
		message.Add("genus", statGenus);
		pclient.request("db.dbHandler.saveSystemStat", message, (data) => {

		});
	}

	public void RegisterEmail(string email, string pass){
		JsonObject message = new JsonObject ();
		message.Add("email", email);
		message.Add("pass", pass);
		pclient.request("db.dbHandler.emailRegister", message, (data) => {
			Dictionary<string, object> loginJSON = Json.Deserialize(data.ToString()) as Dictionary<string, object>;
			parser.SetPlayerID(int.Parse (Json.Serialize(loginJSON["insertId"])));
		});
	}
	
	public void LoginEmail(string email, string pass){
		JsonObject message = new JsonObject ();
		message.Add("email", email);
		message.Add("pass", pass);
		pclient.request("db.dbHandler.emailLogin", message, (data) => {
			Dictionary<string, object> loginJSON = Json.Deserialize(data.ToString()) as Dictionary<string, object>;
			Dictionary<string, object> msgJSON = (Dictionary<string, object>) ((List<object>)loginJSON["msg"])[0];
			parser.SetPlayerID(int.Parse (Json.Serialize(msgJSON["JoviosID"])));
		});
	}
	
	public void Test(){
		JoviosNetworking networking = GameObject.Find ("JoviosObject").GetComponent<JoviosNetworking> ();
		networking.SaveGameStat("12", 1, "test2","domain","kingdom","phylum","order","family","class","genus");
	}
	
	#endif
}
