using UnityEngine;
using System.Collections;

public class PlayerStatus : MonoBehaviour, IJoviosControllerListener {
	
	private Jovios jovios;
	
	void Start(){
		jovios = MenuManager.jovios;
	}
	
	void Update(){
		if(jovios.GetPlayer(0) != null){
			if(jovios.GetPlayer(0).GetInput("accelerometer") != null){
				//transform.eulerAngles = new Vector3(0,0,jovios.GetPlayer(0).GetInput("accelerometer").GetGyro().eulerAngles.z);
				transform.localRotation = jovios.GetPlayer(0).GetInput("accelerometer").GetGyro();
				transform.localPosition = jovios.GetPlayer(0).GetInput("accelerometer").GetAcceleration();
			}
		}
	}
	
	bool IJoviosControllerListener.ButtonEventReceived(JoviosButtonEvent e){
		Debug.Log (e.GetResponse() + e.GetAction());
		//Debug.Log (e.GetControllerStyle().GetQuestionPrompt());
		return false;
	}

	public void Setup(Player p){

	}
	
	IEnumerator GetTwoWhiteCards(Player p){
		JoviosControllerStyle controllerStyle = new JoviosControllerStyle();
		WWWForm form = new WWWForm();
		form.AddField("uid",p.uid);
		form.AddField ("game",MenuManager.gameID);
		WWW post_req = new WWW("http://54.201.173.103/cards/get7cards.php",form);
		yield return post_req;
		controllerStyle.SetSingleButtons("Choose one of these two cards to add to your hand", post_req.text.Split('~'), "Choose");
		jovios.SetControls(new JoviosUserID(p.uid), controllerStyle);
	}
	
	IEnumerator GetTwoBlackCards(Player p){
		JoviosControllerStyle controllerStyle = new JoviosControllerStyle();
		WWWForm form = new WWWForm();
		form.AddField("uid",p.uid);
		form.AddField ("game",MenuManager.gameID);
		WWW post_req = new WWW("http://54.201.173.103/cards/get7cards.php",form);
		yield return post_req;
		controllerStyle.SetSingleButtons("Choose one of these two cards to play as the card for the round", post_req.text.Split('~'), "Choose");
		jovios.SetControls(new JoviosUserID(p.uid), controllerStyle);
	}
}
