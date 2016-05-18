using UnityEngine;
using System.Collections;

public class Vector3Stabilizer : MonoBehaviour {

    // Stabilizes the three axis values of a Vector3 independently.
    
    // The resistance of the sensor value to moving.
    public float measurementMass = 1f;
    // The coefficient of the force towards the value being stabilized. The force gets
    // linearly stronger with distance from the non-stabilized value.
    public float measurementDriveCoeff = 1f;
    // The coefficient of the force against current velocity. This force is linear with velocity.
    public float measurementFrictionCoeff = 1f;
    // The coefficient of the drag against current velocity. This force is quadratic with velocity.
    public float measurementDragCoeff = 1f;

    private Vector3 velocity;

    // Component references.
    public Vector3Sensor sensorToStabilize;
    public Vector3Sensor stableSensor;

    void Start() {
        stableSensor.SetSensorValue(sensorToStabilize.GetSensorValue());
    }

    // Update is called once per frame
    void FixedUpdate() {
        Vector3 target = sensorToStabilize.Read();
        Vector3 distanceToTarget = target - stableSensor.Read();

        // Calculate and sum all forces
        Vector3 forceSum = Vector3.zero;

        forceSum += distanceToTarget * measurementDriveCoeff;   // driving force
        forceSum += -1f * velocity * measurementFrictionCoeff;  // friction force

        // Drag force calculations (independent components)
        {
            float dragX = measurementDragCoeff * velocity.x * velocity.x * (velocity.x > 0 ? -1f : 1f);
            if (dragX == float.NaN) {
                dragX = 0f;
            }
            float dragY = measurementDragCoeff * velocity.y * velocity.y * (velocity.y > 0 ? -1f : 1f);
            if (dragY == float.NaN) {
                dragY = 0f;
            }
            float dragZ = measurementDragCoeff * velocity.z * velocity.z * (velocity.z > 0 ? -1f : 1f);
            if (dragZ == float.NaN) {
                dragZ = 0f;
            }
            forceSum += new Vector3(dragX, dragY, dragZ);
        }

        Vector3 acceleration = forceSum / measurementMass; // F = ma

        // Apply the summed force to the stabilizer sensor value.
        Vector3 deltaVelocity = acceleration * Time.fixedDeltaTime; // v = at + C

        velocity += deltaVelocity;

        stableSensor.SetSensorValue(stableSensor.Read() + velocity);
    }

}
