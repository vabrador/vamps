using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

[Serializable]
public class Creature : MonoBehaviour {

    private static Creature lastCreature = null;
    public static Creature GetCreature() {
        return lastCreature;
    }
    
    private MemoryAccumulator memAccum;
    public MemoryAccumulator Memories {
        get { return memAccum; }
    }

    public MemoryAccumulatorVisualizer memVis;

    private Dictionary<string, int> senseMemoryMap;
    public Dictionary<string, int> SenseMemoryMap {
        get {
            if (senseMemoryMap == null) {
                senseMemoryMap = new Dictionary<string, int>();
            }
            return senseMemoryMap;
        }
    }
    private List<string> senseMemoryNames;
    public List<string> SenseMemoryNames {
        get {
            if (senseMemoryNames == null) {
                senseMemoryNames = new List<string>();
            }
            return senseMemoryNames;
        }
    }
    private Dictionary<string, int> muscleLengthMemoryMap;
    public Dictionary<string, int> MuscleLengthMemoryMap {
        get {
            if (muscleLengthMemoryMap == null) {
                muscleLengthMemoryMap = new Dictionary<string, int>();
            }
            return muscleLengthMemoryMap;
        }
    }
    private List<Muscle> muscles = new List<Muscle>();
    private Dictionary<string, Muscle> muscleMap;
    public Dictionary<string, Muscle> MuscleMap {
        get {
            if (muscleMap == null) {
                muscleMap = new Dictionary<string, Muscle>();
            }
            return muscleMap;
        }
    }
    private List<string> muscleNames;
    public List<string> MuscleNames {
        get {
            if (muscleNames == null) {
                muscleNames = new List<string>();
            }
            return muscleNames;
        }
    }

    private GameObject head;

    // Awake() is called before any Start() methods are called.
    void Awake() {
        lastCreature = this;
        
        // Build a MemoryAccumulator and put it in this Creature's Head.
        GameObject headObj = transform.FindChild("Head").gameObject;
        if (!headObj)
            Debug.LogError("No GameObject named Head found! Creatures gotta have a Head.");
        else {
            head = headObj;
            GameObject memAccumObj = new GameObject("Memory Accumulator");
            memAccumObj.transform.SetParent(head.transform);
            memAccum = memAccumObj.AddComponent<MemoryAccumulator>();
        }

        if (memVis) {
            memVis.memAccum = memAccum;
        }

    }

    public void RegisterMuscle(Muscle muscle) {
        muscles.Add(muscle);
        MuscleNames.Add(muscle.name);
        MuscleNames.Sort();
        MuscleMap[muscle.gameObject.name] = muscle;
    }

    int sensesRegistered = 0;
    /// <summary>SenseCoefficients should be null unless you plan to SenseMemory.UseRealValue.</summary>
    public SenseMemory RegisterSense(string name, SenseCoefficients senseCs) {
        if (!memAccum) {
            Debug.LogError("[Creature] No MemoryAccumulator associated with this Creature; unable to register sense " + name);
            return null;
        }
        SenseMemoryMap[name] = sensesRegistered;
        SenseMemoryNames.Add(name);
        if (name.Contains("Muscle") && name.Contains("Length")) {
            MuscleLengthMemoryMap[name] = sensesRegistered;
        }
        sensesRegistered++;
        return memAccum.RegisterSenseMemory(name, senseCs);
    }

    public GameObject GetHead() {
        return head;
    }

    public Muscle GetMuscle(string name) {
        if (!MuscleMap.ContainsKey(name)) {
            Debug.LogError("[Creature] No muscle found called " + name);
            return null;
        }
        return MuscleMap[name];
    }
    public Muscle[] GetMuscles() {
        return muscles.ToArray();
    }
    public string[] GetMuscleNames() {
        return MuscleNames.ToArray();
    }
    public void ExecuteMuscleCommand(MuscleCommand command) {
        for (int i = 0; i < command.MuscleNames.Length; i++) {
            string muscleName = command.MuscleNames[i];
            float contractionInput = command.ContractionInputs[i];
            Muscle toSet = GetMuscle(muscleName);
            if (toSet) {
                toSet.SetNumFibersToContract(contractionInput);
            }
        }
    }
    public MuscleCommand GetMuscleState() {
        MuscleCommand muscleState = new MuscleCommand();
        muscleState.MuscleNames = MuscleNames.ToArray();
        muscleState.ContractionInputs = MuscleNames.Select((name) => MuscleMap[name].NumFibersContracted).ToArray();
        return muscleState;
    }

