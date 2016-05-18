using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MuscleSequenceUIBuilder : MonoBehaviour {

    private List<Slider> imaginedMuscleStateSliders = new List<Slider>();
    private Creature creature;
    void Start () {
        creature = Creature.GetCreature();

        foreach (string muscleName in creature.GetMuscleNames()) {
            var sliderGroupObj = GameObject.Find("Imagined " + muscleName + " Slider Group");
            if (!sliderGroupObj) {
                Debug.LogError("[MuscleSequenceUIBuilder] No slider group found called " + "Imagined " + muscleName + " Slider Group");
            }
            Slider imaginedMuscleStateSlider = sliderGroupObj.GetComponentInChildren<Slider>();
            imaginedMuscleStateSliders.Add(imaginedMuscleStateSlider);
        }
        Debug.Log("[MuscleSequenceUIBuilder] OK, got sliders.");
        Debug.Log(ListOp.ListToString(imaginedMuscleStateSliders));
    }

    private MuscleCommand currentUIMuscleState = new MuscleCommand();
	void FixedUpdate () {
        currentUIMuscleState.MuscleNames = creature.GetMuscleNames();
        currentUIMuscleState.ContractionInputs = imaginedMuscleStateSliders.Select(x => x.value*10).ToArray();
        if (Input.GetKeyDown(KeyCode.C)) {
            Debug.Log("[MuscleSequenceUIBuilder] Current UI Muscle State: " + currentUIMuscleState);
        }
	}
    public MuscleCommand GetUIMuscleState() {
        return currentUIMuscleState;
    }
}
