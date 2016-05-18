using UnityEngine;
using System.Collections;

public class Vector3Sensor : MonoBehaviour {

    public string label = "[UNLABELED]";

    private Vector3 sensorValue = Vector3.zero;

    public Vector3 Read() {
        return sensorValue;
    }
    public Vector3 GetSensorValue() {
        return Read();
    }

    public float ReadX() {
        return sensorValue.x;
    }

    public float ReadY() {
        return sensorValue.y;
    }

    public float ReadZ() {
        return sensorValue.z;
    }

    public void SetSensorValue(Vector3 value) {
        sensorValue = value;
    }

}
