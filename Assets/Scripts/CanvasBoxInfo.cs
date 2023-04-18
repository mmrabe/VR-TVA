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
        //info.text = experiment.TrialInfo();
    }
}
