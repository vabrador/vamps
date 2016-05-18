using UnityEngine;
using System.Collections;

public class Vector3Visualizer : MonoBehaviour {

	public Transform pointer;
	public float scale = 1f;

	public Vector3Sensor vector3Sensor;

	private GameObject pointerObject;

	void Start() {
		pointerObject = transform.FindChild ("Pointer").gameObject;
	}

	public Vector3 GetPointerDirection() {
		return pointer.up;
	}

	public void SetPointer(Vector3 newPointer) {
		if (newPointer.normalized == Vector3.zero) {
			pointerObject.SetActive (false);
		} else {
			pointerObject.SetActive (true);
			pointer.up = newPointer.normalized;
			pointer.transform.localScale = new Vector3(pointer.localScale.x, newPointer.magnitude * scale, pointer.transform.localScale.z);
		}
	}

	void FixedUpdate() {
		SetPointer (vector3Sensor.Read());
	}

}
