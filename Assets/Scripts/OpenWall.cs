using System.Collections;
using UnityEngine;

public class OpenWall : MonoBehaviour {

    [SerializeField] float duration;
    [SerializeField] Vector3 delta;

    Vector3 startPos, endPos;
    bool activated;

    void Awake() {
        startPos = transform.position;
        endPos = startPos + delta;
        activated = false;
    }

    public void GenericActivate(bool b) {
        if (activated == b) return;
        StopAllCoroutines();
        StartCoroutine(Activate(b));
    }

    IEnumerator Activate(bool b) {
        activated = b;

        Vector3 startPos = transform.position;
        Vector3 endPos = !activated ? this.startPos : this.endPos;

        float timer = 0f;
        while (timer <= duration) {
            transform.position = Vector3.Lerp(startPos, endPos, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
