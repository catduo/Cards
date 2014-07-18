using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;

public class JoviosDirection{
	private string description;
	private string response;
	private Vector2 position;
	private Vector2 scale;
	private string anchor;
	private string simplePosition;
	private string JSON;
	public string GetJSON(){
		return JSON;
	}
	public JoviosDirection(Vector2 setPostion, Vector2 setScale, string setAnchor, string setResponse, int setDepth = 0, string joystickBackground = "", string joystickBackdrop = "", string joystickArrow = ""){
		response = setResponse;
		direction = Vector2.zero;
		position = setPostion;
		scale = setScale;
		anchor = setAnchor;
		JSON = "{'type':'joystick','position':["+position.x+","+position.y+","+scale.x+","+scale.y+"], 'anchor':'"+anchor+"','response':['"+response+"'],'images':['"+joystickArrow+"','"+joystickBackdrop+"','"+joystickBackground+"']}";
	}

	//this is for any directional inputs, will eventually support arbitrary definitions
	private Vector2 direction;
	public Vector2 GetDirection(){
		return direction;
	}
	public void SetDirection(Vector2 newDirection){
		direction = newDirection;
	}
	public bool is_pressed = false;
}

