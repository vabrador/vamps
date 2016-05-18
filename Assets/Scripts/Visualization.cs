using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class Visualization {

    public static void VisualizePath(List<Vector3> vector3s, Vector3 relativeTo, Transform sourceVisualizationObject) {
        var newVisual = Transform.Instantiate(sourceVisualizationObject).gameObject.AddComponent<Vector3VisualizationOverTime>();
        newVisual.GetComponentInChildren<Renderer>().enabled = false; // otherwise it will appear at the origin before playing
        newVisual.relativeTo = relativeTo;
        newVisual.Load(vector3s);
        newVisual.Play();
    }

    public static void VisualizePath(List<Vector3> vector3s, Vector3 relativeTo, List<Vector3> offsetVector3s, Transform sourceVisualizationObject) {
        if (vector3s.Count != offsetVector3s.Count) {
            Debug.LogError("[Visualization.Visualize] vector3s and offsetVector3s must be the same length.");
            return;
        }

        var newVisual = Transform.Instantiate(sourceVisualizationObject).gameObject.AddComponent<Vector3VisualizationOverTime>();
        newVisual.GetComponentInChildren<Renderer>().enabled = false; // otherwise it will appear at the origin before playing
        newVisual.relativeTo = relativeTo;
        List<Vector3> subtracted = vector3s.Select((vector3, index) =>  vector3 - offsetVector3s[index] ).ToList();
        newVisual.Load(subtracted);
        newVisual.Play();
    }

}
