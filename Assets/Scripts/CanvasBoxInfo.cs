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
    }

    // Update is called once per frame
    void Update() {
        //info.text = Application.dataPath;
        //info.text += Application.persistentDataPath;
        //info.text = experiment.TrialInfo();
        //info.text += "Refresh rate: "+ XRDevice.refreshRate;
        //info.text += "\n";
        if(experiment.Procedure != null) {
            TrialType? currentTrial = experiment.Procedure.CurrentDisplayTrial;
            if(currentTrial != null) {
                info.text = $"{experiment.Procedure.CurrentDisplayTrial.ReadableType} {experiment.Procedure.CurrentDisplayTrialID} ({(int) experiment.Procedure.CurrentProgressPercent}%)";
            } else {
                info.text = null;
            }
        }
        //info.text += "\n";
        //info.text += "\n";
    }
}
