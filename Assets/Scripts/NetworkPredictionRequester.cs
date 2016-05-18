using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

public class NetworkPredictionRequester : MonoBehaviour {

    public bool shouldPredict;
    public MuscleSequence lastPredictedSequence;

    Creature creature;
    BalancerOscClient oscClient;
    NetworkResponseProcessor responseProcessor;
    MuscleSequenceBuilder builder;
    MuscleSequenceUIBuilder uiBuilder;
    void Start() {
        creature = Creature.GetCreature();
        oscClient = new BalancerOscClient("127.0.0.1", 1236);
        Debug.Log("Starting server...");
        builder = creature.GetMuscleSequenceBuilder();
        uiBuilder = creature.GetMuscleSequenceUIBuilder();

        responseProcessor = GetComponent<NetworkResponseProcessor>();
    }
    
    private int numRandomRequestsPerInterval = 20;
    private int              sequenceLengths = 70;
    private float         predictionInterval = 0.200F; // seconds
    private float            predictionTimer = 0F;
    private float    interpredictionInterval = 0.08F; //0.08F
    private float       interpredictionTimer = 0F;
    private int         intervalRequestsSent = 0;
    private Vector3    positionAtRestRequest = Vector3.zero;
    private bool            needRestResponse = true;
    private float   resetRestRequestDistance = 3F;
    void Update() {
        if (Input.GetKeyDown(KeyCode.B)) {
            shouldPredict ^= true;
        }
        if (shouldPredict) {
            predictionTimer += Time.deltaTime;
            if (predictionTimer >= predictionInterval) {
                if (needRestResponse || (creature.GetNosePosition() - positionAtRestRequest).magnitude > resetRestRequestDistance) {

                    positionAtRestRequest = creature.GetNosePosition();
                    MuscleSequence idleSequence = builder.Duplicate(creature.GetCurrentMuscleCommand(), sequenceLengths);
                    RequestPrediction(idleSequence, idleSequence.Length, true);

                    needRestResponse = false;
                }

                interpredictionTimer += Time.deltaTime;
                if (interpredictionTimer >= interpredictionInterval) {
                    MuscleSequence uiImagSeq = builder.Duplicate(uiBuilder.GetUIMuscleState(), sequenceLengths);
                    RequestPrediction(uiImagSeq, sequenceLengths);
                    //MuscleSequence randomImagSeq = builder.BuildRandomStaticSequence(sequenceLengths);
                    //RequestPrediction(randomImagSeq, sequenceLengths);
                    interpredictionTimer = 0F;
                    intervalRequestsSent++;
                    if (intervalRequestsSent == numRandomRequestsPerInterval) {
                        intervalRequestsSent = 0;
                        interpredictionTimer = 0F;
                        predictionTimer = 0F;
                        needRestResponse = true;
                    }
                }
            }
        }
    }


    public Dictionary<int, MuscleSequence> requestMemory = new Dictionary<int, MuscleSequence>();
    private static int requestIDAccum = 1;
    private static int GetRequestID() {
        return requestIDAccum++;
    }

    private List<float> PrependShapeData(List<float> data, int num_rows, int num_cols) {
        List<float> toPrepend = new List<float>() { 2F, num_rows, num_cols };
        return toPrepend.Concat(data).ToList();
    }
    public SeedState GetSeed() {
        List<float> ctrSeed = PrependShapeData(ListOp.Invert(creature.GetCtrSeed(3), 18, 3), 18, 3);
        List<float> lenSeed = PrependShapeData(ListOp.Invert(creature.GetLenSeed(3), 18, 3), 18, 3);
        List<float> nosSeed = PrependShapeData(ListOp.Invert(creature.GetNoseSeed(3), 3, 3), 3, 3);
        SeedState seed = new SeedState();
        seed.ctrSeed = ctrSeed;
        seed.lenSeed = lenSeed;
        seed.nosSeed = nosSeed;
        return seed;
    }

