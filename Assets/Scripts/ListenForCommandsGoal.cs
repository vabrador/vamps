using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ListenForCommandsGoal : MonoBehaviour {

    public Creature creature;
    public MuscleSequencePlayer player;
    public MuscleSequenceBuilder builder;
    public bool buildGamutOnStart = false;
    public MuscleSequence gamutSequence = null;
    void Start() {
        creature = Creature.GetCreature();
        player = creature.GetComponentInChildren<MuscleSequencePlayer>();
        builder = creature.GetMuscleSequenceBuilder();
        if (buildGamutOnStart) {
            gamutSequence = builder.BuildGamutSequence(); // This takes a while!
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.H)) {
            GameObject.Find("Help Text").GetComponent<Text>().enabled ^= true;
            GameObject.Find("Help Text Panel").GetComponent<Image>().enabled ^= true;
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            player.LoadSequence(builder.BuildZeroSequence());
            player.Play();
        }
        if (Input.GetKeyDown(KeyCode.F)) {
            var newMuscleState = creature.GetComponentInChildren<MuscleSequenceUIBuilder>().GetUIMuscleState();
            player.LoadSequence(builder.Duplicate(newMuscleState, 50));
            player.Play();
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            GameObject.Find("Screen Overlay Canvas").GetComponent<Canvas>().enabled = !GameObject.Find("Screen Overlay Canvas").GetComponent<Canvas>().enabled;
        }
        if (Input.GetKeyDown(KeyCode.G)) {
            if (gamutSequence == null) {
                gamutSequence = builder.BuildGamutSequence(); // This takes a while!
            }
            else {
                player.LoadSequence(gamutSequence);
                player.Play();
            }
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            gameObject.GetComponent<MuscleRandomExploreGoal>().enabled = !gameObject.GetComponent<MuscleRandomExploreGoal>().enabled;
        }
        if (Input.GetKeyDown(KeyCode.W)) {
            var recorder = GameObject.FindObjectOfType<SenseRecorder>();
            recorder.enabled = !recorder.enabled;
            var indicator = GameObject.Find("RecordIndicator").GetComponent<Image>();
            indicator.enabled = !indicator.enabled;
        }
    }


}
