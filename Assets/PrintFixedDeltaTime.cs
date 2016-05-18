using UnityEngine;
using System.Collections;

public class PrintFixedDeltaTime : MonoBehaviour {

    void FixedUpdate() {
        Debug.Log("FixedDeltaTime: " + Time.fixedDeltaTime);
    }

}
