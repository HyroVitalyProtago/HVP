using UnityEngine;
using System.Collections;
using System;
using UnityEditor.iOS.Xcode;
using VRStandardAssets.Utils;

public class PressButton : MonoBehaviour {

    public event Action<bool> OnActivation;

    [SerializeField]
    float duration;
    [SerializeField]
    Vector3 deltaPos;
    [SerializeField]
    Vector3 deltaRot;
    [SerializeField]
    Transform playerTransf;

    Vector3 startPos, endPos, startRot, endRot;
    bool activated;

    void Awake() {
        startPos = transform.position;
        endPos = startPos + deltaPos;
        startRot = transform.rotation.eulerAngles;
        endRot = startRot + deltaRot;
        activated = false;
    }

    void OnEnable() {
        playerTransf.GetComponentInChildren<VRInput>().OnClick += OnClick;
    }

    void OnClick() {
        Vector3 relativePos = transform.InverseTransformPoint(playerTransf.position);
        Debug.Log(Vector3.Distance(transform.position, playerTransf.position), this);
        if (Vector3.Distance(transform.position,playerTransf.position) < 2f && relativePos.y * transform.localScale.y > 0f) { //&& relativePos.y * transform.localScale.y <= 1.5f && Mathf.Abs(relativePos.x) <= 0.5f ) {
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

        Vector3 startPos = transform.position;
        Vector3 endPos = !activated ? this.startPos : this.endPos;

        Vector3 startRot = transform.rotation.eulerAngles;
        Vector3 endRot = !activated ? this.startRot : this.endRot;

        float timer = 0f;
        while (timer <= duration) {
            transform.position = Vector3.Lerp(startPos, endPos, timer / duration);
            transform.rotation = Quaternion.Euler(Vector3.Lerp(startRot, endRot, timer / duration));
            timer += Time.deltaTime;
            yield return null;
        }

        // Active true only when the button is in place
        if (b && OnActivation != null)
            OnActivation(true);
    }
}
