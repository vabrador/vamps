using UnityEngine;
using System.Collections.Generic;

public class Attention : MonoBehaviour {
    
    private Muscle[] muscles;
    private Creature creature;
    void Start() {
        creature = Creature.GetCreature();
        muscles = creature.GetMuscles();
        if (muscles.Length < 1) {
            Debug.LogError("[Attention] Not enough muscles.");
        }

        ResetFocus();
    }

    public Muscle focus1;
    public Muscle focus2;
    void ResetFocus() {
        int oneIdx = 0, twoIdx = 0;
        while (oneIdx == twoIdx) {
            oneIdx = Random.Range(0, muscles.Length);
            twoIdx = Random.Range(0, muscles.Length);
        }
        focus1 = muscles[oneIdx];
        focus2 = muscles[twoIdx];
    }

    private Dictionary<MusclePair, MovementRelationBelief> muscleRelationBeliefSet = new Dictionary<MusclePair, MovementRelationBelief>();
    class MusclePair : System.Object {
        Muscle one;
        Muscle two;
        public override bool Equals(System.Object obj) {
            if (obj == null) {
                return false;
            }
            MusclePair p = obj as MusclePair;
            if (p == null) {
                return false;
            }
            else return Equals(p);
        }
        public bool Equals(MusclePair other) {
            return (one == other.one && two == other.two) || (two == other.one && one == other.two);
        }
        public override int GetHashCode() {
            return one.GetHashCode() ^ two.GetHashCode();
        }
    }
    class MovementRelationBelief {
        
    }





    void FixedUpdate() {

    }

}
