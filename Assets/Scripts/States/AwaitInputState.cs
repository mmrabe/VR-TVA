using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwaitInputState : ExperimentState
{
    public override bool IsFinished()
    {
        return OVRInput.Get(OVRInput.Button.One) ||
                Input.GetKeyDown(KeyCode.Space);
    }
}
