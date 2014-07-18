using UnityEngine;
using System.Collections;

public class PlayerStatus : MonoBehaviour, IJoviosControllerListener, IPlayerOwned {
	
	private Jovios jovios;
	
	void Start(){
		jovios = MenuManager.jovios;
	}
	
	bool IJoviosControllerListener.ButtonEventReceived(JoviosButtonEvent e){
		if(e.GetAction() == "release"){
			if(e.GetResponse() == cardOption1 || e.GetResponse() == cardOption2){
				ChooseCard(e.GetResponse());
			}
			else{
				switch(e.GetAction()){
				case "submit":
					break;
				case "discard":
					break;
				case "manage":
					break;
				case "good":
					break;
				case "meh":
					break;
				case "bad":
					break;
				case "winner":
					break;
				default:
					Debug.Log ("error in button event");
					break;
				}
			}
		}
		return false;
	}

	void IPlayerOwned.PlayerDisconnected(){
		Destroy (gameObject);
	}

	public void Setup(Player p){

	}

	public string cardOption1, cardOption2;
	public void DrawTwoBlack(){

	}

	public void DrawTwoWhite(){

	}

	public void ChooseCard(string chosenCard){
		if(chosenCard == cardOption1){

		}
		else{

		}
	}
}
