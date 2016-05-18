using UnityEngine;
using System.Collections;

public class MuscleRandomExploreGoal : MonoBehaviour {

    private Creature creature;
    private Muscle[] muscles;
    
    private float extraLongMultiplier = 7F;
    private void ChooseNewMuscleState(float power) {

        if (Random.value > 0.2F) {
            // Usually pick a few muscles to modify slightly
            foreach (Muscle muscle in muscles) {
                if (Random.value < 0.2) {
                    muscle.DriftContraction(muscle.numFibers / 10 * power);
                }
            }
        }
        else if (Random.value > 0.5F) {
            foreach (Muscle muscle in muscles) {
                if (Random.value < 0.45) {
                    muscle.DriftContraction(muscle.numFibers / 10 * power);
                }
            }
        }
        else {
            foreach (Muscle muscle in muscles) {
                if (Random.value < 0.8) {
                    muscle.DriftContraction(muscle.numFibers / 10 * power);
                }
            }
        }
    }

    private int durationToWaitThisCycle = 5;
    void Start() {
        creature = Creature.GetCreature();
        muscles = creature.GetMuscles();
        //durationToWaitThisCycle = GetNewDuration();
    }

    private float oscPhase = 0F;
    private float SlowOscillation() {
        var toReturn = (Mathf.Sin(oscPhase / 360F * (Mathf.PI * 2F)) + 1F) / 2F;
        oscPhase += 1F;
        return toReturn;
    }

    private int frameCounter = 0;
    void FixedUpdate() {
        if (frameCounter >= durationToWaitThisCycle) {
            ChooseNewMuscleState(Random.value * 5 * SlowOscillation());
            frameCounter -= durationToWaitThisCycle;
            //durationToWaitThisCycle = GetNewDuration();
        }
        frameCounter++;
    }

}
