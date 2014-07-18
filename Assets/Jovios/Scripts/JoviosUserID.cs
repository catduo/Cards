using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;

public class JoviosUserID{
	private int userID;
	public int GetIDNumber(){
		return userID;
	}
	public JoviosUserID(int uid){
		userID = uid;
	}
}

