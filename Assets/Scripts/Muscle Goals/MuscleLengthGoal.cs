using UnityEngine;
using System.Collections.Generic;

public class MuscleLengthGoal : MonoBehaviour{

    private Creature creature;

    private Dictionary<Muscle, int> muscleLengthTargets = null;
    public Dictionary<Muscle, int> MuscleLengthTargets {
        get {
            if (muscleLengthTargets == null) {
                muscleLengthTargets = new Dictionary<Muscle, int>();
            }
            return muscleLengthTargets;
        }
        set {
            muscleLengthTargets = value;
        }
    }

    [Tooltip("Recharge time in seconds for relaxing or tightening a muscle by one fiber.")]
    public float cooldownTime = 0.5F;
    private Dictionary<Muscle, float> muscleChangeCooldowns = null;
    public Dictionary<Muscle, float> MuscleChangeCooldowns {
        get {
            if (muscleChangeCooldowns == null) {
                muscleChangeCooldowns = new Dictionary<Muscle, float>();
            }
            return muscleChangeCooldowns;
        }
        set {
            muscleChangeCooldowns = value;
        }
    }

    void Awake() {
        MuscleLengthTargets = null;
    }

    void Start() {
        creature = transform.GetComponentInParent<Creature>();
    }

    void FixedUpdate() {
        foreach (Muscle muscle in MuscleLengthTargets.Keys) {
            if (MuscleChangeCooldowns.ContainsKey(muscle)) {
                // Still needs to cool down.
                MuscleChangeCooldowns[muscle] -= Time.fixedDeltaTime;
                if (MuscleChangeCooldowns[muscle] < 0F) {
                    MuscleChangeCooldowns.Remove(muscle);
                }
            }
            else {
                //Debug.LogWarning("[Muscle: " + muscle.name + "] No longer on cooldown.");
                float desiredLength = muscleLengthTargets[muscle];
                float currentLength = creature.GetSenseValue(muscle.LengthSenseName);
                if (desiredLength > currentLength) {
                    muscle.Relax(1);
                    MuscleChangeCooldowns[muscle] = cooldownTime;
                }
                else if (desiredLength < currentLength) {
                    muscle.Tighten(1);
                    MuscleChangeCooldowns[muscle] = cooldownTime;
                }
            }
        }
    }

    public void AddTarget(Muscle muscle, int desiredLength) {
        MuscleLengthTargets[muscle] = desiredLength;
    }

    public void MatchMusclesFromMemory(Memory memory) {
        MuscleLengthTargets = memory.MuscleLengths;
    }

}
