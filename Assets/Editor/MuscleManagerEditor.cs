using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MuscleManager))]
public class MuscleManagerEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		if (GUILayout.Button("Reset/Init Muscles->Tendons")) {
			Debug.Log("Connecting muscles to similarly-named tendons...");
			int errors = 0;
			foreach (Muscle muscle in GameObject.FindObjectsOfType<Muscle>()) {
				int tendonCount = 0;
				Debug.Log("Found a muscle called " + muscle.gameObject.name);
				string proxName = muscle.gameObject.name + " Proximal Tendon";
				GameObject proximalTendonObj = GameObject.Find(proxName);
				if (proximalTendonObj) {
					Tendon proximalTendon = proximalTendonObj.GetComponent<Tendon>();
					if (proximalTendon) {
						muscle.proximalTendon = proximalTendon;
					}
					else {
						Debug.LogError("GameObject found with name " + proxName + " but it didn't have a Tendon component...");
					}
				}
				else {
					Debug.LogError("No proximal tendon found: " + proxName);
					errors++;
				}
				string distName = muscle.gameObject.name + " Distal Tendon";
				GameObject distalTendonObj = GameObject.Find(distName);
				if (distalTendonObj) {
					Tendon distalTendon = distalTendonObj.GetComponent<Tendon>();
					if (distalTendon) {
						muscle.distalTendon = distalTendon;
					}
					else {
						Debug.LogError("GameObject found with name " + distName + " but it didn't have a Tendon component...");
					}
				}
				else {
					Debug.LogError("No distal tendon found: " + distName);
					errors++;
				}
				if (tendonCount == 2) {
					muscle.UpdatePosition();
				}
			}
			Debug.Log("Muscle initialization completed with " + errors + " errors.");
        }
	}

}
