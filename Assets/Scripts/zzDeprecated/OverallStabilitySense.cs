using UnityEngine;
using System.Collections;

public class OverallStabilitySense : MonoBehaviour {

    // deprecated, used old int-based AxisSenseMemory

    /*

    [Tooltip("Stability score (sense value) is an integer from 0 to this value.")]
    private int senseLength = 10;
    [Tooltip("The maximum absolute distance from zero below which an acceleration is considered" +
        "\"stable,\" as a fraction of its maximum acceleration measurement. 0F to 1F.")]
    [Range(0F, 1F)]
    private float stabilityThreshold = 1/10F;

    private SenseMemory sense;
    public string SenseName {
        get { return "Overall Stability Sense"; }
    }

    void Awake() {
        // TODO: Update this sense for the new UseRealValue paradigm.
        // It's probably better to keep track of the real value for stability.
        sense = Creature.GetCreature().RegisterSense(SenseName, null);
        sense.Length = senseLength;
    }

    void FixedUpdate() {
        AxisSenseMemory[] accelerationSenses = Creature.GetCreature().GetAccelerationSenses();
        float instability = 0F;
        float sumMaxAccelMeasurable = 0F;
        int numAxes = 0;
        foreach (AxisSenseMemory axisSense in accelerationSenses) {
            float absAxisFraction = axisSense.AbsAxisFraction;
            if (absAxisFraction > stabilityThreshold) {
                instability += absAxisFraction - stabilityThreshold;
            }
            sumMaxAccelMeasurable += axisSense.AxisLength;
            numAxes++;
        }
        if (numAxes != 0) {
            float avgMaxAccelMeasurable = sumMaxAccelMeasurable / numAxes;
            sense.Value = (int)SenseUtil.SenseValueFromWorldValue(
                instability,
                0F,
                avgMaxAccelMeasurable,
                senseLength,
                2F
            );
        }
    }

    */

}
