using UnityEngine;
using System.Collections;
using System;
using VRStandardAssets.Utils;

public class Activator : MonoBehaviour {

    public event Action<bool> OnActivation;

    [SerializeField]
    float duration;
    [SerializeField]
    Vector3 deltaPos;
    [SerializeField]
    Vector3 deltaRot;
    [SerializeField]
    Transform playerTransf;

    bool isNear;

    Vector3 startPos, endPos, startRot, endRot;
    bool activated;

    void Awake() {
        startPos = transform.localPosition;
        endPos = startPos + deltaPos;
        startRot = deltaRot;
        endRot = - deltaRot;
        transform.localRotation = Quaternion.Euler(startRot);
        activated = false;
        isNear = false;
    }

    void OnTriggerEnter() {
        isNear = true;
    }

    void OnTriggerExit() {
        isNear = false;
    }

    void OnEnable() {
        playerTransf.GetComponentInChildren<VRInput>().OnClick += OnClick;
    }

    void OnClick() {
        Vector3 relativePos = transform.InverseTransformPoint(playerTransf.position);
        if (isNear) { //&& relativePos.y * transform.localScale.y <= 1.5f && Mathf.Abs(relativePos.x) <= 0.5f ) {
            GenericActivate(!activated);
        }
    }

    void GenericActivate(bool b) {
        StopAllCoroutines();
        StartCoroutine(Activate(b));
    }

    IEnumerator Activate(bool b) {
        activated = b;

        // Disable from the beginning of the coroutine
        if (!b && OnActivation != null) {
            OnActivation(false);
        }

        Vector3 startPos = transform.localPosition;
        Vector3 endPos = !activated ? this.startPos : this.endPos;

        Vector3 startRot = !activated ? this.endRot : this.startRot;
        Vector3 endRot = !activated ? this.startRot : this.endRot;
        
        float timer = 0f;
        while (timer <= duration) {
            transform.localPosition = Vector3.Lerp(startPos, endPos, timer / duration);
            transform.localRotation = Quaternion.Euler(Vector3.Lerp(startRot, endRot, timer / duration));
            timer += Time.deltaTime;
            yield return null;
        }

        // Active true only when the button is in place
        if (b && OnActivation != null)
            OnActivation(true);
    }

    public void ActivateByLink() {
        StopAllCoroutines();
        StartCoroutine(Activate(!activated));
    }

}
