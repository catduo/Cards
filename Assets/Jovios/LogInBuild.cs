using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LogInBuild : MonoBehaviour {

	List<string> logList = new List<string>();
	bool is_logList = false;

	// Use this for initialization
	void Start () {
		Application.RegisterLogCallback(LogGUI);
	}
	
	public void LogGUI(string logString, string stackTrace, LogType type){
		logList.Add(logString);
	}

	public void OnGUI(){
		if(is_logList){
			string logging = "";
			int listLength = Mathf.Min (20, logList.Count);
			for(int i = 0; i < listLength; i++){
				logging += "\n" + logList[logList.Count - listLength + i];
			}
			GUI.Box(new Rect(Screen.width - 500,0,500, Screen.height), logging);
			if(GUI.Button(new Rect(Screen.width - 500,Screen.height - 100,500, 100), "disable debug")){
				is_logList = false;
			}
			if(GUI.Button(new Rect(Screen.width - 500,Screen.height - 200,500, 100), "Test")){
				GameObject.Find ("JoviosObject").GetComponent<JoviosNetworking>().Test();
			}
		}
	}

	public void ManagedUpdate(){
		if(Input.touchCount > 3 && !is_logList){
			is_logList = true;
		}
	}
}
