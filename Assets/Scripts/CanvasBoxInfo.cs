using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasBoxInfo : MonoBehaviour
{

    private Text info;
    public Experiment experiment;
    // Start is called before the first frame update
    void Start()
    {
        info = this.GetComponent<Text>();

    }

    // Update is called once per frame
    void Update()
    {

        info.text = Application.dataPath;
        info.text += "\n";
        info.text += Application.persistentDataPath;
        info.text += "\n\n\n\n\n\n";

        //info.text = experiment.TrialInfo();
    }
}
