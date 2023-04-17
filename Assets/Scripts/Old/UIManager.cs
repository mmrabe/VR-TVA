using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] public Text visualStimuli;

    // Start is called before the first frame update
    void Start()
    {
        visualStimuli.text = "hallo";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
