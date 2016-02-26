using UnityEngine;

public class ActionnerLinker : MonoBehaviour {

    Activator _actionner;

    void Awake() {
        _actionner = GetComponentInChildren<Activator>();
    }

    public void Activate(bool b) {
        _actionner.ActivateByLink();
    }
}
