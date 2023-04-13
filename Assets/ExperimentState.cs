using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ExperimentState: MonoBehaviour
{
    public bool isFinished { get; private set; } = false;
    private Action routine;
    private TimeCounter timer;

    public ExperimentState(Action routine)
    {
        this.routine = routine;
        timer = new TimeCounter();
    }

    public void Run(float duration)
    {
        timer.StartCounting(duration);
        isFinished = false;
    }



    void Update()
    {
        if(!isFinished)
        {
            routine();
        }
        else
        {
            isFinished = true;
        }
    }
}
