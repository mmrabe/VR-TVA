using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ExperimentState : MonoBehaviour
{
    public TimeCounter timer { get; private set; }

    protected float duration;

    public void Start() { timer = new TimeCounter(); }

    virtual protected void Update()
    {
        timer.Update();
    }

    virtual public void Run()
    {
        timer.StartCounting(duration);
    }

    virtual public void Pause()
    {
        timer.PauseTimer(true);
    }

    virtual public void UnPause()
    {
        timer.PauseTimer(false);
    }

    virtual public void Initialize(float duration)
    {
        this.duration = duration;
    }

    virtual public bool IsFinished()
    {
        return !timer.isRunning;
    }


}

//public class AwaitInputState : ExperimentState
//{
//    public override bool IsFinished()
//    {
//        return OVRInput.Get(OVRInput.Button.One) ||
//                Input.GetKeyDown(KeyCode.Space);
//    }
//}


//public class FixationCrossState : ExperimentState
//{
//    public GameObject fixCross;

//    public override void Initialize(float duration)
//    {
//        base.Initialize(duration);
//        fixCross.GetComponent<MeshRenderer>().enabled = false;
//    }
//    public override void Run()
//    {
//        base.Run();
//        fixCross.GetComponent<MeshRenderer>().enabled = true;

//    }

//    protected override void Update()
//    {
//        base.Update();
//        if (IsFinished()) { fixCross.GetComponent<MeshRenderer>().enabled = false; }
//    }
//}

//public class PatternMaskState : ExperimentState
//{
//    public CircleArray array;

//    public override void Initialize(float duration)
//    {
//        base.Initialize(duration);
//        array.HidePatternMask();
//    }
//    public override void Run()
//    {
//        base.Run();
//        array.ShowPatternMask();

//    }

//    protected override void Update()
//    {
//        base.Update();
//        if (IsFinished()) { array.HidePatternMask(); }
//    }
//}


//public class StimuliState : ExperimentState
//{
//    public CircleArray array;

//    public override void Initialize(float duration)
//    {
//        base.Initialize(duration);
//        array.HideStimuli();
//    }
//    public override void Run()
//    {
//        base.Run();
//        array.ShowStimuli();

//    }

//    protected override void Update()
//    {
//        base.Update();
//        if (IsFinished()) { array.HideStimuli(); }
//    }

//}

























//public bool isFinished { get; private set; } = false;
//private Action routine;
//private TimeCounter timer;

//public ExperimentState(Action routine)
//{
//    this.routine = routine;
//    timer = new TimeCounter();
//}

//public void Run(float duration)
//{
//    timer.StartCounting(duration);
//    isFinished = false;
//}



//void Update()
//{
//    if(!isFinished)
//    {
//        routine();
//    }
//    else
//    {
//        isFinished = true;
//    }
//}