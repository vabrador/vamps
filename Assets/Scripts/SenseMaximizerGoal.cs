using UnityEngine;
using System.Collections.Generic;

public class SenseMaximizerGoal : MonoBehaviour {

    /* public string goalSenseName;
    public bool minimizer = false;

    private Creature creature;
    private MemoryAccumulator memories;
    private int goalSenseMemoryIdx;

    void Start() {
        creature = transform.GetComponentInParent<Creature>();
        memories = creature.Memories;
        goalSenseMemoryIdx = creature.GetSenseMemoryIndex(goalSenseName);
    }

    void FixedUpdate() {
        Memory memoryWithBestGoalValue;
        if (!minimizer) {
            memoryWithBestGoalValue = memories.FindHighestSenseValue(goalSenseMemoryIdx);
        }
        else {
            memoryWithBestGoalValue = memories.FindLowestSenseValue(goalSenseMemoryIdx);
        }

        // baked in here is the idea that the goal can be accomplished by maintaining a specific muscle configuration.
        MuscleLengthGoal muscleLengthGoal = creature.GetHead().GetComponent<MuscleLengthGoal>();
        if (!muscleLengthGoal) {
            muscleLengthGoal = creature.GetHead().AddComponent<MuscleLengthGoal>();
        }
        muscleLengthGoal.MatchMusclesFromMemory(memoryWithBestGoalValue);
    } */

}
