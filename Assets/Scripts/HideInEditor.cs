using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class HideInEditor : MonoBehaviour {
    void OnEnable() {
        GetComponentsInChildren<Renderer>().ToList().ForEach(r => r.enabled = !Application.isEditor || Application.isPlaying);
    }
    void OnDisable() {
        GetComponentsInChildren<Renderer>().ToList().ForEach(r => r.enabled = true);
    }
}