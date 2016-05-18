using UnityEngine;

public class SenseCoefficients {
    public float minReal;
    public float maxReal;
    public int senseLength;
    public float exponent = 1.618F;
}

public static class SenseUtil {

    //
    // Curve plot, minMeasureable = 5, maxMeasureable = 15, senseLength = 40, exponent ~= 1.618
    // https://www.wolframalpha.com/input/?i=y+%3D+5+%2B+((15+-+5)+*+(x%5E(golden+ratio))+%2F+(40%5E(golden+ratio)))+for+x+from+0+to+40
    //
    public static float WorldValueFromSenseValue(
        int sense,
        float minActual,
        float maxActual,
        int senseLength,
        float exponent = 1.618F) {
        return WorldValueFromSenseValue((float)sense, minActual, maxActual, senseLength, exponent);
    }
    public static float WorldValueFromSenseValue(
        int sense,
        SenseCoefficients senseCs) {
        return WorldValueFromSenseValue((float)sense, senseCs.minReal, senseCs.maxReal, senseCs.senseLength, senseCs.exponent);
    }
    public static float WorldValueFromSenseValue(
        float sense,
        SenseCoefficients senseCs) {
        return WorldValueFromSenseValue(sense, senseCs.minReal, senseCs.maxReal, senseCs.senseLength, senseCs.exponent);
    }
    public static float WorldValueFromSenseValue(
        float sense,
        float minActual,
        float maxActual,
        int senseLength,
        float exponent = 1.618F) {
        return minActual + ((maxActual - minActual) * Mathf.Pow(sense, exponent) / (Mathf.Pow(senseLength, exponent)));
    }

    //
    // Too lazy to math on paper so Wolfram Alpha says this is the inverse of the above,
    // where minMeasurable = M, maxMeasurable = A, realValue = x, senseValue = y, R = senseLength, phi = exponent
    // https://www.wolframalpha.com/input/?i=solve+y+%3D+M+%2B+((A+-+M)+*+(x%5E(phi))+%2F+(R%5E(phi)))+for+x
    // and, verified numerically:
    // https://www.wolframalpha.com/input/?i=y+%3D+(-((5-x)*(40%5E1.618))%2F(15-5))%5E(1%2F1.618)+for+x+from+5+to+15
    //
    public static float SenseValueFromRealValue(
        float real,
        SenseCoefficients senseCs) {
        return SenseValueFromWorldValue(real, senseCs.minReal, senseCs.maxReal, senseCs.senseLength, senseCs.exponent);
    }

    /// <summary>Maps input signals to sensory signals.</summary>
    public static float SenseValueFromWorldValue(
        float real,
        float minActual,
        float maxActual,
        int senseLength,
        float exponent = 1.618F) {

        if (real < minActual) {
            return minActual;
        }
        if (real > maxActual) {
            return maxActual;
        }

        float fval = Mathf.Pow(-(((minActual-real) * Mathf.Pow(senseLength, exponent)) / (maxActual - minActual)), 1F/exponent);

        return fval;

        /*if (float.IsNaN(fval)) {
            // Still debugging this -- put a breakpoint here for NaN debugging
            float val1 = (minReal - real);
            float val2 = Mathf.Pow(senseLength, exponent);
            float val3 = (maxReal - minReal);
            float val4 = (-((minReal-real) * Mathf.Pow(senseLength, exponent)));
            float val5 = (-((minReal-real) * Mathf.Pow(senseLength, exponent)) / (maxReal - minReal));
            float val6 = Mathf.Pow(0.1293F, 1F/exponent);
            fval = val1 + val2 + val3 + val4 + val5 + val6; // ignore this
        }*/
    }


}
