using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Globalization;
using CustomExtensions;

public class SenseRecorder : MonoBehaviour {

    [Header("Sense Group Matchers")]
    [Tooltip("Any matches are organized into the corresponding group.")]
    public string[] inputGroupMatchers;
    [Tooltip("Any matches are organized into the corresponding group.")]
    public string[] outputGroupMatchers;

    private List<int> inputGroupSenseIndices = new List<int>();
    private List<string> inputGroupSenseNames = new List<string>();
    private List<int> outputGroupSenseIndices = new List<int>();
    private List<string> outputGroupSenseNames = new List<string>();


    private Creature creature;
    void Start() {
        creature = Creature.GetCreature();

        // Scan the creature's senses and group their indices.
        {
            int i = 0;
            foreach (string senseName in creature.SenseMemoryMap.Keys) {
                if (senseName.ContainsAny(inputGroupMatchers)) {
                    inputGroupSenseIndices.Add(i);
                    inputGroupSenseNames.Add(creature.GetSenseMemoryName(i));
                }
                else if (senseName.ContainsAny(outputGroupMatchers)) {
                    outputGroupSenseIndices.Add(i);
                    outputGroupSenseNames.Add(creature.GetSenseMemoryName(i));
                }
                else {
                    //unsortedIndices.Add(i);
                    //unsortedSenseNames.Add(creature.GetSenseMemoryName(i));
                    Debug.LogWarning("[SenseRecorder] Ungrouped sense detected: " + senseName + " (won't record)");
                }
                i++;
            }
        }

        // Scan the sensory names and organize them alphabetically (swapping index storage as well).
        {
            List<int>[] allGroupSenseIndices = new List<int>[] { inputGroupSenseIndices, outputGroupSenseIndices };
            int gIdx = 0;
            foreach (List<string> groupSenseNames in new List<string>[] { inputGroupSenseNames, outputGroupSenseNames }) {
                // this is O(n^2) but n is expected to be small so there.
                // TODO: This will cause problems for launching with many, many senses.
                // TODO: Also this many operations and checks on strings overall is VERY BLEH.
                List<int> groupSenseIndices = allGroupSenseIndices[gIdx++];
                int swaps;
                do {
                    swaps = 0;
                    for (int i = 0, nx = 1; nx < groupSenseNames.Count; i = nx++) {
                        if (groupSenseNames[i].CompareTo(groupSenseNames[nx]) >= 0) {
                            // Swap!
                            string tempS = groupSenseNames[i];
                            groupSenseNames[i] = groupSenseNames[nx];
                            groupSenseNames[nx] = tempS;
                            // The whole point of writing out this shitty sorting algorithm was to track swaps
                            // and perform the same swaps on the sense index storage.
                            int tempI = groupSenseIndices[i];
                            groupSenseIndices[i] = groupSenseIndices[nx];
                            groupSenseIndices[nx] = tempI;

                            swaps++; // track number of swaps per pass.
                        }
                    }
                } while (swaps > 0); // Stop when we get a clean no-swap pass (means we're sorted).
            }
        }
    }


    // Writer State
    private long timeIndex = 0;
    private bool recording = true;
    private bool haveFile = false;
    private bool failureToGetFile = false;

