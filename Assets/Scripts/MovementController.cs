using UnityEngine;

// KC688V6-0BAB
public class MovementController : MonoBehaviour {
    [SerializeField] float m_speed;
    Transform m_Cam; // transform of main camera.
    
    void Start() {
        if (Camera.main != null) {
            m_Cam = Camera.main.transform;
        } else {
            Debug.LogWarning("Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
        }
    }
    
    void FixedUpdate() {
        Vector3 move = Input.GetAxis("Vertical") * Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized + Input.GetAxis("Horizontal") * m_Cam.right;
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move); // get the move direction in the correct space.

        transform.Translate(move * m_speed);
    }
}
