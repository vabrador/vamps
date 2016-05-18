using UnityEngine;
using System.Collections;

public class SenseMemory : MonoBehaviour { 

    private SenseCoefficients senseCs;
    public SenseCoefficients SenseCs {
        get {
            if (senseCs == null) {
                senseCs = new SenseCoefficients();
                senseCs.minReal = 0F;
                senseCs.maxReal = 10F;
                senseCs.senseLength = Length;
                senseCs.exponent = 1.618F;
            }
            return senseCs;
        }
        set {
            senseCs = value;
        }
    }

    [SerializeField]
    protected int length;
    public virtual int Length {
        get {
            return length;
        }
        set {
            length = value;
            ValueLengthCheck();
        }
    }
    
    [SerializeField]
    protected float value = 0F;
    public virtual float Value {
        get { return value; }
        set {
            this.value = Mathf.Min(SenseCs.maxReal, Mathf.Max(SenseCs.minReal, value));
        }
    }

    protected void ValueLengthCheck() {
        if (value >= length) {
            value = length - 1;
        }
    }

}
