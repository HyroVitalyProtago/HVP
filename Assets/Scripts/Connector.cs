using UnityEngine;
using System;
using System.Reflection;

public class Connector : MonoBehaviour {

    [SerializeField]
    Entry[] connexions = null;

    void Awake() {
        foreach (Entry entry in connexions) {
            if (entry.sender == null || entry.receiver == null || string.IsNullOrEmpty(entry.senderEventName) || string.IsNullOrEmpty(entry.receiverCallbackName)) {
                Debug.LogWarning("Invalid entry in a connector...", this);
                continue;
            }

            entry.eventAdd = entry.sender.GetType().GetMethod(EventConductor.EventPrefixAdd + entry.senderEventName, EventConductor.InstancePublic);
            entry.eventRem = entry.sender.GetType().GetMethod(EventConductor.EventPrefixRemove + entry.senderEventName, EventConductor.InstancePublic);
            if (entry.eventAdd == null || entry.eventRem == null) {
                throw new EventConductor.EventNotFoundException();
            }

            MethodInfo method = entry.receiver.GetType().GetMethod(entry.receiverCallbackName, EventConductor.InstancePublic);
            if (method == null) {
                throw new EventConductor.CallbackNotFoundException();
            }

            try {
                entry.callback = Delegate.CreateDelegate(EventConductor.DelegateType(method), entry.receiver, entry.receiverCallbackName);
                entry.enabled = true;
            } catch (ArgumentException ae) {
                throw new EventConductor.CallbackBadTypeException();
            } // MethodAccessException
        }
    }

    void OnEnable() {
        foreach (Entry entry in connexions) {
            if (entry.enabled) Connect(entry, true);
        }
    }

    void OnDisable() {
        foreach (Entry entry in connexions) {
            if (entry.enabled) Connect(entry, false);
        }
    }

    void Connect(Entry entry, bool value) {
        try {
            (value ? entry.eventAdd : entry.eventRem).Invoke(entry.sender, new object[] { entry.callback });
        } catch (ArgumentException) {
            throw new EventConductor.EventNotMatchCallbackException();
        }
    }

    [Serializable]
    class Entry {
        public Component sender, receiver;
        public string senderEventName, receiverCallbackName;
        public bool enabled; // not used in editor
        public Entry() { }
        [NonSerialized]
        public MethodInfo eventAdd, eventRem; // not used in editor
        [NonSerialized]
        public Delegate callback; // not used in editor
    }
}
