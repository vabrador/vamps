using UnityEngine;
using CustomExtensions;
using System.Collections.Generic;
using System.Linq;

public class MuscleSequenceBuilder : MonoBehaviour {

    private Creature creature;
    void Awake() {
        creature = Creature.GetCreature();
    }
    void Start() {
        creature = Creature.GetCreature();
    }

    /// <summary>Returns a MuscleSequence containing one MuscleAction of length
    /// (numTimes) where each frame is (toDuplicate).</summary>
    public MuscleSequence Duplicate(MuscleCommand toDuplicate, int numTimes) {
        MuscleSequence sequence = new MuscleSequence();
        string[] muscleNames = creature.GetMuscleNames();

        // Just one action containing the duplicate MuscleCommands.
        MuscleAction action1 = new MuscleAction();
        action1.MuscleNames = muscleNames;
        for (int ts = 0; ts < numTimes; ts++) {
            action1.Append(toDuplicate.ContractionInputs);
        }
        sequence.Add(action1);

        sequence.CalcLength();
        return sequence;
    }

    public MuscleSequence BuildWeightedAveragedSequence(List<MuscleSequence> sequences, List<float> weights) {
        MuscleSequence sequence = new MuscleSequence();
        string[] muscleNames = creature.GetMuscleNames();

        MuscleAction action1 = new MuscleAction();
        action1.MuscleNames = muscleNames;
        float[] weightedAveragedAction = new float[18] { 0, 0, 0, 0, 0, 0,
                                                       0, 0, 0, 0, 0, 0,
                                                       0, 0, 0, 0, 0, 0, };
        for (int i = 0; i < sequences.Count; i++) {
            for (int j = 0; j < 18; j++) {
                //if (sequences == null) {
                //    Debug.Log("sequence builder 46: sequences is null");
                //}
                //else if (sequences[i] == null) {
                //    Debug.Log("sequence builder 49: sequences at " + i + " is null");
                //}
                //else if (sequences[i].GetCommand(0) == null) {
                //    Debug.Log("sequence builder 52: sequences at " + i + ", the first command is null");
                //}
                weightedAveragedAction[j] += (sequences[i].GetCommand(0).ContractionInputs[j] * weights[i]);
            }
        }
        for (int i = 0; i < sequences[0].Length; i++) {
            action1.Append(weightedAveragedAction);
        }
        Debug.Log("WeightedAverageSequence: " + ListOp.ListToString(weightedAveragedAction.ToList()));
        sequence.Add(action1);

        sequence.Length = sequences[0].Length;
        return sequence;
    }

    public MuscleSequence BuildRandomStaticSequence(int length) {
        MuscleSequence sequence = new MuscleSequence();
        string[] muscleNames = creature.GetMuscleNames();

        // Just one action: Zeros on everything -- total relaxation.
        MuscleAction action1 = new MuscleAction();
        action1.MuscleNames = muscleNames;
        float gain = 10;
        float[] randomAction = new float[18] { Random.value * gain, Random.value * gain, Random.value * gain,
                                           Random.value * gain, Random.value * gain, Random.value * gain,
                                           Random.value * gain, Random.value * gain, Random.value * gain,
                                           Random.value * gain, Random.value * gain, Random.value * gain,
                                           Random.value * gain, Random.value * gain, Random.value * gain,
                                           Random.value * gain, Random.value * gain, Random.value * gain  };
        for (int ts = 0; ts < length; ts++) {
            action1.Append(randomAction);
        }
        sequence.Add(action1);

        sequence.Length = sequence.CalcLength();
        return sequence;
    }

    public MuscleSequence BuildZeroSequence() {
        MuscleSequence sequence = new MuscleSequence();
        string[] muscleNames = creature.GetMuscleNames();

        // Just one action: Zeros on everything -- total relaxation.
        MuscleAction action1 = new MuscleAction();
        action1.MuscleNames = muscleNames;
        for (int ts = 0; ts < 1; ts++) {
            action1.Append(new float[18] { 0, 0, 0, 0, 0, 0,
                                           0, 0, 0, 0, 0, 0,
                                           0, 0, 0, 0, 0, 0  });
        }
        sequence.Add(action1);

        sequence.Length = sequence.CalcLength();
        return sequence;
    }

    public MuscleSequence BuildPullUpSequence() {
        MuscleSequence sequence = new MuscleSequence();
        string[] muscleNames = creature.GetMuscleNames();

        // Just one action: Pull up!
        MuscleAction action1 = new MuscleAction();
        action1.MuscleNames = muscleNames;
        for (int ts = 0; ts < 90; ts++) {
            action1.Append(new float[18] { 10, 0, 0, 0, 0, 10,
                                           10, 0, 0, 0, 0, 10,
                                           0,  0, 0, 0, 0, 0  });
        }
        sequence.Add(action1);
        return sequence;
    }