    public SenseMemory GetSense(string name) {
        if (!SenseMemoryMap.ContainsKey(name)) {
            Debug.LogError("[Creature] No sense found called " + name);
            return null;
        }
        return GetSense(SenseMemoryMap[name]);
    }
    public SenseMemory GetSense(int senseIndex) {
        return memAccum.GetSense(senseIndex);
    }

    public float GetSenseValue(string name) {
        if (!SenseMemoryMap.ContainsKey(name)) {
            Debug.LogError("[Creature] No sense found called " + name);
            return 0;
        }
        return memAccum.GetLatestSenseValue(SenseMemoryMap[name]);
    }

    public int GetSenseMemoryIndex(string name) {
        if (!SenseMemoryMap.ContainsKey(name)) {
            Debug.LogError("[Creature] No sense found called " + name);
            return 0;
        }
        return SenseMemoryMap[name];
    }

    public string GetSenseMemoryName(int index) {
        return SenseMemoryNames[index];
    }

    public AxisSenseMemory[] GetAccelerationSenses() {
        List<AxisSenseMemory> accelSenses = new List<AxisSenseMemory>();
        foreach (SenseMemory sense in memAccum.senses) {
            if (sense.name.Contains("Acceleration")) {
                AxisSenseMemory axisSense = sense as AxisSenseMemory;
                if (axisSense) {
                    accelSenses.Add(axisSense);
                }
            }
        }
        return accelSenses.ToArray();
    }


    // For now, use statically defined muscle names list
    private List<string> LengthNames = new List<string>() {
        "Body->Neck Muscle 1 Length Sense",
        "Body->Neck Muscle 2 Length Sense",
        "Body->Neck Muscle 3 Length Sense",
        "Body->Neck Muscle 4 Length Sense",
        "Body->Neck Muscle 5 Length Sense",
        "Body->Neck Muscle 6 Length Sense",
        "Neck->Head Muscle 1 Length Sense",
        "Neck->Head Muscle 2 Length Sense",
        "Neck->Head Muscle 3 Length Sense",
        "Neck->Head Muscle 4 Length Sense",
        "Neck->Head Muscle 5 Length Sense",
        "Neck->Head Muscle 6 Length Sense",
        "Neck->Head Twist Muscle 1 Length Sense",
        "Neck->Head Twist Muscle 2 Length Sense",
        "Neck->Head Twist Muscle 3 Length Sense",
        "Neck->Head Twist Muscle 4 Length Sense",
        "Neck->Head Twist Muscle 5 Length Sense",
        "Neck->Head Twist Muscle 6 Length Sense",
    };
    public List<float> GetLenSeed(int seedLength) {
        // Ask memAccum for the latest neck muscle sense arrays
        List<float> rawSeed = memAccum.GetLatestSenseValues(seedLength, LengthNames);
        return rawSeed.Select(i => (float)i).ToList();
    }


    // For now, use statically defined muscle names list
    private List<string> CtrNames = new List<string>() {
        "Body->Neck Muscle 1 Contraction Sense",
        "Body->Neck Muscle 2 Contraction Sense",
        "Body->Neck Muscle 3 Contraction Sense",
        "Body->Neck Muscle 4 Contraction Sense",
        "Body->Neck Muscle 5 Contraction Sense",
        "Body->Neck Muscle 6 Contraction Sense",
        "Neck->Head Muscle 1 Contraction Sense",
        "Neck->Head Muscle 2 Contraction Sense",
        "Neck->Head Muscle 3 Contraction Sense",
        "Neck->Head Muscle 4 Contraction Sense",
        "Neck->Head Muscle 5 Contraction Sense",
        "Neck->Head Muscle 6 Contraction Sense",
        "Neck->Head Twist Muscle 1 Contraction Sense",
        "Neck->Head Twist Muscle 2 Contraction Sense",
        "Neck->Head Twist Muscle 3 Contraction Sense",
        "Neck->Head Twist Muscle 4 Contraction Sense",
        "Neck->Head Twist Muscle 5 Contraction Sense",
        "Neck->Head Twist Muscle 6 Contraction Sense",
    };
    public List<float> GetCtrSeed(int seedLength) {
        // Ask memAccum for the latest neck muscle sense arrays
        List<float> rawSeed = memAccum.GetLatestSenseValues(seedLength, CtrNames);
        return rawSeed.Select(i => (float)i).ToList();
    }


