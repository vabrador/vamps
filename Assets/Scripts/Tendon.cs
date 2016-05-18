using UnityEngine;
using System.Collections;

public class Tendon : MonoBehaviour {

    public new Rigidbody rigidbody;

    void Start() {
        if (!rigidbody) {
            rigidbody = GetComponentInParent<Rigidbody>();
            if (!rigidbody) {
                Debug.LogWarning("[Tendon] No rigidbody attached to Tendon; No Muscle will be able to use it.");
            }
        }
        rigidbody.sleepThreshold = 0f;
    }

}