    public MuscleSequence BuildGamutSequence() {
        MuscleSequence sequence = new MuscleSequence();
        string[] muscleNames = creature.GetMuscleNames();

        // Action 1: Up and down.
        MuscleAction action1 = new MuscleAction();
        action1.MuscleNames = muscleNames;
        for (int ts = 0; ts < 250; ts++) {
            action1.Append(new float[18] { 0, 0, 0, 0, 0, 0,
                                           0, 0, 0, 0, 0, 0, 
                                           0, 0, 0, 0, 0, 0 });
        }
        for (int ts = 0; ts < 250; ts++) {
            action1.Append(new float[18] { _smooth_(ts, 0, 10, dur:250), _smooth_(ts, 0, 6, dur:250), 0, 0, _smooth_(ts, 0, 6, dur:250), _smooth_(ts, 0, 10, dur:250),
                                           _smooth_(ts, 0, 10, dur:250), 0, 0, 0, 0, _smooth_(ts, 0, 10, dur:250), 
                                           0, 0, 0, 0, 0, 0 });
        }
        for (int ts = 0; ts < 250; ts++) {
            action1.Append(new float[18] { _smooth_(ts, 10, 0, dur:250), _smooth_(ts, 6, 0, dur:250), _smooth_(ts, 0, 10, dur:250), _smooth_(ts, 0, 10, dur:250), _smooth_(ts, 6, 0, dur:250), _smooth_(ts, 10, 0, dur:250),
                                           _smooth_(ts, 10, 0, dur:250), 0, _smooth_(ts, 0, 10, dur:250), _smooth_(ts, 0, 10, dur:250), 0, _smooth_(ts, 10, 0, dur:250), 
                                           0, 0, 0, 0, 0, 0 });
        }
        for (int ts = 0; ts < 250; ts++) {
            action1.Append(new float[18] { 0, 0, _smooth_(ts, 10, 0, dur:250), _smooth_(ts, 10, 0, dur:250), 0, 0,
                                           0, 0, _smooth_(ts, 10, 0, dur:250), _smooth_(ts, 10, 0, dur:250), 0, 0, 
                                           0, 0, 0, 0, 0, 0 });
        }
        //Debug.Log(action1.ContractionInputs.ArrayToString<int>());
        sequence.Add(action1);

        // Action 2: Around and around.
        MuscleAction action2 = new MuscleAction();
        action2.MuscleNames = muscleNames;
        for (int ts = 0; ts < 420; ts++) {
            action2.Append(new float[18] { _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120),
                                           0, 0, 0, 0, 0, 0, 
                                           0, 0, 0, 0, 0, 0 });
        }
        for (int ts = 0; ts < 420; ts++) {
            action2.Append(new float[18] { 0, 0, 0, 0, 0, 0,
                                           _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120),
                                           0, 0, 0, 0, 0, 0 });
        }
        for (int ts = 0; ts < 420; ts++) {
            action2.Append(new float[18] { _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120),
                                           _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120),
                                           0, 0, 0, 0, 0, 0 });
        }
        for (int ts = 0; ts < 420; ts++) {
            action2.Append(new float[18] { _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120),
                                           _onecycle_(ts, 0, 10, delay:300, dur:120), _onecycle_(ts, 0, 10, delay:240, dur:120),
                                           _onecycle_(ts, 0, 10, delay:180, dur:120), _onecycle_(ts, 0, 10, delay:120, dur:120),
                                           _onecycle_(ts, 0, 10, delay:60, dur:120), _onecycle_(ts, 0, 10, delay:0, dur:120),
                                           0, 0, 0, 0, 0, 0 });
        }
        for (int ts = 0; ts < 420; ts++) {
            action2.Append(new float[18] { _onecycle_(ts, 0, 10, delay:300, dur:120), _onecycle_(ts, 0, 10, delay:240, dur:120),
                                           _onecycle_(ts, 0, 10, delay:180, dur:120), _onecycle_(ts, 0, 10, delay:120, dur:120),
                                           _onecycle_(ts, 0, 10, delay:60, dur:120), _onecycle_(ts, 0, 10, delay:0, dur:120),
                                           _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120),
                                           0, 0, 0, 0, 0, 0 });
        }
        sequence.Add(action2);

        // Action 3: Around and around, with twisting!
        MuscleAction action3 = new MuscleAction();
        action3.MuscleNames = muscleNames;
        for (int ts = 0; ts < 420; ts++) {
            action3.Append(new float[18] { _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120),
                                           0, 0, 0, 0, 0, 0, 
                                           _poscos_(ts, 60)*10, _poscos_(ts, 60)*10,
                                           _poscos_(ts, 60, phase:0.5F)*10,
                                           _poscos_(ts, 60)*10,
                                           _poscos_(ts, 60, phase:0.5F)*10, _poscos_(ts, 60, phase:0.5F)*10 });
        }
        for (int ts = 0; ts < 420; ts++) {
            action3.Append(new float[18] { 0, 0, 0, 0, 0, 0,
                                           _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120), 
                                           _poscos_(ts, 60)*10, _poscos_(ts, 60)*10,
                                           _poscos_(ts, 60, phase:0.5F)*10,
                                           _poscos_(ts, 60)*10,
                                           _poscos_(ts, 60, phase:0.5F)*10, _poscos_(ts, 60, phase:0.5F)*10 });
        }
        for (int ts = 0; ts < 420; ts++) {
            action3.Append(new float[18] { _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120),
                                           _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120), 
                                           _poscos_(ts, 60)*10, _poscos_(ts, 60)*10,
                                           _poscos_(ts, 60, phase:0.5F)*10,
                                           _poscos_(ts, 60)*10,
                                           _poscos_(ts, 60, phase:0.5F)*10, _poscos_(ts, 60, phase:0.5F)*10 });
        }
        for (int ts = 0; ts < 420; ts++) {
            action3.Append(new float[18] { _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120),
                                           _onecycle_(ts, 0, 10, delay:300, dur:120), _onecycle_(ts, 0, 10, delay:240, dur:120),
                                           _onecycle_(ts, 0, 10, delay:180, dur:120), _onecycle_(ts, 0, 10, delay:120, dur:120),
                                           _onecycle_(ts, 0, 10, delay:60, dur:120), _onecycle_(ts, 0, 10, delay:0, dur:120), 
                                           _poscos_(ts, 60)*10, _poscos_(ts, 60)*10,
                                           _poscos_(ts, 60, phase:0.5F)*10,
                                           _poscos_(ts, 60)*10,
                                           _poscos_(ts, 60, phase:0.5F)*10, _poscos_(ts, 60, phase:0.5F)*10 });
        }
        for (int ts = 0; ts < 420; ts++) {
            action3.Append(new float[18] { _onecycle_(ts, 0, 10, delay:300, dur:120), _onecycle_(ts, 0, 10, delay:240, dur:120),
                                           _onecycle_(ts, 0, 10, delay:180, dur:120), _onecycle_(ts, 0, 10, delay:120, dur:120),
                                           _onecycle_(ts, 0, 10, delay:60, dur:120), _onecycle_(ts, 0, 10, delay:0, dur:120),
                                           _onecycle_(ts, 0, 10, delay:0, dur:120), _onecycle_(ts, 0, 10, delay:60, dur:120),
                                           _onecycle_(ts, 0, 10, delay:120, dur:120), _onecycle_(ts, 0, 10, delay:180, dur:120),
                                           _onecycle_(ts, 0, 10, delay:240, dur:120), _onecycle_(ts, 0, 10, delay:300, dur:120), 
                                           _poscos_(ts, 60)*10, _poscos_(ts, 60)*10,
                                           _poscos_(ts, 60, phase:0.5F)*10,
                                           _poscos_(ts, 60)*10,
                                           _poscos_(ts, 60, phase:0.5F)*10, _poscos_(ts, 60, phase:0.5F)*10 });
        }
        sequence.Add(action3);

        // Action 4: Finish at rest.
        MuscleAction action4 = new MuscleAction();
        action4.MuscleNames = muscleNames;
        for (int ts = 0; ts < 50; ts++) {
            action4.Append(new float[18] { 0, 0, 0, 0, 0, 0,
                                           0, 0, 0, 0, 0, 0,
                                           0, 0, 0, 0, 0, 0 });
        }
        sequence.Add(action4);

        return sequence;
    }
    /// <summary>delayable smoothed (see _smooth_) Attack-Sustain-Release envelope (no Decay after attack)</summary>
    private static float _envelope_(int ts, float att, float sus, float rel, int delay=0) {
        ts -= delay;
        return (ts < 0 || ts > att+sus+rel)?
                   ((ts < att+sus)?
                       ((ts < att)?
                           (_smooth_(ts, 0, 1, att))
                       : (1))
                   : (_smooth_(ts, 1, 0, rel)))
               : (0);
    }
    /// <summary>delayable sine oscillation once</summary>
    private static float _onecycle_(int ts, float start, float peak, float dur, int delay=0) {
        ts -= delay;
        return (ts >= 0)?
                   ((ts < dur)?
                       (start + (peak - start) * _sin_(ts, dur * 2))
                   : (start))
               : (start);
    }
    /// <summary>sine curve interpolation between start and stop</summary>
    private static float _smooth_(int ts, float start, float stop, float dur, int delay=0) {
        ts -= delay;
        return (ts >= 0)?
                   ((ts < dur)?
                       (start + ((stop - start) * _poscos_(ts, dur * 2)))
                   : stop)
               : start;
    }
    /// <summary>positive cosine between 0 and 1</summary>
    private static float _poscos_(int ts, float periodTS, float phase=0F) {
        return 0.5F * (1 + _sin_(ts, periodTS, phase:phase+0.75F));
    }
    private static float _sin_(int ts, float periodTS, float phase=0F) {
        return Mathf.Sin((phase + (ts * 1F / periodTS)) * 2F * Mathf.PI);
    }

}
