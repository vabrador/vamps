using System;
using System.Text;

/// <summary>
/// A MuscleCommand is a MuscleAction of duration 1 ts (timeslice, or timestep).
/// </summary>
public class MuscleCommand : Object {

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

    public bool IsEmpty() {
        return MuscleNames.Length == 0;
    }

    public override bool Equals(Object other) {
        MuscleCommand otherCommand = other as MuscleCommand;
        if (otherCommand == null) {
            return false;
        }
        return this.Equals(otherCommand);
    }
    public bool Equals(MuscleCommand other) {
        if (IsEmpty()) { // All empty MuscleCommands are equivalent regardless of their other variables.
            return other.IsEmpty();
        }
        var namesMatch = true;
        int idx = 0;
        foreach (string name in muscleNames) {
            if (name != other.muscleNames[idx++]) {
                return false;
            }
        }
        var valuesMatch = true;
        idx = 0;
        foreach (float value in contractionInputs) {
            if (value != other.contractionInputs[idx++]) {
                return false;
            }
        }
        return namesMatch && valuesMatch;
    }
    public override int GetHashCode() {
        if (IsEmpty()) {
            return this.MuscleNames.GetHashCode();
        }
        return this.MuscleNames.GetHashCode() ^ this.ContractionInputs.GetHashCode();
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        int i = 0;
        int c = ContractionInputs.Length;
        foreach (float contractionValue in ContractionInputs) {
            sb.Append(contractionValue);
            if (i < c-1) {
                sb.Append(", ");
            }
        }
        sb.Append("]");
        return sb.ToString();
    }

}
