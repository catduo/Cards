using UnityEngine;
using System.Collections;

public interface IJoviosPlayerListener {
	bool PlayerConnected(JoviosPlayer p);
	bool PlayerUpdated(JoviosPlayer p);
	bool PlayerDisconnected(JoviosPlayer p);
}
