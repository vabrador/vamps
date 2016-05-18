using System.Collections.Generic;

public class Memory {

    private Dictionary<Muscle, int> muscleLengths;
    public Dictionary<Muscle, int> MuscleLengths {
        get {
            if (muscleLengths == null) {
                muscleLengths = new Dictionary<Muscle, int>();
            }
            return muscleLengths;
        }
        set {
            muscleLengths = value;
        }
    }

    public int GoalValue = 0;

}
