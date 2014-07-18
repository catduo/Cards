using System;
using UnityEngine;

public class JoviosButton{
	//these are the setters for the overall styles
	public JoviosButton(Vector2 setPosition, Vector2 setScale, string setAnchor, string setType, string[] setContent, string[] setResponseKeys, string setPrompt = "", string setSubmit = "", string setCancel = "", string setColor = "", int setDepth = 0, string image = "", int parent = -1){
		type = setType;
		anchor = setAnchor;
		submit = setSubmit;
		prompt = setPrompt;
		cancel = setCancel;
		position = setPosition;
		scale = setScale;
		switch(type){
		case "button1":
			content = new string[] {""};
			responseKeys = new string[] {""};
			if(setContent.Length > 0){
				content[0] = setContent[0];
			}
			if(setResponseKeys.Length > 0){ 
				responseKeys [0] = setResponseKeys[0];
			}
			else if(setContent.Length > 0){
				responseKeys[0] = setContent[0];
			}
			break;
		case "button2":
			content = new string[] {"",""};
			responseKeys = new string[] {"",""};
			for(int i = 0; i < content.Length; i++){
				if(setContent.Length > i){ 
					content [i] = setContent[i];
				}
				if(setResponseKeys.Length > i){ 
					responseKeys [i] = setResponseKeys[i];
				}
				else if(setContent.Length > i){
					responseKeys[i] = setContent[i];
				}
			}
			break;
		case "button4":
			content = new string[] {"","","",""};
			responseKeys = new string[] {"","","",""};
			for(int i = 0; i < content.Length; i++){
				if(setContent.Length > i){ 
					content [i] = setContent[i];
				}
				if(setResponseKeys.Length > i){ 
					responseKeys [i] = setResponseKeys[i];
				}
				else if(setContent.Length > i){
					responseKeys[i] = setContent[i];
				}
			}
			break;


		default:
			break;
		}
		string rsp = "'response':['";
		for(int i = 0; i < responseKeys.Length; i++){
			rsp += responseKeys[i];
			if(i != responseKeys.Length-1){
				rsp += "','";
			}
			else{
				rsp += "']";
			}
		}
		string cnt = "'content':['";
		for(int i = 0; i < content.Length; i++){
			cnt += content[i];
			if(i != content.Length-1){
				cnt += "','";
			}
			else{
				cnt += "']";
			}
		}
		JSON = "{'type':'"+type+"','position':["+position.x+","+position.y+","+scale.x+","+scale.y+"], 'anchor':'"+anchor+"',"+cnt+","+rsp+",'image':'"+image+"','parent':"+parent+"}";
	}
	public bool is_pressed = false;
	private Vector2 position;
	private Vector2 scale;
	private string anchor;
	private string prompt;
	private string submit;
	private string cancel;
	private string type;
	private string[] content;
	private string[] responseKeys;
	private int parent;
	private string JSON;
	public string GetJSON(){
		return JSON;
	}
}