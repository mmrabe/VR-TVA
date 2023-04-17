using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PatternMaskState : ExperimentState
{
    public CircleArray array;

    public override void Initialize(float duration)
    {
        base.Initialize(duration);
        array.HidePatternMask();
    }
    public override void Run()
    {
        base.Run();
        array.ShowPatternMask();

    }

    protected override void Update()
    {
        base.Update();
        if (IsFinished()) { array.HidePatternMask(); }
    }
}