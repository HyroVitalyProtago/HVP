using UnityEngine;

// Sets the RenderQueue of an object's materials on Awake. This will instance
// the materials, so the script won't interfere with other renderers that
// reference the same materials.
[AddComponentMenu("Rendering/SetRenderQueue")]
public class SetRenderQueue : MonoBehaviour {

    [SerializeField]
    int[] m_queues = { 3000 };

    void Awake() {
        Material[] materials = GetComponent<Renderer>().materials;
        for (int i = 0; i < materials.Length && i < m_queues.Length; ++i) {
            materials[i].renderQueue = m_queues[i];
        }
    }
}