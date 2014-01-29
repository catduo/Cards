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
		GetGameID();
	}
	
	void OnGUI(){
		GUI.Box(new Rect(0,0,100,50), jovios.GetGameName());
		if(GUI.Button(new Rect(0,50,100,50), "Start Server")){
			jovios.StartServer();
		}
	}
	
	bool IJoviosPlayerListener.PlayerConnected(JoviosPlayer p){
		JoviosControllerStyle controllerStyle = new JoviosControllerStyle();
		GameObject newStatusObject = (GameObject) GameObject.Instantiate(statusObject, Vector3.zero, Quaternion.identity);
		if(players.ContainsKey(p.GetUserID().GetIDNumber())){
			players[p.GetUserID().GetIDNumber()].statusObject = newStatusObject;
			controllerStyle.SetSingleButtons("Review your cards", players[p.GetUserID().GetIDNumber()].playCards.ToArray(), "Discard");
		}
		else{
			players.Add(p.GetUserID().GetIDNumber(), new Player(p.GetUserID().GetIDNumber(), p.GetPlayerName(), newStatusObject));
			controllerStyle.SetBasicButtons("Loading Cards", new string[0]);
			StartCoroutine(SetInitialCards(players[p.GetUserID().GetIDNumber()]));
		}
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
	
	IEnumerator SetInitialCards(Player p){
		JoviosControllerStyle controllerStyle = new JoviosControllerStyle();
		WWWForm form = new WWWForm();
		form.AddField("uid",p.uid);
		form.AddField ("game",gameID);
		form.AddField ("ip",Network.player.externalIP);
		WWW post_req = new WWW("http://54.201.173.103/cards/get7cards.php",form);
		yield return post_req;
		p.SetCards(post_req.text.Split('~'));
		jovios.SetControls(new JoviosUserID(p.uid), controllerStyle);
	}

	IEnumerator GetGameID(){
		WWWForm form = new WWWForm();
		form.AddField ("ip",Network.player.externalIP);
		WWW post_req = new WWW("http://54.201.173.103/cards/getgameid.php",form);
		yield return post_req;
		int.TryParse(post_req.text, out gameID);
	}
}
