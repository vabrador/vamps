using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MusclePrediction {
    public MuscleSequence   restMuscleState = null;
    public List<Vector3>    restMuscleStateResponse = null;
    public int              imagMuscleStateID = -1;
    public List<Vector3>    imagMuscleStateResponse = null;
    public Vector3          tendencyVector = Vector3.zero;
    public List<Vector3>    alignedResponse = null;


    public Vector3 NosePositionWhenRestPredicted { get { return restMuscleStateResponse[0]; } }
    public Vector3 NosePositionWhenImagPredicted { get { return imagMuscleStateResponse[0]; } }

    public MusclePrediction() { }
    public MusclePrediction(MusclePrediction other) {
        restMuscleState = other.restMuscleState;
        restMuscleStateResponse = other.restMuscleStateResponse;
        imagMuscleStateID = other.imagMuscleStateID;
        imagMuscleStateResponse = other.imagMuscleStateResponse;
        tendencyVector = other.tendencyVector;
        alignedResponse = other.alignedResponse;
    }
}