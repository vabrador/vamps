using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using CustomExtensions;

public class MemoryAccumulator : MonoBehaviour {

	public readonly int memorySpan = 256;

	public List<SenseMemory> senses = new List<SenseMemory>();
    private CircularList<float> timeSlices;

    private Creature creature;

    // Things that produce sensations call this in their Awake()
	// initialization to get a SenseMemory to which to write their sensory data.
    public SenseMemory RegisterSenseMemory(string name, SenseCoefficients senseCs) {
		GameObject newMemObj = new GameObject();
		newMemObj.name = name;
		newMemObj.transform.SetParent(transform);
        SenseMemory newMem;
        newMem = newMemObj.AddComponent<SenseMemory>();
        newMem.SenseCs = senseCs;
		senses.Add(newMem);
		return newMem;
	}

    void Awake() {
        creature = transform.GetComponentInParent<Creature>();
    }

	void Start() {
        // Construct the circular list for storing sense timeslices.
        // All senses better have been added before this Start() method is called!
        Debug.Log("sense count: " + senses.Count + "; mempory span: " + memorySpan);
		timeSlices = new CircularList<float>(senses.Count * memorySpan);
    }

	void FixedUpdate() {
        // The memory accumulator records a timeslice of each sense at every FixedUpdate().
		foreach (SenseMemory sense in senses) {
			timeSlices.Add(sense.Value);
		}
    }

    public SenseMemory GetSense(int senseIndex) {
        return senses[senseIndex];
    }

    public float GetSenseValue(int senseIndex, int memoryIndex) {
        return timeSlices.Get(senseIndex + memoryIndex * senses.Count);
    }

    public float GetLatestSenseValue(int senseIndex) {
        return GetSenseValue(senseIndex, memorySpan - 1);
    }

    public float[] GetLatestSenseValues() {
        return timeSlices.TailSlice(0, senses.Count);
    }
    public float[] GetLatestSenseValues(int numTimeslices) {
        return timeSlices.TailSlice(0, senses.Count * numTimeslices);
    }
    public List<float> GetLatestSenseValues(int numTimeslices, int startSenseIdx, int numSenses) {
        List<float> senseValues = new List<float>();
        for (int ts = 0; ts < numTimeslices; ts++) {
            senseValues.AddRange(timeSlices.TailSlice((ts * senses.Count + startSenseIdx),
                                                      (ts * senses.Count + startSenseIdx + numSenses)));
        }
        return senseValues;
    }
    public List<float> GetLatestSenseValues(int numTimeslices, List<string> senseNames) {
        IEnumerable<int> senseIndices = senseNames.Select(name => creature.GetSenseMemoryIndex(name));
        List<float> senseValues = new List<float>();
        for (int ts = numTimeslices - 1; ts >= 0; ts--) {
            foreach (int senseIdx in senseIndices) {
                //Debug.Log("sense index is " + senseIdx);
                senseValues.Add(timeSlices.GetFromEnd(ts * senses.Count + (senses.Count - 1) - senseIdx));
            }
        }
        return senseValues;
    }

    // If inverted, finds lowest sense value instead.
    /*public Memory FindHighestSenseValue(int senseIndex, bool inverted = false) {
        int highestValueTimeIdx = memorySpan;
        bool valueSet = false;
        float highestValue = 0;
        int resultInverter = (inverted ? -1 : 1);
        for (int t = memorySpan; t >= 0; t--) {
            float senseValue = GetSenseValue(senseIndex, t);
            if (senseValue * resultInverter > highestValue * resultInverter || !valueSet) {
                highestValueTimeIdx = t;
                highestValue = senseValue;
                valueSet = true;
            }
        }
        Memory memory = new Memory();
        foreach (string muscleLengthSenseName in creature.MuscleLengthMemoryMap.Keys) {
            int senseIdx = creature.MuscleLengthMemoryMap[muscleLengthSenseName];
            string muscleName = muscleLengthSenseName.Substring(0, muscleLengthSenseName.IndexOf(Muscle.LengthSenseSuffix));
            memory.MuscleLengths[creature.GetMuscle(muscleName)] = GetSenseValue(senseIdx, highestValueTimeIdx);
        }
        memory.GoalValue = GetSenseValue(senseIndex, highestValueTimeIdx);
        return memory;
    }
    public Memory FindLowestSenseValue(int senseIndex) {
        return FindHighestSenseValue(senseIndex, true);
    }*/


}
