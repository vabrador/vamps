using UnityEngine;
using System.Collections;

public class FloatSensor : MonoBehaviour {

    private float sensorValue = 0f;

    public float Read() {
        return sensorValue;
    }

    public void SetSensorValue(float value) {
        sensorValue = value;
    }

    // Alias for Read()
    public float GetSensorValue() {
        return Read();
    }

}
