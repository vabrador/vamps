using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

	// The desired torque vector.
	public Vector3 torque = Vector3.up;

	// Multiplier for the torque vector.
	public float torqueStrength = 1f;

	// The duration of the ramp-up and ramp-down phases.
	public float rampTime = 2f;

	// The duration of waiting between ramp-up and ramp-down.
	public float timeBetweenRamps = 2f;

	// Check this during play to start the rotation.
	public bool shouldStartRotating = false;

	// Component references.
	public Rigidbody toRotate;

	// State variables.
	private bool rampingUp = false;
	private bool waitingForRampDown = false;
	private bool rampingDown = false;
	private float rampTimer = 0f;

	// Use this for initialization
	void Start () {
		if (!toRotate) {
			toRotate = GetComponent<Rigidbody> ();
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (shouldStartRotating) {
			shouldStartRotating = false;
			rampingUp = true;
		}

		if (rampingUp) {
			// Apply torque.
			toRotate.AddTorque(torque * torqueStrength, ForceMode.Impulse);
			// Tick the timer.
			rampTimer += Time.fixedDeltaTime;
			if (rampTimer > rampTime) {
				rampTimer = 0f;
				rampingUp = false;
				waitingForRampDown = true;
			}
		} else if (waitingForRampDown) {
			// Tick the timer.
			rampTimer += Time.fixedDeltaTime;
			if (rampTimer > timeBetweenRamps) {
				rampTimer = 0f;
				waitingForRampDown = false;
				rampingDown = true;
			}
		} else if (rampingDown) {
			// Apply reverse torque.
			toRotate.AddTorque(torque * torqueStrength * -1, ForceMode.Impulse);
			// Tick the timer.
			rampTimer += Time.fixedDeltaTime;
			if (rampTimer > rampTime) {
				rampTimer = 0f;
				rampingDown = false;
				Debug.Log ("[Rotator] Finished rotating.");
			}
		}
	
	}
}
