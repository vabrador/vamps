using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using CustomExtensions;

/// <summary>
/// A MuscleSequence encapsulates a lists of MuscleActions.
/// </summary>
public class MuscleSequence : System.Object {

    private List<MuscleAction> muscleActions;
    public List<MuscleAction> MuscleActions {
        get {
            if (muscleActions == null) {
                muscleActions = new List<MuscleAction>();
            }
            return muscleActions;
        }
    }

    public void Add(MuscleAction muscleAction) {
        MuscleActions.Add(muscleAction);
        length += muscleAction.ActionLength;
    }

    public int CalcLength() {
        length = MuscleActions.Sum(x => x.ActionLength);
        return length;
    }
    private int length = -1;
    public int Length {
        get {
            if (length == -1) {
                CalcLength();
            }
            return length;
        }
        set {
            length = value;
        }
    }

    public MuscleSequence() { }

    public MuscleCommand GetCommand(int index) {
        if (index >= Length) {
            return null;
        }
        int actionIndexOffset = 0;
        int actionIndex = 0;
        for (actionIndex = 0; actionIndex < MuscleActions.Count; actionIndex++) {
            actionIndexOffset += MuscleActions[actionIndex].ActionLength;
            if (actionIndexOffset > index) {
                actionIndexOffset -= MuscleActions[actionIndex].ActionLength;
                break;
            }
        }
        return MuscleActions[actionIndex].GetSlice(index - actionIndexOffset);
    }

    public List<float> GetFlatList() {
        List<float> inputs = new List<float>();
        foreach (MuscleAction action in MuscleActions) {
            inputs.AddRange(action.ContractionInputs.Select(x => (float)x));
        }
        return inputs;
    }

    public bool IsEmpty() {
        return MuscleActions.Count == 0 || MuscleActions.All<MuscleAction>(x => x.IsEmpty());
    }

    public override bool Equals(System.Object other) {
        MuscleSequence otherSequence = other as MuscleSequence;
        if (otherSequence == null) {
            return false;
        }
        return this.Equals(otherSequence);
    }
    public bool Equals(MuscleSequence other) {
        if (IsEmpty()) { // All empty MuscleActions are equivalent regardless of their other variables.
            return other.IsEmpty();
        }
        return MuscleActions.Equals(other.MuscleActions);
    }
    public override int GetHashCode() {
        if (IsEmpty())
            return MuscleActions.GetHashCode();
        return MuscleActions.GetHashCode();
    }

    public static MuscleSequence FromCSV(
        string actionLengthsPath,
        string muscleNamesPath,
        string contractionInputsPath) {

        int[] actionLengths = File.ReadAllLines(actionLengthsPath)[0].Trim().Split(',').ParseInts().ToArray();
        int sumActionLengths = actionLengths.Sum();
        
        string[] muscleNames = File.ReadAllLines(muscleNamesPath)[0].Trim().Split(',');
        int numMuscles = muscleNames.Length;

        float[] contractionInputs = new float[sumActionLengths * numMuscles];
        int i = 0;
        foreach (string line in File.ReadAllLines(contractionInputsPath)) {
            if (line.Length == 0) continue;
            else {
                foreach (int contractionValue in line.Trim().Split(',').ParseInts()) {
                    contractionInputs[i++] = contractionValue;
                }
            }
        }

        MuscleSequence muscleSequence = new MuscleSequence();
        int actionBeginIndex = 0;
        for (int idx = 0; idx < actionLengths.Length; idx++) {
            MuscleAction muscleAction = new MuscleAction();
            muscleAction.MuscleNames = muscleNames;
            int actionLength = actionLengths[idx];
            int numInputs = actionLength * numMuscles;
            muscleAction.ContractionInputs = contractionInputs.Skip(actionBeginIndex).Take(numInputs).ToArray();
            actionBeginIndex += numInputs;
            muscleSequence.Add(muscleAction);
        }

        return muscleSequence;
    }

}
