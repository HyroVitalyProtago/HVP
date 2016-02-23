using UnityEngine;

public class PhysicsLinker : MonoBehaviour {

    [SerializeField] Rigidbody otherRigidbody;
    Rigidbody rigidbody;

    void Awake() {
        rigidbody = GetComponent<Rigidbody>();
    }

    // TODO : When synchronize ? Only when the other is manipulated ?
    // TODO : What synchronize ? Physics or just position ?
    void FixedUpdate() {
        rigidbody.velocity = otherRigidbody.velocity;
        rigidbody.angularVelocity = otherRigidbody.angularVelocity;
    }
}
