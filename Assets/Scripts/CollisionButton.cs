using System;
using System.Collections;
using UnityEngine;

public class CollisionButton : MonoBehaviour {

    public event Action<bool> OnActivation;

    [SerializeField] float duration;
    [SerializeField] Vector3 delta;

    Vector3 startPos, endPos;
    bool activated;

    void Awake() {
        startPos = transform.position;
        endPos = startPos + delta;
        activated = false;
    }

	void OnCollisionEnter(Collision other) {
        GenericActivate(true);
	}

    void OnCollisionExit(Collision other) {
        GenericActivate(false);
    }

    void GenericActivate(bool b) {
        if (activated == b) return;
        StopAllCoroutines();
        StartCoroutine(Activate(b));
    }

    IEnumerator Activate(bool b) {
        activated = b;

        // Disable from the beginning of the coroutine
        if (!b && OnActivation != null) {
            OnActivation(false);
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = !activated ? this.startPos : this.endPos;

        float timer = 0f;
        while (timer <= duration) {
            transform.position = Vector3.Lerp(startPos, endPos, timer/duration);
            timer += Time.deltaTime;
            yield return null;
        }

        // Active true only when the button is in place
        if (b && OnActivation != null)
            OnActivation(true);
    }
}
