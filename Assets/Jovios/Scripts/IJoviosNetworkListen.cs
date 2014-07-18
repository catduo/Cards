using UnityEngine;
using System.Collections;

public interface IJoviosNetworkListen {
	JoviosPlayer GetPlayer(int i);
	JoviosPlayer GetPlayer(JoviosUserID jUID);
	int GetPlayerCount();
	void ParsePacket(string packet);
	void JoinedNodeLinux(string packet);
	void OnLeave(string packet);
	void OnAdd(string packet);
	string GetUserName();
	string GetGameCode();
	string GetDeviceID();
	void Login(string loginInfo);
	void SetDeviceID(int deviceID);
	void SetPlayerID(int playerID);
	void SetGameCode(string gameID);
}
