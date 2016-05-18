using UnityEngine;
using System.Collections;

public class WorldDistanceSense : MonoBehaviour {

    public Transform worldTransform;
    public int senseLength = 10;
    public float minDistance = 0.0F;
    public float maxDistance = 10.0F;

    private SenseMemory sense;
    public string SenseName {
        get { return "World Distance Sense"; }
    }

    void Awake() {
        sense = Creature.GetCreature().RegisterSense(SenseName, null);
        sense.Length = senseLength;
    }

    void FixedUpdate() {
        sense.Value = (int)SenseUtil.SenseValueFromWorldValue(
            (worldTransform.position - transform.position).magnitude,
            minDistance,
            maxDistance,
            senseLength);
    }

}
