using UnityEngine;
using System.Collections;

public class ApplyForceTest : MonoBehaviour {

    [SerializeField] Vector3 force;

	void FixedUpdate() {
	    if (force != Vector3.zero) {
	        GetComponent<Rigidbody>().AddForce(force);
	        force = Vector3.zero;
	    }
	}
}
