using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ListOp {
    public static List<float> Invert(List<float> data, int numRows, int numCols) {
        List<float> inverted = new List<float>();
        for (int k = 0; k < numRows * numCols; k++) {
            inverted.Add(0F);
        }
        for (int i = 0; i < numRows; i++) {
            for (int j = 0; j < numCols; j++) {
                inverted[i * numCols + j] = data[j * numRows + i];
            }
        }
        return inverted;
    }

    public static List<float> InvertAndReverseCols(List<float> data, int numRows, int numCols) {
        List<float> inverted = new List<float>();
        for (int k = 0; k < numRows * numCols; k++) {
            inverted.Add(0F);
        }
        for (int i = 0; i < numRows; i++) {
            for (int j = 0; j < numCols; j++) {
                inverted[i * numCols + (numCols - 1 - j)] = data[j * numRows + i];
            }
        }
        return inverted;
    }

    public static string ListToString<T>(List<T> list) {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        foreach (T value in list) {
            sb.Append(value.ToString());
            sb.Append(",");
        }
        sb.Append("]");
        return sb.ToString();
    }

    public static List<Vector3> UnflattenNumpyOscDataToVector3s(List<float> flatData) {
        List<Vector3> v3s = new List<Vector3>();
        int cur = 0;
        int n_dimensions = (int)flatData[cur++];
        List<int> shape = new List<int>();
        for (int i = 0; i < n_dimensions; i++) {
            shape.Add((int)flatData[cur++]);
        }
        if (shape[0] != 3) {
            Debug.LogError("[ListOp] Could not parse Vector3s from data shaped [" + shape[0] + "," + shape[1] + "]");
            return null;
        }
        for (int c = 0; c < shape[1]; c++) {
            Vector3 v3 = Vector3.zero;
            v3.x = flatData[cur];
            v3.y = flatData[shape[1] + cur];
            v3.z = flatData[2 * shape[1] + cur];
            cur += 1;
            v3s.Add(v3);
        }
        return v3s;
    }

    public static List<Vector3> ElementwiseSubtract(List<Vector3> initial, List<Vector3> subtracts) {
        if (initial.Count != subtracts.Count) {
            Debug.LogError("[ListOp] Lists not the same length.");
            return null;
        }
        List<Vector3> subtracted = initial.Select((startVector, index) =>  startVector - subtracts[index] ).ToList();
        return subtracted;
    }

    /// Shifts the right-operand list so that its first element is identical to the first element of
    /// the left operand, then returns left minus right, element-wise.
    public static List<Vector3> AlignedSubtract(List<Vector3> initial, List<Vector3> subtracts) {
        Vector3 shift = initial[0] - subtracts[0];
        return initial.Select((vector, index) => (vector - subtracts[index]) + shift).ToList();
    }
    public static List<Vector3> AlignedSubtractWithGain(List<Vector3> initial, List<Vector3> subtracts, float leftGain = 1F, float rightGain = 1F) {
        Vector3 shift = initial[0] * leftGain - subtracts[0] * rightGain;
        return initial.Select((vector, index) => (vector * leftGain) - (subtracts[index] * rightGain + shift)).ToList();
    }
}