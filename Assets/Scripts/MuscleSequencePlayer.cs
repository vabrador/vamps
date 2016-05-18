using UnityEngine;
using System.Collections;
using System.IO;

public class MuscleSequencePlayer : MonoBehaviour {

    public bool playOnStart = false;
    public string dataFolderPath;
    public string actionLengthsFilename;
    public string ActionLengthsPath { get { return Path.Combine(dataFolderPath, actionLengthsFilename); } }
    public string muscleNamesFilename;
    public string MuscleNamesPath { get { return Path.Combine(dataFolderPath, muscleNamesFilename); } }
    public string contractionInputsFilename;
    public string ContractionInputsPath { get { return Path.Combine(dataFolderPath, contractionInputsFilename); } }

    private Creature creature;
    void Start() {
        creature = Creature.GetCreature();

        if (playOnStart) {
            LoadSequence(MuscleSequence.FromCSV(ActionLengthsPath, MuscleNamesPath, ContractionInputsPath));
            Play();
        }
    }

    private MuscleSequence sequence = null;
    public MuscleSequence Sequence { get { return this.sequence; } }
    public bool SequenceLoaded { get { return sequence != null; } }
    public MuscleSequencePlayer LoadSequence(MuscleSequence sequence) {
        playing = false;
        this.sequence = sequence;
        sequenceLength = sequence.Length;
        return this;
    }

    private int currentFrame = 0;
    private int sequenceLength = 0;
    public void Play() {
        if (!SequenceLoaded) {
            Debug.LogError("[MuscleSequencePlayer] Cannot play, not sequence loaded.");
            return;
        }
        currentFrame = 0;
        playing = true;
    }

    private bool playing = false;
    void FixedUpdate() {
        if (playing) {
            if (currentFrame == sequenceLength) {
                playing = false;
            }
            else {
                MuscleCommand currentCommand = sequence.GetCommand(currentFrame);
                if (currentCommand == null) {
                    playing = false;
                    Debug.LogError("[MuscleSequencePlayer] Got null command at frame " + currentFrame + "; stopping.");
                }
                else {
                    creature.ExecuteMuscleCommand(currentCommand);
                }
                currentFrame += 1;
            }
        }
    }

    public bool IsDone() {
        return !playing;
    }

}
