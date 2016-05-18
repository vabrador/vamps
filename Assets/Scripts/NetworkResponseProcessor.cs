using CustomExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class NetworkResponseProcessor : MonoBehaviour {

    public delegate void OnMuscleResponsePrediction(MusclePrediction response);
    public OnMuscleResponsePrediction onMuscleResponsePrediction;

    public bool visualizeResponses = true;
    public Transform visualizationSourceObject;
    public bool visualizeResponseDifferences = true;
    public Transform differenceVisualizationSourceObject;
    public Transform restVisualizationSourceObject;
    
    public bool HasCallbacks {
        get { return false; }
    }


    NetworkPredictionRequester requester;
    BalancerOscServer oscServer;
    BalancerOscClient oscServerDebugClient;
    void Start() {
        oscServer = gameObject.AddComponent<BalancerOscServer>();
        oscServer.Start("127.0.0.1", 1237, ProcessResponse);
        oscServerDebugClient = new BalancerOscClient("127.0.0.1", 1237);
        var uglyButNecessary = UnityThreadHelper.Dispatcher; // yes, removing this breaks things, thanks UnityThreadHelper
        requester = GetComponent<NetworkPredictionRequester>();
        if (!requester) {
            Debug.LogError("[NetworkResponseProcessor] No NetworkPredictionRequester found in this gameObject.");
        }
    }
    public void ProcessResponse(List<float> flattenedPathResponse) {
        int responseID = (int)flattenedPathResponse[0];
        flattenedPathResponse.RemoveAt(0);
        List<Vector3> pathResponse = ListOp.UnflattenNumpyOscDataToVector3s(flattenedPathResponse);
        if (visualizeResponses) {
            StartResponseVisualization(pathResponse, visualizationSourceObject);
        }
        ProcessTendencyResponse(pathResponse, responseID);
    }

    private Queue<MusclePrediction> predictions = new Queue<MusclePrediction>();
    private MusclePrediction incomplete = new MusclePrediction();
    private void ProcessTendencyResponse(List<Vector3> pathResponse, int responseID) {
        if (responseID == 0) {
            // rest response
            //Debug.Log("[NetworkResponseProcessor] Got a rest path response, ID: " + responseID);
            incomplete = new MusclePrediction();
            incomplete.restMuscleState = requester.requestMemory[0];
            incomplete.restMuscleStateResponse = pathResponse;

            if (visualizeResponseDifferences) {
                StartResponseVisualization(pathResponse, restVisualizationSourceObject);
            }
        }
        else {
            //Debug.Log("[NetworkResponseProcessor] Got an imaginary path response, ID: " + responseID);
            if (incomplete.restMuscleState != null) {
                incomplete.imagMuscleStateID = responseID;
                incomplete.imagMuscleStateResponse = pathResponse;

                var restState = incomplete.restMuscleState;
                var restResponse = incomplete.restMuscleStateResponse;
                var tendAndAligned = ComputeTendencyVector(pathResponse, restResponse);
                incomplete.tendencyVector = tendAndAligned.First;
                incomplete.alignedResponse = tendAndAligned.Second;
                predictions.Enqueue(new MusclePrediction(incomplete));
                incomplete = new MusclePrediction();
                incomplete.restMuscleState = restState;
                incomplete.restMuscleStateResponse = restResponse;

                UnityThreadHelper.Dispatcher.Dispatch(() => {
                    MusclePrediction prediction = predictions.Dequeue();
                    onMuscleResponsePrediction(prediction);
                });
            }
        }
    }



    //MuscleCommand restMuscleState = null;
    //public Vector3Sensor pathTendencySensor;
    //private bool imagUnchangedSinceRestChange = true;
    //private void RememberResponse(List<Vector3> pathResponse) {
    //    if (expectRest) {
    //        restMuscleStateResponse = pathResponse;
    //        if (visualizeResponseDifferences) {
    //            StartResponseVisualization(pathResponse, restVisualizationSourceObject);
    //        }
    //        expectRest = false;
    //        imagUnchangedSinceRestChange = true;
    //    }
    //    else {
    //        imagMuscleStateResponse = pathResponse;
    //        imagUnchangedSinceRestChange = false;
    //    }
    //}
    //private void ProcessTendencyResponse2(List<Vector3> pathResponse, int responseID) {
    //    RememberResponse(pathResponse);
    //    if (restMuscleState != null && restMuscleStateResponse != null
    //        && imagMuscleStateResponse != null) {
    //        if (imagUnchangedSinceRestChange) {
    //            return;
    //        }
    //        Vector3 tendencyVector = ComputeTendencyVector(imagMuscleStateResponse, restMuscleStateResponse);
    //        if (pathTendencySensor) {
    //            UnityThreadHelper.Dispatcher.Dispatch(() => {
    //                pathTendencySensor.SetSensorValue(tendencyVector);
    //            });
    //        }
    //        UnityThreadHelper.Dispatcher.Dispatch(() => {
    //            onTendencyVectorReceived(tendencyVector);
    //        });
    //    }
    //}


    public bool visualizeTendencyVectors = true;
    public Vector3Sensor tendencyVectorSensor;
    public Pair<Vector3, List<Vector3>> ComputeTendencyVector(List<Vector3> imagMuscleStateResponse, List<Vector3> restMuscleStateResponse) {
        List<Vector3> alignedSubtracted =
            ListOp.AlignedSubtractWithGain(imagMuscleStateResponse, restMuscleStateResponse, 1F, 1F);

        if (visualizeResponseDifferences) {
            StartResponseDifferenceVisualization(alignedSubtracted, differenceVisualizationSourceObject);
        }

        Vector3 sum = Vector3.zero;
        for (int i = 0; i < alignedSubtracted.Count; i++) {
            sum += alignedSubtracted[i];
        }
        if (visualizeTendencyVectors && tendencyVectorSensor) {
            tendencyVectorSensor.SetSensorValue(sum / alignedSubtracted.Count);
        }
        return new Pair<Vector3, List<Vector3>>(sum / alignedSubtracted.Count, alignedSubtracted);

        //Vector3 middleTendencyVector = alignedSubtracted[alignedSubtracted.Count / 2] - alignedSubtracted.First();
        //Vector3 lastTendencyVector = alignedSubtracted.Last() - alignedSubtracted.First();
        //return Vector3.Max(middleTendencyVector, lastTendencyVector);
    }

    public void PrintTheData(List<float> theData) {
        Debug.Log("[PrintTheData] " + ListOp.ListToString(theData));
    }
    public void StartResponseVisualization(List<Vector3> path, Transform sourceObject) {
        UnityThreadHelper.Dispatcher.Dispatch(() => {
            Visualization.VisualizePath(path,
                // it's important that this position is identical to the training origin:
                GameObject.Find("Nose Tracker").transform.position,
                sourceObject);
        });
    }
    public void StartResponseDifferenceVisualization(List<Vector3> path, Transform sourceObject) {
        UnityThreadHelper.Dispatcher.Dispatch(() => {
            Visualization.VisualizePath(path,
                GameObject.Find("Nose Tracker").GetComponent<Vector3Sensor>().GetSensorValue(),
                sourceObject);
        });
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.O)) {
            oscServerDebugClient.Send(new BalancerOscMessage("/debug"));
            Debug.Log("[NetworkResponseProcessor] Sent debug message to self.");
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            visualizeResponses ^= true;
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            visualizeResponseDifferences ^= true;
        }
    }

}