using System;
using System.Linq;
using CustomExtensions;
using UnityEngine;

/// <summary>
/// A MuscleAction is an collection of contraction values,
/// and names of corresponding muscles, over (actionLength) timeslices.
/// </summary>
public class MuscleAction : System.Object {

    public int ActionLength {
        get {
            return contractionInputs.Length / MuscleNames.Length; }
    }
    private string[] muscleNames;
    public string[] MuscleNames {
        get {
            if (muscleNames == null) {
                muscleNames = new string[0];
            }
            return muscleNames;
        }
        set {
            muscleNames = value;
        }
    }
    private float[] contractionInputs;
    public float[] ContractionInputs {
        get {
            if (contractionInputs == null) {
                contractionInputs = new float[0];
            }
            return contractionInputs;
        }
        set {
            contractionInputs = value;
        }
    }

    public void Append(float[] contractionInputs) {
        ContractionInputs = ContractionInputs.Concat(contractionInputs).ToArray();
    }

    public MuscleCommand GetSlice(int timeIndex) {
        MuscleCommand commandSlice = new MuscleCommand();
        commandSlice.MuscleNames = MuscleNames;
        commandSlice.ContractionInputs =
            this.contractionInputs.Skip(timeIndex * MuscleNames.Length).Take(MuscleNames.Length).ToArray();
        return commandSlice;
    }

    public bool IsEmpty() {
        return ActionLength == 0;
    }

    public override bool Equals(System.Object other) {
        MuscleAction otherAction = other as MuscleAction;
        if (otherAction == null) {
            return false;
        }
        return this.Equals(otherAction);
    }
    public bool Equals(MuscleAction other) {
        if (IsEmpty()) { // All empty MuscleActions are equivalent regardless of their other variables.
            return other.IsEmpty();
        }
        return this.ActionLength == other.ActionLength
            && this.MuscleNames.Equals(other.MuscleNames)
            && this.ContractionInputs.Equals(other.ContractionInputs);
    }
    public override int GetHashCode() {
        if (IsEmpty())
            return this.ActionLength.GetHashCode();
        return this.ActionLength.GetHashCode() ^ this.MuscleNames.GetHashCode() ^ this.ContractionInputs.GetHashCode();
    }

    public override string ToString() {
        return string.Format("[MuscleAction: {0} muscles of contraction data over {1} timesteps]",
            MuscleNames.Length, ActionLength);
    }

}
