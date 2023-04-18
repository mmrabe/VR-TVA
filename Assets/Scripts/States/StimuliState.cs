using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StimuliState : ExperimentState
{
    public CircleArray array;

    public override void Initialize(float duration)
    {
        base.Initialize(duration);
        array.HideStimuli();
        //Debug.Log("Stimuli hidden initialy");
    }
    public override void Run()
    {
        array.ShowStimuli();
        //Debug.Log("Stimuli showed");
        base.Run();
        

    }

    protected override void Update()
    {
        base.Update();
        if (array.stimuliEnabled &&  IsFinished()) 
        { 
            array.HideStimuli();
            //Debug.Log("Stimuli hidden");
        }
        
    }

}