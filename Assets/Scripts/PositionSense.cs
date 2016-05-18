using UnityEngine;
using System.Collections;

public class PositionSense : MonoBehaviour {

    [Header("Position to Track")]
    public Transform positionToTrack;
    [Header("Corner of Tracking Volume (Auto)")]
    public Transform trackVolumeTransform;
    [Header("Volume Square Radius")]
    [Tooltip("The position sense will clipped and gridded to the cube with this radius in Unity units.")]
    public float squareSensoryRadius;

    [Header("Sense Memory Out (Auto)")]
    public SenseMemory xSense;
    public string XSenseSuffix { get { return " X Sense"; } }
    public string XSenseName { get { return gameObject.name + XSenseSuffix; } }
    public SenseMemory ySense;
    public string YSenseSuffix { get { return " Y Sense"; } }
    public string YSenseName { get { return gameObject.name + YSenseSuffix; } }
    public SenseMemory zSense;
    public string ZSenseSuffix { get { return " Z Sense"; } }
    public string ZSenseName { get { return gameObject.name + ZSenseSuffix; } }

    [Header("Optional World Position Sensor Output")]
    public Vector3Sensor positionSensor;

    private Creature creature;
    void Awake() {
        creature = Creature.GetCreature();

        GameObject relativeToObj = new GameObject();
        trackVolumeTransform = relativeToObj.transform;
        trackVolumeTransform.SetParent(this.transform);
        trackVolumeTransform.localPosition = new Vector3(-squareSensoryRadius, -squareSensoryRadius, -squareSensoryRadius);

        SenseCoefficients posSenseCoefficients = new SenseCoefficients() {
                minReal = -squareSensoryRadius,
                maxReal = squareSensoryRadius,
                senseLength = 20,
                exponent = 1F // linear, because we're tracking an actual position in linear 3-space
            };

        xSense = creature.RegisterSense(XSenseName, posSenseCoefficients);
        xSense.Length = posSenseCoefficients.senseLength;
        ySense = creature.RegisterSense(YSenseName, posSenseCoefficients);
        ySense.Length = posSenseCoefficients.senseLength;
        zSense = creature.RegisterSense(ZSenseName, posSenseCoefficients);
        zSense.Length = posSenseCoefficients.senseLength;
    }

    void FixedUpdate() {
        Vector3 trackedOffsetFromTrackVolumeCorner = positionToTrack.position - trackVolumeTransform.position;
        if (xSense) {
            xSense.Value = trackedOffsetFromTrackVolumeCorner.x - squareSensoryRadius;
        }
        if (ySense) {
            ySense.Value = trackedOffsetFromTrackVolumeCorner.y - squareSensoryRadius;
        }
        if (zSense) {
            zSense.Value = trackedOffsetFromTrackVolumeCorner.z - squareSensoryRadius;
        }
        if (positionSensor != null) {
            positionSensor.SetSensorValue(positionToTrack.position);
        }
    }

}
