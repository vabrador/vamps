using UnityEngine;
using System.Collections.Generic;

public class MemoryAccumulatorVisualizer : MonoBehaviour {

	public MemoryAccumulator memAccum;

	[Header("Orientation")]
	public Vector3 xDir = Vector3.right;
	public Vector3 yDir = Vector3.up;
	public Vector3 timeDir = Vector3.forward;
	private float xScale = 1.0f;

	[Header("Per Cube")]
	public Material contractionCubeMaterial;
    public Material lengthCubeMaterial;
    public Material noseCubeMaterial;
    public float cubeScale = 0.5f;

	private Transform[] cubeTs;

    private Creature creature;
	void Start() {
        creature = Creature.GetCreature();

		xScale = transform.localScale.x;

		if (!memAccum) {
			memAccum = GetComponent<MemoryAccumulator> ();
			if (!memAccum) {
				Debug.LogError ("No MemoryAccumulator set to be visualized!");
			}
		}

		cubeTs = new Transform[memAccum.senses.Count * memAccum.memorySpan];

        for (int t = 0; t < memAccum.memorySpan; t++) {
            for (int i = 0; i < memAccum.senses.Count; i++) {
				GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
				cube.transform.SetParent (transform);
				cube.transform.localScale = Vector3.one * cubeScale;
				Destroy (cube.GetComponent<Collider> ());

                if (creature.GetSenseMemoryName(i).Contains("Contraction")) {
                    if (contractionCubeMaterial)
                        cube.GetComponent<Renderer>().material = contractionCubeMaterial;
                }
                else if (creature.GetSenseMemoryName(i).Contains("Length")) {
                    if (lengthCubeMaterial)
                        cube.GetComponent<Renderer>().material = lengthCubeMaterial;
                }
                else if (creature.GetSenseMemoryName(i).Contains("Nose")) {
                    if (noseCubeMaterial)
                        cube.GetComponent<Renderer>().material = noseCubeMaterial;
                }
                
                cubeTs [i + t * memAccum.senses.Count] = cube.transform;
			}
		}

	}

	void FixedUpdate() {
		for (int t = 0; t < memAccum.memorySpan; t++) {
            for (int i = 0; i < memAccum.senses.Count; i++) {
				float senseValue = memAccum.GetSenseValue (i, t);
                Transform cubeT = cubeTs [i + t * memAccum.senses.Count];
				cubeT.localPosition =
					((xDir * senseValue) + (yDir * i) + (timeDir * t))
					* xScale;	
			}
		}
	}

}
