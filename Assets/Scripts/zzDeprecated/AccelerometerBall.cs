using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class AccelerometerBall : MonoBehaviour {

    private Accelerometer accelerometer;
    private Rigidbody body;

    private Vector3 totalImpulseThisFrame;

    void Start() {
        body = GetComponent<Rigidbody>();
    }

    public void SetAccelerometer(Accelerometer accelerometer) {
        this.accelerometer = accelerometer;
    }

    void FixedUpdate() {
        if (accelerometer) {
            if ((transform.position - accelerometer.transform.position).sqrMagnitude > accelerometer.WorldRadius * accelerometer.WorldRadius) {
                Debug.LogWarning("Accelerometer Ball outside of Accelerometer! Resetting.");
                body.velocity = Vector3.zero;
                transform.position = accelerometer.transform.position;
            }
        }
        totalImpulseThisFrame = Vector3.zero;
    }

    void OnCollisionEnter(Collision collision) {
        Vector3 detectedImpulse = -collision.impulse;
        totalImpulseThisFrame += detectedImpulse;
    }

    void OnCollisionStay(Collision collision) {
        Vector3 detectedImpulse = -collision.impulse;
        totalImpulseThisFrame += detectedImpulse;
    }

    public Vector3 GetTotalImpulseThisFrame() {
        return totalImpulseThisFrame;
    }

}
