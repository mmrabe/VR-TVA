using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class CanvasBoxInfo : MonoBehaviour
{

    private Text info;
    public Experiment experiment;

    // Start is called before the first frame update
    void Start() {
        info = this.GetComponent<Text>();
        OVRPlugin.systemDisplayFrequency = 120.0f;
    }

    // Update is called once per frame
    void Update() {
        //info.text = Application.dataPath;
        //info.text += Application.persistentDataPath;
        info.text = experiment.TrialInfo();
        info.text += "Refresh rate: "+ XRDevice.refreshRate;
        info.text += "\n";
        info.text += experiment.getTrialsSoFar().ToString();
        info.text += "\n";
        info.text += "\n";
    }
}
