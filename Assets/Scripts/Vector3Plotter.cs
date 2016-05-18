using UnityEngine;
using System.Collections;

public class Vector3Plotter : MonoBehaviour {

	public string plotLabel = "[UNLABELED]";

	public float negativeRange = -1f;
	public float positiveRange = 1f;

	public Vector2 screenPos = new Vector2(50f, 50f);

    public string PlotNameX {
        get {
            return plotLabel + "_X";
        }
    }
    public string PlotNameY {
        get {
            return plotLabel + "_Y";
        }
    }
    public string PlotNameZ {
        get {
            return plotLabel + "_Z";
        }
    }

    // Component references.
    // Use either this Sensor or the individual components, not both!
    //[Header("Use either this full Vector3...")]
    public Vector3Sensor sensor;

    //[Header("OR these components.")]
    //public FloatSensor sensorX;
    //public FloatSensor sensorY;
    //public FloatSensor sensorZ;

	// Use this for initialization
	void Start () {

		if (!sensor) {
            sensor = GetComponent<Vector3Sensor> ();
		}

		PlotManager.Instance.PlotCreate (PlotNameX, negativeRange, positiveRange, Color.red, screenPos);
		PlotManager.Instance.PlotCreate (PlotNameY, Color.green, PlotNameX);
		PlotManager.Instance.PlotCreate (PlotNameZ, Color.blue, PlotNameX);
	
	}

	// Update is called once per frame
	void FixedUpdate () {

        float x, y, z;
        Vector3 reading = sensor.Read ();
        x = reading.x;
        y = reading.y;
        z = reading.z;

        PlotManager.Instance.PlotAdd (PlotNameX, x);
		PlotManager.Instance.PlotAdd (PlotNameY, y);
		PlotManager.Instance.PlotAdd (PlotNameZ, z);
	
	}
}
