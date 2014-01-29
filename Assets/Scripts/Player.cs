using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player {
	public string name;
	public int uid;
	public int score;
	public List<string> playCards;
	public string[] wonCards;
	public bool is_leader;
	public bool is_inRound;
	public GameObject statusObject;
	public Player(int jUID, string jname, GameObject newStatusObject){
		name = jname;
		uid = jUID;
		is_leader = false;
		is_inRound = false;
		wonCards = new string[0];
		playCards = new List<string>();
		score = 0;
		statusObject = newStatusObject;
	}
	public void SetCards(string[] newCards){
		playCards = new List<string>();
		for(int i = 0; i < newCards.Length; i++){
			playCards.Add(newCards[i]);
		}
	}
}
