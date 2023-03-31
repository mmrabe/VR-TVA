using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    public Canvas canvasMain;
    private Text text;

    private Transform child1;


    private void Start()
    {
        text = this.gameObject.GetComponent<Text>();

        

    }

    private void Update()
    {
       
        text.text = "Canvas Position:\n";
        text.text += canvasMain.transform.position.ToString();
        text.text += "\nChild Position:\n";
        text.text += child1.position.ToString();
        text.text += "\nChild Rotation:\n";
        text.text += child1.rotation.ToString();
        if (child1 == null) { child1 = canvasMain.transform.GetChild(0); }
    }
}