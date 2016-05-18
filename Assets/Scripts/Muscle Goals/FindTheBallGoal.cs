using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CustomExtensions;

public class FindTheBallGoal : MonoBehaviour {

    public Transform theBall;
    public Creature creature;
    public Vector3Sensor nosePositionSensor;
    private NetworkPredictionRequester requester;
    private NetworkResponseProcessor predictionReceiver;
    private MuscleSequencePlayer player = null;
    private MuscleSequenceBuilder builder = null;
    public bool seekingTheBall = false;
    void Start() {
        creature = Creature.GetCreature();
        requester = creature.GetNetworkPredictionRequester();
        predictionReceiver = creature.GetNetworkResponseProcessor();
        predictionReceiver.onMuscleResponsePrediction += ProcessResponse;
        player = creature.GetComponentInChildren<MuscleSequencePlayer>();
        builder = creature.GetMuscleSequenceBuilder();
        if (!nosePositionSensor) {
            Debug.LogError("[FindTheBallGoal] No nose position sensor attached.");
        }
        if (!player) {
            Debug.LogError("[FindTheBallGoal] No MuscleSequencePlayer in creature.");
        }
    }

    //private float timeInNose = 0;
    //void OnTriggerStay(Collider other) {
    //    if (other.name == "The Ball") {
    //        Debug.Log("Staying in the ball!");
    //        timeInNose += Time.deltaTime;
    //        if (timeInNose >= 2.0F) {
    //            Debug.Log("FOUND IT, HALTING");
    //            enabled = false;
    //        }
    //    }
    //}
    //void OnTriggerExit(Collider other) {
    //    if (other.name == "The Ball") {
    //        Debug.Log("Aww, left the ball. Starting again!");
    //        enabled = true;
    //        timeInNose = 0;
    //    }
    //}
    
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
        if (Input.GetKeyDown(KeyCode.N)) {
            seekingTheBall ^= true;
            if (!seekingTheBall) responses.Clear();
        }
        if (seekingTheBall) {
            predictionTimer += Time.deltaTime;
            if (predictionTimer >= predictionInterval) {
                if (needRestResponse || (creature.GetNosePosition() - positionAtRestRequest).magnitude > resetRestRequestDistance) {

                    positionAtRestRequest = creature.GetNosePosition();
                    MuscleSequence idleSequence = builder.Duplicate(creature.GetCurrentMuscleCommand(), sequenceLengths);
                    requester.RequestPrediction(idleSequence, idleSequence.Length, true);

                    needRestResponse = false;
                }

                interpredictionTimer += Time.deltaTime;
                if (interpredictionTimer >= interpredictionInterval) {
                    MuscleSequence randomImagSeq = builder.BuildRandomStaticSequence(sequenceLengths);
                    requester.RequestPrediction(randomImagSeq, sequenceLengths);
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

    public MuscleSequence currentSequence = null;
    List<MusclePrediction> responses = new List<MusclePrediction>();
    public int responsesCountBeforeDecision = 200;
    private float timeWaited = 0F;
    private MuscleSequence bestSoFar = null;
    private float bestSoFarDist = -1F;
    private void ProcessResponse(MusclePrediction response) {
        if (!seekingTheBall) {
            return;
        }

        float goalDistance = CalcGoalDistanceFromNose(response.alignedResponse[response.alignedResponse.Count - 1]);
        if (bestSoFar == null || goalDistance < bestSoFarDist) {
            bestSoFar = requester.requestMemory[response.imagMuscleStateID];
            bestSoFarDist = goalDistance;
        }

        if (bestSoFarDist < timeWaited) {
            player.LoadSequence(bestSoFar);
            player.Play();
            timeWaited = 0;
            bestSoFar = null;
            bestSoFarDist = -1;
            MoveTheBall();
        }

        timeWaited += Time.deltaTime;
        theBall.localScale = new Vector3(timeWaited, timeWaited, timeWaited);

    }
    //Debug.Log("Nose speed: " + creature.GetNoseSpeed());
    //if (creature.GetNoseSpeed() > 5F
    //    && 1 / (creature.GetNoseSpeed() / 5F) * 20F < responses.Count) {
    //    // Discard results if our head is moving too quickly.
    //    var optimalSequence = DecideOptimalSequence(responses);
    //    if (optimalSequence != null) {
    //        currentSequence = optimalSequence;
    //        player.LoadSequence(optimalSequence).Play();
    //        return;
    //    }
    //    responses.Clear();
    //}
    //else {
    //}


    //responses.Add(response);


    //        if (responses.Count > responsesCountBeforeDecision) {
    //            var optimalSequence = DecideOptimalSequence(responses);
    //            Debug.Log("Playing calculated optimal sequence.");
    //            currentSequence = optimalSequence;
    //            if (optimalSequence != null) {
    //                player.LoadSequence(optimalSequence).Play();
    //                responses.Clear();
    //            }
    //            else {
    //                Debug.Log("No optimal sequence (better than current nose position anyway).");
    //            }
    //        }




    //        Debug.Log("Current distance: " + (creature.GetNosePosition() - theBall.transform.position).magnitude);
    //        float happiness = (creature.GetNosePosition() - theBall.transform.position).magnitude.ZMap(0F, 9F, 1F, 0F);
    //        Debug.Log("Current happiness: " + happiness);
    //        responses.Add(response);
    //        if (responses.Count >= 40 * (happiness * happiness).ZMap(0, 1, -20, 20)) {
    //            Debug.Log("READY");
    //            //DecideMovement(responses.Select((x) => x.tendencyVector).ToList());
    //            var optimalSequence = DecideOptimalSequence(responses);
    //            if (optimalSequence != null) {
    //                List<float> resistMovingWeights = new List<float>() { happiness, 1F-happiness };
    //                if (currentSequence != null) {
    //                    optimalSequence = builder.BuildWeightedAveragedSequence(
    //                        new List<MuscleSequence>() { currentSequence, optimalSequence }, resistMovingWeights);
    //                }
    //                currentSequence = optimalSequence;
    //                player.LoadSequence(optimalSequence).Play();
    //                responses.Clear();
    //            }
    //        }
    //        Debug.Log("Got response.");

    private float CalcGoalDistance(Vector3 worldPositionVec) {
        return (theBall.position - worldPositionVec).magnitude;
    }
    private float CalcGoalDistanceFromNose(Vector3 vecFromNose) {
        return (theBall.position - (creature.GetNosePosition() + vecFromNose)).magnitude;
    }


    private Transform theBallSpawnRoot = null;
    private void MoveTheBall() {
        if (!theBallSpawnRoot) {
            theBallSpawnRoot = GameObject.Find("TheBallSpawnRoot").transform;
        }
        // 3D
        theBall.position = theBallSpawnRoot.position + Random.insideUnitSphere * 5F; //3D
        // 2D
        //Vector2 r = Random.insideUnitCircle;
        //theBall.position = theBallSpawnRoot.position + new Vector3(0F, r.x, r.y) * 5F;
    }


    private MuscleSequence DecideOptimalSequence(List<MusclePrediction> predictions) {
        // obtain a score signal by scoring each prediction with a chosen function.
        List<float> scores = predictions.Select((x) => ComputeEndClosenessScore(x.alignedResponse)).Where((x) => x > 0).ToList();
        if (scores.Count == 0) {
            return null;
        }
        else {
            Debug.Log("Got " + scores.Count + " scores for sequences better than the current nose position");
        }
        //Debug.Log("[DecideMovenent2] " + scores.Count + " scores: " + ListOp.ListToString(scores));
        // attempt to remove noise from the signal
        List<Pair<float, int>> promisingScores = RemoveNoise(scores);
        List<float> promisingScoreValues = promisingScores.Select((x) => x.First).ToList();
        List<int>   promisingScoreIndices = promisingScores.Select((x) => x.Second).ToList();
        if (promisingScores.Count != 0) {
            List<float> weights = DivideBySum(promisingScoreValues).ToList();
            List<MuscleSequence> sequences =
                weights.Select((x, i) =>
                    requester.requestMemory[predictions[i].imagMuscleStateID]).ToList();
            //Debug.Log("[DecideMovement2] " + weights.Count + " non-noise weights: " + ListOp.ListToString(weights));
            MuscleSequence optimalSequence = builder.BuildWeightedAveragedSequence(sequences, weights);
            return optimalSequence;
        }
        else {
            //Debug.Log("[DecideMovement2] No signal detected over the noise floor.");
            return null;
        }
    }
    private List<Pair<float, int>> Identity(List<float> scores) {
        List<Pair<float, int>> signals = new List<Pair<float, int>>();
        for (int i = 0; i < scores.Count; i++) {
            signals.Add(new Pair<float, int>(scores[i], i));
        }
        return signals;
    }
    private List<Pair<float, int>> RemoveNoise(List<float> scores) {
        float averageWidth = 0;
        float averageValue = 0;
        for (int i = 0; i < scores.Count; i++) {
            averageValue += scores[i];
            if (i > 0) {
                averageWidth += Mathf.Abs(scores[i] - scores[i - 1]);
            }
        }
        averageWidth /= scores.Count;
        averageValue /= scores.Count;
        //Debug.Log("Average width: " + averageWidth);
        //Debug.Log("Average value: " + averageValue);

        List<Pair<float, int>> signals = new List<Pair<float, int>>();
        for (int i = 0; i < scores.Count; i++) {
            if (scores[i] - averageValue - averageWidth > 0) {
                signals.Add(new Pair<float, int>(scores[i], i));
            }
        }
        return signals;
    }
    private IEnumerable<float> DivideBySum(IEnumerable<float> values) {
        float sum = values.Sum();
        return values.Select((x) => x / sum);
    }

    //public float currentlyPlayingTotalScore = -1;
    //private float scoreDecayPerInterval = 0.1F;
    //public void DecideMovement(List<Vector3> tendencyVectors) {
    //    List<float> scores = new List<float>();
    //    float scoreSum = 0;
    //    for (int i = 0; i < tendencyVectors.Count; i++) {
    //        Vector3 tendencyVector = tendencyVectors[i];
    //        scores.Add(ComputeTendencyScore(tendencyVector));
    //        scoreSum += scores[i];
    //    }

    //    List<MuscleSequence> tendencySeqs = responses.Select((x) => predictionRequester.requestMemory[x.imagMuscleStateID]).ToList();
    //    List<float> weights = scores.Select((x) => x / scoreSum).ToList();
    //    if (scoreSum > currentlyPlayingTotalScore) {
    //        Debug.Log("Found better solution, overwriting play: " + scoreSum + " > " + currentlyPlayingTotalScore);
    //        MuscleSequence optimalSequence = builder.BuildWeightedAveragedSequence(tendencySeqs, weights);
    //        currentlyPlayingTotalScore = scoreSum;
    //        lastChosenSequence = optimalSequence;
    //        player.LoadSequence(optimalSequence);
    //        player.Play();
    //    }
    //    else if (player.IsDone() && lastChosenSequence != null) {
    //        Debug.Log("Refreshed older sequence. " + currentlyPlayingTotalScore * scoreDecayPerInterval + " > " + scoreSum);
    //        currentlyPlayingTotalScore *= scoreDecayPerInterval;
    //        player.LoadSequence(lastChosenSequence);
    //        player.Play();
    //    }
    //    else {
    //        Debug.Log("Not overwriting play.");
    //    }
    //}


    private float ComputeEndClosenessScore(List<Vector3> alignedPath) {
        float distance = (theBall.position - (nosePositionSensor.GetSensorValue() + alignedPath[alignedPath.Count - 1])).sqrMagnitude;
        //float betterThanNose = (theBall.position - (nosePositionSensor.GetSensorValue())).sqrMagnitude > distance;
        if (distance == 0) {
            return float.MaxValue;
        }
        //else if (!betterThanNose) {
        //    return -1;
        //}
        else {
            return 1 / Mathf.Pow(distance, 2);
        }
    }

    private float ComputeTendencyScore(Vector3 tendencyVector) {
        var distance = (theBall.position - (nosePositionSensor.GetSensorValue() + tendencyVector)).magnitude;
        if (distance == 0) {
            return float.MaxValue;
        }
        else {
            return 1 / Mathf.Pow(distance, 2);
        }
    }

    private bool IsBetter(Vector3 tendencyVector1, Vector3 tendencyVector2) {
        Vector3 ballWorldPosition = theBall.transform.position;
        Vector3 noseWorldPosition = nosePositionSensor.GetSensorValue();
        Vector3 tv1Pos = noseWorldPosition + tendencyVector1;
        Vector3 tv2Pos = noseWorldPosition + tendencyVector2;
        if ((ballWorldPosition - noseWorldPosition - tv1Pos).sqrMagnitude
            < (ballWorldPosition - noseWorldPosition - tv2Pos).sqrMagnitude) {
            return true;
        }
        else {
            return false;
        }
    }
    private float sqrMagDistance(Vector3 f) {
        return 0;
    }
}
