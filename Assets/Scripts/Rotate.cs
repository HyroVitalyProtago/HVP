using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

    [SerializeField]
    float duration;
    [SerializeField]
    Vector3 startRot, endRot;
    bool activated;

    void Awake() {
        transform.localRotation = Quaternion.Euler(startRot);
    }

    public void GenericActivate(bool b) {
        StopAllCoroutines();
        StartCoroutine(Activate(!activated));
    }

    IEnumerator Activate(bool b) {
        activated = b;

        Vector3 startRot = !activated ? this.endRot : this.startRot;
        Vector3 endRot = !activated ? this.startRot : this.endRot;

        float timer = 0f;
        while (timer <= duration) {
            transform.localRotation = Quaternion.Euler(Vector3.Lerp(startRot, endRot, timer / duration));
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
