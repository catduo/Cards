using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;

public class JoviosButtonEvent {
	public JoviosButtonEvent(string buttonIn, JoviosControllerStyle controllerStyle, string actionIn = "", string typeIn = ""){
		response = buttonIn;
		style = controllerStyle;
		action = actionIn;
		type = typeIn;
	}
	//the side is right or left, but will eventually be supporting arbitrary deffinitions and will change to locations instead of side.
	private string type;
	public string GetButtonType(){
		return type;
	}
	//the response is the string defined by the game as what the game should be listening for.  this is not the description, but should be associated with it
	private string response;
	public string GetResponse(){
		return response;
	}
	// the full controller style is included with the button press.  This can be out of sync if the controller is changing rapidly
	private JoviosControllerStyle style;
	public JoviosControllerStyle GetControllerStyle(){
		return style;
	}
	// primary actions are press and release, but other actions may occur
	private string action;
	public string GetAction(){
		return action;
	}
}
