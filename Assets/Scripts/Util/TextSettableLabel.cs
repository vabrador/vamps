using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextSettableLabel : MonoBehaviour {

    public void SetText(float value) {
        GetComponent<Text>().text = value.ToString();
    }
    public void SetText<T>(T value) {
        GetComponent<Text>().text = value.ToString();
    }

}
