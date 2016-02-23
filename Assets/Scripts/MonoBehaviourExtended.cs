using UnityEngine;
using System;

public abstract class MonoBehaviourExtended : MonoBehaviour {

	[SerializeField, HideInInspector]
	string[] events;

	[SerializeField, HideInInspector]
	Entry[] listeners;

	void Awake() {
		foreach (string e in events) {

            // Warning null or empty event
		    if (string.IsNullOrEmpty(e)) {
                Debug.LogWarning("Invalid event in communication system.", this);
                continue;
		    }

			EventConductor.Offer(this, e);
		}

		foreach (Entry e in listeners) {

            // Warning null or empty (event || callback)
            if (string.IsNullOrEmpty(e.eventName) || string.IsNullOrEmpty(e.callbackName)) {
                Debug.LogWarning("Invalid entry of listener in communication system.", this);
                continue;
            }

            EventConductor.On(this, e.eventName, e.callbackName);
		}
	}

	[Serializable]
	public class Entry {
		public string eventName;
		public string callbackName;
		public Entry() {}
	}

}
