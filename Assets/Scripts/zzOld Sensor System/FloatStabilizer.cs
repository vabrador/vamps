using UnityEngine;
using System.Collections;

public class FloatStabilizer : FloatSensor {

    // The resistance of the sensor value to moving.
    public float measurementMass = 1f;
    // The coefficient of the force towards the value being stabilized. The force gets
    // linearly stronger with distance from the non-stabilized value.
    public float measurementDriveCoeff = 1f;
    // The coefficient of the force against current velocity. This force is linear with velocity.
    public float measurementFrictionCoeff = 1f;
    // The coefficient of the drag against current velocity. This force is quadratic with velocity.
    public float measurementDragCoeff = 0.05f;

    private float velocity = 0f;

    // Component references.
    public FloatSensor toStabilize;

    void Start() {
        SetSensorValue(toStabilize.GetSensorValue());
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        float target = toStabilize.Read();
        float distanceToTarget = Read() - target;

        // Calculate and sum all forces
        float forceSum = 0f;
        
        forceSum += distanceToTarget * measurementDriveCoeff;   // driving force
        forceSum += velocity * measurementFrictionCoeff;        // friction force
        forceSum += measurementDragCoeff * velocity * velocity; // drag force

        float acceleration = forceSum / measurementMass; // F = ma

        // Apply the summed force to the stabilizer sensor value.
        float deltaVelocity = acceleration * Time.fixedDeltaTime; // v = at + C

        velocity += deltaVelocity;

        SetSensorValue(Read() + velocity);
    }

}
