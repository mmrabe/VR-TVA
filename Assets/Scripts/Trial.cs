using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trial : ITrial {
    public int numbOfTargets { private set; get; }
    public int numbOfDistractors { private set; get; }
    public float depth { private set; get; }
    public bool targetsFarAway { private set; get; }
    public bool distractorsFarAway { private set; get; }
    public float timeInterval;

    public Trial(int nT, int nD, float d, bool tP, bool dP, float tI) {
        this.numbOfTargets = nT;
        this.numbOfDistractors = nD;
        this.depth = d;
        this.targetsFarAway = tP;
        this.distractorsFarAway = dP;
        this.timeInterval = tI; 
    }

    public override string ToString() {
      return numbOfTargets.ToString() + ", " + numbOfDistractors.ToString() + ", " + depth.ToString() + ", " + targetsFarAway.ToString() + ", " + distractorsFarAway.ToString() + ", " +  timeInterval.ToString();
    }
}