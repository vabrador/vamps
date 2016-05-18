using UnityEngine;
using System.Collections;

public class AxisSenseMemory : SenseMemory {

    // deprecated--unused extension of old int-based SenseMemory model

    /*private bool zeroPointSet = false;
    void Awake() {
        if (!zeroPointSet) {
            zeroPoint = GetDefaultZeroPoint();
            zeroPointSet = true;
        }
    }

    private int zeroPoint = 0;
    public int ZeroPoint {
        get { return zeroPoint; }
        set {
            zeroPoint = value;
            zeroPointSet = true;
            ZeroPointCheck();
        }
    }

    public int AxisValue {
        get { return ZeroPoint - Value; }
    }
    public int AbsAxisValue {
        get { return Mathf.Abs(AxisValue); }
    }
    public int ValueDirection {
        get { return (AxisValue >= 0 ? (AxisValue == 0 ? 0 : 1) : -1); }
    }

    public new int Length {
        get {
            return Length;
        }
        set {
            length = value;
            ValueLengthCheck();
            ZeroPointCheck();
        }
    }
    /// <summary>The maximum AxisValue in either direction.</summary>
    public int AxisLength {
        get { return Length / 2; }
    }
    /// <summary>AxisValue / AxisLength.</summary>
    public float AxisFraction {
        get { return AxisValue / (float)AxisLength; }
    }
    public float AbsAxisFraction {
        get { return Mathf.Abs(AxisFraction); }
    }

    protected void ZeroPointCheck() {
        if (zeroPoint >= Length) {
            zeroPoint = GetDefaultZeroPoint();
        }
    }

    protected int GetDefaultZeroPoint() {
        return zeroPoint = Length / 2;
    }*/

}