    // For now, use statically defined muscle names list
    private List<string> NoseNames = new List<string>() {
        "Nose Tracker X Sense",
        "Nose Tracker Y Sense",
        "Nose Tracker Z Sense",
    };
    public List<float> GetNoseSeed(int seedLength) {
        // Ask memAccum for the latest nose position vector sense arrays
        List<float> rawSeed = memAccum.GetLatestSenseValues(seedLength, NoseNames);
        return rawSeed.ToList();
    }


    public Canvas canvas;
    public List<Slider> muscleContractionSliderBindings;
    public Slider.SliderEvent sliderEvent;
    public void InitControlPanel() {
        foreach (string muscleName in MuscleNames) {
            var newSlider = MakeChild(muscleName + " Contraction Slider", canvas.gameObject).AddComponent<Slider>();
            //newSlider.onValueChanged = sliderEvent;
            //sliderEvent.AddListener()
            //GetMuscle(muscleName).
        }
    }
    public void SliderValueChanged(float value) {

    }

    public GameObject MakeChild(string name, GameObject parent) {
        var child = new GameObject().GetComponent<Transform>().parent = parent.GetComponent<Transform>();
        child.name = name;
        return child.gameObject;
    }

    private MuscleSequenceBuilder msb = null;
    public MuscleSequenceBuilder GetMuscleSequenceBuilder() {
        if (!msb) {
            msb = gameObject.AddComponent<MuscleSequenceBuilder>();
        }
        return msb;
    }
    private MuscleSequenceUIBuilder msuib = null;
    public MuscleSequenceUIBuilder GetMuscleSequenceUIBuilder() {
        if (!msuib) {
            msuib = GameObject.FindObjectOfType<MuscleSequenceUIBuilder>();
        }
        return msuib;
    }


    public NetworkPredictionRequester GetNetworkPredictionRequester() {
        return GetComponentInChildren<NetworkPredictionRequester>();
    }
    public NetworkResponseProcessor GetNetworkResponseProcessor() {
        return GetComponentInChildren<NetworkResponseProcessor>();
    }

    public MuscleCommand GetCurrentMuscleCommand() {
        MuscleCommand command = new MuscleCommand();
        command.MuscleNames = muscleNames.ToArray();
        int i = 0;
        float[] contractionInputs = new float[muscles.Count];
        foreach (String s in muscleNames) {
            var muscle = MuscleMap[s];
            contractionInputs[i++] = muscle.NumFibersContracted;
        }
        command.ContractionInputs = contractionInputs;
        return command;
    }

    public float noseSpeed = 0F;
    public Vector3 nosePositionLastInterval;
    public Vector3 nosePositionThisInterval;
    private float noseVelocityTimer = 0F;
    private float noseVelocityInterval = 0.05F;
    public Vector3Sensor nosePositionSensor;
    void Start() {
        nosePositionSensor = GameObject.Find("Nose Tracker").GetComponent<Vector3Sensor>();
    }
    private bool initLastPosition = true;
    void Update() {
        noseVelocityTimer += Time.deltaTime;
        if (noseVelocityTimer >= noseVelocityInterval) {
            nosePositionThisInterval = nosePositionSensor.GetSensorValue();
            if (initLastPosition) {
                nosePositionLastInterval = nosePositionThisInterval;
                initLastPosition = false;
            }
            noseSpeed = (nosePositionLastInterval - nosePositionThisInterval).magnitude / noseVelocityTimer;
            nosePositionLastInterval = nosePositionThisInterval;
            noseVelocityTimer = 0F;
        }
    }
    public float GetNoseSpeed() {
        return noseSpeed;
    }
    public Vector3 GetNosePosition() {
        return nosePositionSensor.GetSensorValue();
    }

}
