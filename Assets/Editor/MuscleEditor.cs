using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Muscle))]
[CanEditMultipleObjects]
public class MuscleEditor : Editor {
	
    public void OnSceneGUI() {
        Muscle muscle = target as Muscle;
		if (muscle) {
			Color relaxedColor = Color.cyan;
			Color tightColor = Color.red;
			Handles.color = Color.Lerp(relaxedColor, tightColor, ((float)muscle.NumFibersContracted / muscle.numFibers));

			int numFiberPoints = muscle.numFibers * 4;
			Vector3[] fiberSegments = new Vector3[numFiberPoints];
			bool leftSide = false;
			if (muscle.proximalTendon && muscle.distalTendon) {
				for (int i = 0; i < numFiberPoints; i += 4) {
					Vector3 rootpoint = muscle.proximalTendon.transform.position;
					Vector3 midpoint = (muscle.proximalTendon.transform.position + muscle.distalTendon.transform.position) * (1/2F);
					Vector3 endpoint = muscle.distalTendon.transform.position;
					fiberSegments[i] = rootpoint;
					fiberSegments[i + 1] = fiberSegments[i + 2] = midpoint
						+ (((-muscle.proximalTendon.transform.right - muscle.distalTendon.transform.right) / 2f).normalized
							//+ (Vector3.up
							* (leftSide? 1 : -1)
							* 0.01f * i);
					fiberSegments[i + 3] = endpoint;
					leftSide = !leftSide;
				}

				// Drawing
				Handles.DrawLines(fiberSegments);
			}
		}
    }

}
