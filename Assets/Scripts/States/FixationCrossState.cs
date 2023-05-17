//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class FixationCrossState : ExperimentState
//{
//    public GameObject fixCross;
//    public CircleArray circleArray; 

//    public override void Initialize(float duration)
//    {
//        base.Initialize(duration);
//        //fixCross.SetActive(false);
//        circleArray.HideReceptiveField();
//    }
//    public override void Run()
//    {
//        base.Run();
//        //fixCross.SetActive(true);
//        circleArray.ShowReceptiveField();
//    }

//    protected override void Update()
//    {
//        base.Update();
//        if (IsFinished()) { 
//            //fixCross.SetActive(false);
//            circleArray.HideReceptiveField();
//        }
//    }
//}