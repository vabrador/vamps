using UnityEngine;
using System.Collections;
using System;

public class Accelerometer : MonoBehaviour {
    
    private float accelerometerScale = 1f;
    public float cavityRadius = 2f;

    public Material accelerometerBallMaterial;
    public Material cavityMaterial;
    public PhysicMaterial physicMaterial;

    public bool alreadyConstructed = false;

    private Rigidbody innerBody;
    private AccelerometerBall innerBall;

    // The sensor whose value to drive with this accelerometer.
    public Vector3Sensor sensor;

	// Use this for initialization
	void Start () {

        if (!sensor) {
            sensor = this.gameObject.AddComponent<Vector3Sensor>();
        }

        accelerometerScale = this.transform.localScale.x;
        
        // Create the inner body
        GameObject innerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        innerSphere.transform.localScale = Vector3.one * accelerometerScale * 0.5f;
        innerSphere.transform.position = this.transform.position;
        innerSphere.layer = 8;
        innerSphere.GetComponent<Collider>().material = physicMaterial;
        innerSphere.GetComponent<Renderer>().material = accelerometerBallMaterial;
        innerSphere.AddComponent<AccelerometerBall>();
        innerBall = innerSphere.GetComponent<AccelerometerBall>();
        innerBall.SetAccelerometer(this);
        innerBody = innerSphere.GetComponent<Rigidbody>();
        innerBody.useGravity = true;
        innerBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        innerBody.sleepThreshold = 0f;
		innerBody.mass = 0.01f * accelerometerScale * accelerometerScale;
		innerBody.drag = 1f;
		innerBody.angularDrag = 0f;

        if (!alreadyConstructed) {

            // Create sphere of collision boxes
            float localScaleCoefficient = 0.4f;
            int hSegments = 7;
            int vSegments = 16;
            int count = 0;
            for (int i = 0; i < vSegments; i++) {
                float yAngle = i * (360f / (vSegments));
                for (int j = 1; j < hSegments + 1; j++) {
                    float zAngle = j * (360f / ((hSegments + 1) * 2));
                    GameObject collBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    collBox.transform.SetParent(this.transform);
                    collBox.transform.localScale = new Vector3(1f, 0.25f, 1f) * accelerometerScale * localScaleCoefficient;
                    collBox.transform.localPosition = new Vector3(0f, -1f, 0f) * accelerometerScale;
                    collBox.layer = 8;
                    collBox.GetComponent<Collider>().material = physicMaterial;
                    collBox.GetComponent<Renderer>().material = cavityMaterial;
                    collBox.transform.RotateAround(this.transform.position, Vector3.forward, zAngle);
                    collBox.transform.RotateAround(this.transform.position, Vector3.up, yAngle);
                    count++;
                }
            }
            // poles
            GameObject topCollBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topCollBox.transform.SetParent(this.transform);
            topCollBox.transform.localScale = new Vector3(1f, 0.25f, 1f) * accelerometerScale * localScaleCoefficient;
            topCollBox.transform.localPosition = new Vector3(0f, -1f, 0f) * accelerometerScale;
            topCollBox.layer = 8;
            topCollBox.GetComponent<Collider>().material = physicMaterial;
            topCollBox.GetComponent<Renderer>().material = cavityMaterial;
            GameObject botCollBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            botCollBox.transform.SetParent(this.transform);
            botCollBox.transform.localScale = new Vector3(1f, 0.25f, 1f) * accelerometerScale * localScaleCoefficient;
            botCollBox.transform.localPosition = new Vector3(0f, 1f, 0f) * accelerometerScale;
            botCollBox.layer = 8;
            botCollBox.GetComponent<Collider>().material = physicMaterial;
            botCollBox.GetComponent<Renderer>().material = cavityMaterial;
        }
        
    }
	
	// Update is called once per frame
	void FixedUpdate () {
		
		sensor.SetSensorValue(Quaternion.FromToRotation(Vector3.up, this.transform.up) * innerBall.GetTotalImpulseThisFrame());
        Debug.DrawRay(transform.position, sensor.Read() * 1000f, Color.green, 2f, false);
        Debug.Log("Accelerometer reading: " + sensor.Read().ToString("F5"));

    }

    public float WorldRadius {
        get {
            return accelerometerScale * cavityRadius;
        }
    }
}
