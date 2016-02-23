using System;
using UnityEngine;
using VRStandardAssets.Utils;
using UnityEngine.UI;

[RequireComponent(typeof(VRInteractiveItem))]
public class NumberButton : MonoBehaviour {

    public event Action OnValueChange;

    [SerializeField] Color overColor;
    [SerializeField] VRInteractiveItem interactiveItem;

    Text text;
    Color defaultTextColor;

    void Start() {
        text = GetComponentInChildren<Text>();
        defaultTextColor = text.color;
    }

    void OnEnable() {
        interactiveItem.OnOver += OnOver;
        interactiveItem.OnOut += OnOut;
        interactiveItem.OnClick += OnClick;
    }

    void OnDisable() {
        interactiveItem.OnOver -= OnOver;
        interactiveItem.OnOut -= OnOut;
        interactiveItem.OnClick -= OnClick;
    }

    void OnOver() {
        text.color = overColor;
    }

    void OnOut() {
        text.color = defaultTextColor;
    }

    void OnClick() {
        text.text = ((int.Parse(text.text) + 1) % 10).ToString();
        if (OnValueChange != null) OnValueChange();
    }
}
