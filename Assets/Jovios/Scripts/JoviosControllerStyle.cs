using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;

public class JoviosControllerStyle{
	public JoviosControllerStyle(){
		controlsCount = 1;
		AddToJSON("{'resetControls':1}");
	}
	//the following will add areas, they can only take right or left, but should be updated to take any arbitrary location information
	public void AddJoystick(Vector2 position, Vector2 scale, string anchor, string response, string joystickBackground = "", string joystickBackdrop = "", string joystickArrow = "", int depth = 0){
		controlsCount++;
		directions.Add(response, new JoviosDirection(position, scale, anchor, response, setDepth: depth, joystickArrow: joystickArrow, joystickBackdrop: joystickBackdrop, joystickBackground: joystickBackground));
		AddToJSON(directions[response].GetJSON());
	}
	public void AddButton1(Vector2 position, Vector2 scale, string anchor, string description, string response = "", string color = "", int depth = 0, string image = "", int parent = -1){
		controlsCount++;
		if(response == ""){
			response = description;
		}
		buttons.Add(response, new JoviosButton(position, scale, anchor, "button1", new string[1] {description}, new string[1] {response}, setColor: color, setDepth: depth, image: image, parent: parent));
		AddToJSON(buttons[response].GetJSON());
	}
	public void AddScrollView(Vector2 position, Vector2 scale, string anchor, string description, string response = ""){
		controlsCount++;
		AddToJSON("{'type':'scrollView','position':["+position.x+","+position.y+","+scale.x+","+scale.y+"], 'anchor':'"+anchor+"','content':'"+description+"','response':'"+response+"'}");
	}
	public void AddGrid(Vector2 position, Vector2 scale, string anchor, string description, string response = "", int cellWidth = 200, int cellHeight = 200, int parent = -1){
		controlsCount++;
		AddToJSON("{'type':'grid','position':["+position.x+","+position.y+","+scale.x+","+scale.y+"], 'anchor':'"+anchor+"','content':'"+description+"','response':'"+response+"','cellWidth':"+cellWidth+",'cellHeight':"+cellHeight+",'parent':"+parent+"}");
	}
	
	//this is the accelerometer information.  it is currently either on or off, but should have intermediate states added in.
	public void SetAccelerometerStyle(JoviosAccelerometerStyle setAccelerometerStyle){
		controlsCount++;
		accelerometer = new JoviosAccelerometer(setAccelerometerStyle);
		AddToJSON(accelerometer.JSON);
	}
	public void AddLabel(Vector2 position, Vector2 scale, string anchor, string description, string color = "", int depth = 0, int fontSize = 0, int parent = -1){
		controlsCount++;
		AddToJSON("{'type':'label','position':["+position.x+","+position.y+","+scale.x+","+scale.y+"], 'anchor':'"+anchor+"','content':'"+description+"','color':'"+color+"','depth':"+depth+",'fontSize':"+fontSize+",'parent':"+parent+"}");
	}
	public void AddImage(Vector2 position, Vector2 scale, string anchor, string imageNameOrUrl, string color = "", int depth = 0, int parent = -1){
		controlsCount++;
		AddToJSON("{'type':'image','position':["+position.x+","+position.y+","+scale.x+","+scale.y+"], 'anchor':'"+anchor+"','content':'"+imageNameOrUrl+"','color':'"+color+"','depth':"+depth+",'parent':"+parent+"}");
	}
	public void AddAvatar(Vector2 position, Vector2 scale, string anchor, int depth = 0){
		controlsCount++;
		AddToJSON("{'type':'avatar','position':["+position.x+","+position.y+","+scale.x+","+scale.y+"], 'anchor':'"+anchor+"','depth':"+depth+"}");
	}

	
	//these are the currently used inputs for the controller style
	private Dictionary<string, JoviosDirection> directions = new Dictionary<string, JoviosDirection>();
	private Dictionary<string, JoviosButton> buttons = new Dictionary<string, JoviosButton>();
	private JoviosAccelerometer accelerometer;
	private int controlsCount = 0;
	public int GetControlsCount(){
		return controlsCount;
	}
	public JoviosDirection GetDirection(string response){
		if(directions.ContainsKey(response)){
			return directions[response];
		}
		else{
			return null;
		}
	}
	public JoviosButton
	GetButton(string response){
		if(buttons.ContainsKey(response)){
			return buttons[response];
		}
		else{
			return null;
		}
	}
	public JoviosAccelerometer GetAccelerometer(){
		return accelerometer;
	}

	
	private List<string> JSON = new List<string>();
	private string thisJSON = "";
	public List<string> GetJSON(){
		if(thisJSON != ""){
			thisJSON += "]";
			JSON.Add(thisJSON);
		}
		return JSON;
		JSON = new List<string>();
	}
	private void AddToJSON(string toAdd){
		if(thisJSON.Length < 3000){
			if(thisJSON == "" || thisJSON == null){
				thisJSON += "'controlStyle':["+toAdd;
			}
			else{
				thisJSON += ","+toAdd+"";
			}
		}
		else{
			thisJSON += "]";
			JSON.Add(thisJSON);
			thisJSON = "";
			AddToJSON(toAdd);
		}
	}
}