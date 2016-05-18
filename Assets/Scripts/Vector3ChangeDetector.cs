using UnityEngine;
using System.Collections;

public class Vector3ChangeDetector : MonoBehaviour {

    public Vector3Sensor sourceSensor;
    public Vector3Sensor changeSensor;

    private Vector3 valueLastUpdate;

    void Start() {
        valueLastUpdate = sourceSensor.Read();
    }
	
	void FixedUpdate() {
        changeSensor.SetSensorValue(sourceSensor.Read() - valueLastUpdate);
        valueLastUpdate = sourceSensor.Read();

        // Debug.Log("changeSensor " + changeSensor.Read().ToString("F5"));
	}
}