    public int RequestPrediction(MuscleSequence ctrImagSeq, int seqLength, bool isRestPrediction=false) {
        SeedState seed = GetSeed();
        return RequestPrediction(ctrImagSeq, seqLength, seed, isRestPrediction);
    }

    public int RequestPrediction(MuscleSequence ctrImagSeq, int seqLength, SeedState seed, bool isRestPrediction=false) {

        int reqID = isRestPrediction? 0 : GetRequestID();

        BalancerOscMessage readyMsg = new BalancerOscMessage("/ready");
        readyMsg.Add(reqID);
        BalancerOscMessage ctrSeedMsg = new BalancerOscMessage("/predict");
        ctrSeedMsg.Add(seed.ctrSeed);
        BalancerOscMessage lenSeedMsg = new BalancerOscMessage("/predict");
        lenSeedMsg.Add(seed.lenSeed);
        BalancerOscMessage ctrImagMsg = new BalancerOscMessage("/predict");
        List<float> ctrImag = PrependShapeData(ListOp.Invert(ctrImagSeq.GetFlatList(), 18, seqLength), 18, seqLength);
        ctrImagMsg.Add(ctrImag);
        BalancerOscMessage noseSeedMsg = new BalancerOscMessage("/predict");
        noseSeedMsg.Add(seed.nosSeed);

        //BalancerOscBundle bundle = new BalancerOscBundle();
        //bundle.Add(readyMsg);
        //bundle.Add(ctrSeedMsg);
        //bundle.Add(lenSeedMsg);
        //bundle.Add(ctrImagMsg);
        //bundle.Add(noseSeedMsg);

        oscClient.Send(readyMsg);
        oscClient.Send(ctrSeedMsg);
        oscClient.Send(lenSeedMsg);
        oscClient.Send(ctrImagMsg);
        oscClient.Send(noseSeedMsg);

        requestMemory[reqID] = ctrImagSeq;
        return reqID;
    }


    
    //public List<int> RequestPrediction(List<MuscleSequence> ctrImagSeqs, int seqLengths) {
    //    return RequestPrediction(ctrImagSeqs, seqLengths, GetSeed());
    //}
    //public List<int> RequestPrediction(List<MuscleSequence> ctrImagSeqs, int seqLengths, SeedState seed) {
    //    List<int> requestIDs = new List<int>();
    //    BalancerOscBundle bundle = new BalancerOscBundle();
    //    foreach (MuscleSequence ctrImagSeq in ctrImagSeqs) {
    //        int reqID = GetRequestID();
    //        BalancerOscMessage readyMsg = new BalancerOscMessage("/ready");
    //        readyMsg.Add(reqID);
    //        BalancerOscMessage ctrSeedMsg = new BalancerOscMessage("/predict");
    //        ctrSeedMsg.Add(seed.ctrSeed);
    //        BalancerOscMessage lenSeedMsg = new BalancerOscMessage("/predict");
    //        lenSeedMsg.Add(seed.lenSeed);
    //        BalancerOscMessage ctrImagMsg = new BalancerOscMessage("/predict");
    //        List<float> ctrImag = PrependShapeData(ListOp.Invert(ctrImagSeq.GetFlatList(), 18, seqLengths), 18, seqLengths);
    //        ctrImagMsg.Add(ctrImag);
    //        BalancerOscMessage noseSeedMsg = new BalancerOscMessage("/predict");
    //        noseSeedMsg.Add(seed.nosSeed);

    //        bundle.Add(readyMsg);
    //        bundle.Add(ctrSeedMsg);
    //        bundle.Add(lenSeedMsg);
    //        bundle.Add(ctrImagMsg);
    //        bundle.Add(noseSeedMsg);

    //        requestMemory[reqID] = ctrImagSeq;
    //        requestIDs.Add(reqID);
    //    }
    //    oscClient.Send(bundle);
    //    return requestIDs;
    //}

}

public struct SeedState {
    public List<float> ctrSeed;
    public List<float> lenSeed;
    public List<float> nosSeed;
}
