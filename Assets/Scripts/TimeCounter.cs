using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TimeCounter: MonoBehaviour {
    private float dynamicOffset = 0f;
    private float dynamicOffset_buffer;
    private int calibrationTrialCount;

    float secondsLeft;
    public bool isRunning { private set; get; } = false;
    public bool isPaused { private set; get; } = false;
    bool isFirstFrame = false;

    public bool isExtraFrameLogged { get; private set; } = false;
    public float realTotalTime;
    public float estimatedTotalTime;
    public float startTime;
    private float frameDur = 1f/120f;
    
    public void StartCounting(float secondsToWait) {
        secondsLeft  = secondsToWait;
        isRunning = true;
        isFirstFrame = true;
        //calibrationCount += 1;
    }

    void Start() {
        
    }

    public void Update() {
        if (isFirstFrame) {
            isFirstFrame = false;
            isExtraFrameLogged = false;
            startTime = Time.time;
            return;
        }

        if (isRunning && !isPaused) {
            secondsLeft -= Time.deltaTime;
        }

        if (isRunning && secondsLeft < 2 * frameDur) {
            isRunning = false;
            float toWait = secondsLeft - frameDur;
            float stamp = Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup - stamp < toWait) {  }
            estimatedTotalTime = secondsLeft - frameDur - toWait;
            secondsLeft -= toWait;
        }
        

        if (!isRunning && !isExtraFrameLogged) {
            isExtraFrameLogged = true;
            realTotalTime = secondsLeft - Time.deltaTime;
        }

        //if ( isRunning &&  (secondsLeft - Time.deltaTime) < 0) {
        //    isRunning = false;
        //    estimatedTotalTime = secondsLeft - Time.deltaTime;
        //    //Debug.Log("Estimated Time: " + estimatedTotalTime);
        //} 
    }

    public void PauseTimer(bool pause) {
        isPaused = pause;
    }
}


