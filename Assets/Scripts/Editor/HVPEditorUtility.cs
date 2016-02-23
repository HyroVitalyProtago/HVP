using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public static class HVPEditorUtility {

	static readonly Dictionary<Color, Texture2D> TexturesFromColor = new Dictionary<Color, Texture2D>();
	public static Texture2D GetTexture2DFromColor(Color c) {
		Texture2D tex;
		if (TexturesFromColor.TryGetValue(c, out tex))
			return tex;

		tex = new Texture2D(1, 1);
		tex.hideFlags = HideFlags.HideAndDontSave;
		tex.SetPixel(0, 0, c);
		tex.Apply();

		TexturesFromColor[c] = tex;
		return tex;
	}

    static readonly string[] MSpecialNames = { "Missing " };
    public static void Popup(SerializedProperty property, ref string[] options) {
        int index = EditorGUILayout.Popup(IndexOrMissing(ref options, property.stringValue), options);
        if (index == 0) { // default value => no value
            property.stringValue = null;
        } else if (!IsSpecialName(options[index])) {
            property.stringValue = options[index];
        }
    }

    static bool IsSpecialName(string name) {
        return MSpecialNames.Any(name.StartsWith);
    }

    static int IndexOrMissing(ref string[] options, string value) {
        // If there is no value, return the default value of the popup (index 0)
        if (string.IsNullOrEmpty(value)) {
            return 0;
        }
        
        // Return the index of value if it is found
        int index = UnityEditor.ArrayUtility.IndexOf(options, value);
        if (index != -1) {
            return index;
        }

        // If not found, display ("Missing " + value)
        index = UnityEditor.ArrayUtility.IndexOf(options, "Missing " + value);
        if (index != -1) {
            return index;
        }

        // If ("Missing " + value) not found, add it 
        UnityEditor.ArrayUtility.Add(ref options, "Missing " + value);
        return options.Length - 1;
    }
}
