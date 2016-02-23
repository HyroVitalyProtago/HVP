using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CombinationHandler : MonoBehaviour {

    public event Action OnSuccess, OnFail;

    [SerializeField] string combination; // the wanted combination
    [SerializeField] Text[] texts; // warning, the ordered set of texts

    public void OnCombinationChange() {
        if (texts.Select(t => t.text).Aggregate((i, j) => i + j) == combination) {
            if (OnSuccess != null) OnSuccess();
        } else {
            if (OnFail != null) OnFail();
        }
    }
}