    // Global Record Config
    [Header("Output Configuration")]
    public string writerDir = ".";
    public string sessionDir = "Untitled";
    private DateTime recordTime = DateTime.Now;
    private String RecordLabel { get { return recordTime.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture); } }
    private String RecordExtension { get { return ".csv"; } }
    public string SessionFullDir { get { return Path.Combine(writerDir, sessionDir); } }

    // Input Group Record Config
    public string inputRecordSuffix = "-IN";
    private string InputRecordFileName { get { return RecordLabel + inputRecordSuffix + RecordExtension; } }
    public string InputRecordPath { get { return Path.Combine(SessionFullDir, InputRecordFileName); } }

    // Output Group Record Config
    public string outputRecordSuffix = "-OUT";
    private string OutputRecordFileName { get { return RecordLabel + outputRecordSuffix + RecordExtension; } }
    public string OutputRecordPath { get { return Path.Combine(SessionFullDir, OutputRecordFileName); } }

    // Record Key File Config
    public bool writeKeyFile = true;
    private string RecordKeyFileName { get { return "0__" + sessionDir + "_RecordKey.txt"; } }
    private string RecordKeyPath { get { return Path.Combine(SessionFullDir, RecordKeyFileName); } }

    // Writer Buffers
    private StringBuilder inputWriteBuffer;
    private StringBuilder outputWriteBuffer;

    // Writer Interval State
    private int writeInterval = 100;
    private bool writeNow = false;
    private int timeIndexSinceWrite = 0;

    void FixedUpdate() {
        if (recording) {
            // init input and output files
            if (!haveFile && !failureToGetFile) {
                Directory.CreateDirectory(SessionFullDir);
                File.Create(InputRecordPath).Close();
                File.Create(OutputRecordPath).Close();
                if (File.Exists(InputRecordPath) && File.Exists(OutputRecordPath)) {
                    inputWriteBuffer = new StringBuilder();
                    outputWriteBuffer = new StringBuilder();
                    haveFile = true;
                }
                // Write out a session key file with the names of the senses being written.
                if (!File.Exists(RecordKeyPath)) {
                    StringBuilder key = new StringBuilder();
                    key.Append("INPUT\n");
                    int i = 0;
                    foreach (String name in inputGroupSenseNames) {
                        key.Append(name);
                        key.Append((i++ != inputGroupSenseNames.Count - 1)? ", " : "\n");
                    }
                    key.Append("OUTPUT\n");
                    i = 0;
                    foreach (String name in outputGroupSenseNames) {
                        key.Append(name);
                        key.Append((i++ != outputGroupSenseNames.Count - 1) ? ", " : "\n");
                    }
                    StreamWriter writer = new StreamWriter(File.Create(RecordKeyPath));
                    writer.Write(key.ToString());
                    writer.Close();
                }
            }
            else if (failureToGetFile) {
                Debug.LogError("[SenseRecorder] Failed to get file to write at: " + InputRecordPath + " or " + OutputRecordPath);
            }

            // write
            if (writeNow) {
                File.AppendAllText(InputRecordPath, inputWriteBuffer.ToString());
                File.AppendAllText(OutputRecordPath, outputWriteBuffer.ToString());
                inputWriteBuffer.Length = 0;
                outputWriteBuffer.Length = 0;
                writeNow = false;
                timeIndexSinceWrite = 0;
            }

            // accumulate
            if (!writeNow) {
                inputWriteBuffer.Append(GetCurrentTimeSliceDataForGroup(
                        inputGroupSenseIndices,
                        inputGroupSenseNames));
                outputWriteBuffer.Append(GetCurrentTimeSliceDataForGroup(
                        outputGroupSenseIndices,
                        outputGroupSenseNames));
            }

            // track time
            timeIndex++;
            timeIndexSinceWrite++;
            if (timeIndex < 0) {
                Debug.LogError("[SenseRecorder] Time Index overflow, stopping recording.");
                recording = false;
            }
            else if (timeIndexSinceWrite >= writeInterval) {
                writeNow = true;
                Debug.Log("[SenseRecorder] Writing.");
            }
        }
    }


    private string GetCurrentTimeSliceDataForGroup(
        List<int> groupSenseIndices,
        List<string> groupSenseNames) {

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < groupSenseIndices.Count; i++) {
            SenseMemory sense = creature.GetSense(groupSenseIndices[i]);
            string valueStr;
            valueStr = sense.Value.ToString();
//           StringBuilder ds = new StringBuilder();
//            ds.Append("Sense: ").Append(groupSenseNames[i]).Append(" = ").Append(valueStr);
//            Debug.Log(ds.ToString());
            string followSymbol = (i != groupSenseIndices.Count - 1) ? ", " : "\n"; // .csv
            sb.Append(valueStr + followSymbol);
        }
        return sb.ToString();
    }

}

