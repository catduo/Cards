using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour, IJoviosPlayerListener{
	
	public static Jovios jovios;
	public static Dictionary<int, Player> players = new Dictionary<int, Player>();
	public static int gameID;
	public GameObject statusObject;

	void Start(){
		jovios = Jovios.Create();
		jovios.AddPlayerListener(this);
	}

	JoviosNetworking networking;
	string kingdom = ""
		, phylum = ""
			, statClass = ""
			, order = ""
			, family = ""
			, genus = ""
			, species = "";
	bool is_black;
	void OnGUI(){
		GUI.Box(new Rect(0,0,100,50), jovios.gameName);
		if(GUI.Button(new Rect(0,50,100,50), "Start Server")){
			jovios.StartServer();
			networking = GameObject.Find ("JoviosObject").GetComponent<JoviosNetworking>();
		}
		if(GUI.Button(new Rect(0,100,100,50), "find")){
			networking.LoadGameStat("cards", 0, "GameName = 'Cards' AND Domain = 'Cards'");
		}
		is_black = GUI.Toggle (new Rect (100, 0, 100, 50), is_black, "black?");
		if(is_black){
			kingdom = "black";
		}
		else{
			kingdom = "white";
		}
		GUI.Box(new Rect(100,50,100,25), "Category");
		phylum = GUI.TextArea(new Rect(100,75,100,25), phylum);
		
		GUI.Box(new Rect(100,100,100,25), "Sub Category");
		statClass = GUI.TextArea(new Rect(100,125,100,25), statClass);

		GUI.Box(new Rect(100,150,100,25), "Age (int)");
		order = GUI.TextArea(new Rect(100,175,100,25), order);
		//family = GUI.TextArea(new Rect(100,200,100,50), family);
		//genus = GUI.TextArea(new Rect(100,250,100,50), genus);
		if(kingdom == "black"){
			GUI.Box(new Rect(100,300,100,25), "#White Cards");
			species = GUI.TextArea(new Rect(100,325,100,25), species);
		}
		if(GUI.Button(new Rect(100,350,100,50), "set")){
			networking.SaveGameStat("Cards", 0, species, "Cards", kingdom, phylum, order, family, statClass, genus);
		}
	}
	
	bool IJoviosPlayerListener.PlayerConnected(JoviosPlayer p){
		JoviosControllerStyle controllerStyle = new JoviosControllerStyle();
		GameObject newStatusObject = (GameObject) GameObject.Instantiate(statusObject, Vector3.zero, Quaternion.identity);
		statusObject.GetComponent<PlayerStatus>().Setup(players[p.GetUserID().GetIDNumber()]);
		jovios.SetControls(p.GetUserID(), controllerStyle);
		return false;
	}
	bool IJoviosPlayerListener.PlayerUpdated(JoviosPlayer p){
		Debug.Log (p.GetPlayerName());
		return false;
	}
	bool IJoviosPlayerListener.PlayerDisconnected(JoviosPlayer p){
		Debug.Log (p.GetPlayerName());
		return false;
	}
}
