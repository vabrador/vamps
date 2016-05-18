using UnityEngine;
using System.Collections.Generic;

public class Vector3VisualizationOverTime : MonoBehaviour {

    public int timestep = 0;
    public bool playing = false;
    public List<Vector3> positionsOverTime = null;
    public Vector3 relativeTo = Vector3.zero;

    public void Load(List<Vector3> positionsOverTime) {
        this.positionsOverTime = positionsOverTime;
    }

    private bool unhideNextUpdate = false;
    public void Play() {
        unhideNextUpdate = true;
        playing = true;
    }

    public void Stop() {
        playing = false;
        Destroy(this.gameObject);
    }

    void FixedUpdate() {
        if (positionsOverTime != null) {
            if (playing) {
                if (unhideNextUpdate) {
                    GetComponentInChildren<Renderer>().enabled = true;
                    unhideNextUpdate = false;
                }
                if (timestep == positionsOverTime.Count) {
                    Stop();
                    GetComponentInChildren<Renderer>().enabled = false;
                    return;
                }
                transform.position = relativeTo + positionsOverTime[timestep++];
            }
        }
    }

}
