using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Experiment : MonoBehaviour {
    // States
    private StateMachine stateMachine;
    private States currentState;
    private List<ExperimentState> states;
    public StimuliState stimuliState;
    public PatternMaskState maskState;
    public AwaitInputState awaitState;
    public ExperimentState coolDownPostTrialState;

    public float patternMaskDuration;
    public float coolDownPostTrialDuration;

    // GameObjects
    public Canvas canvas;
    private Transform canvasCenter;
    public OVRCameraRig camRig;
    private List<GameObject> symbolsT;
    private List<GameObject> symbolsD;
    private GameObject FixationCrossObject;

    // Booleans
    private bool isFinished = false;
    private bool isFirstLoggedFinished = false;
    private bool isTrialDataLogged = false;

    // ExperimentSettings
    private List<IExperimentSetting> settings;
    private List<float> timeIntervals;
    private List<Trial> trials;
    private Trial currentTrial;
    private int trialsSoFar = 0;
    private int trialAmount;
    private int totalTrials;
    public int stimuliDistance;
    public int numbOfSlots;
    public float DistanceToArraySizeRatio;

    ExperimentSettingContainer esc;

    // Utility
    public CircleArray array;
    private TimeCounter timer = new TimeCounter();
    private string loggedData;
    public string trialInfo { get; private set; }

    public Experiment() {
        this.trialAmount = 1;
        this.timeIntervals = new List<float> { 0.01f, 0.02f, 0.05f, 0.1f, 0.15f, 0.2f };
        List<float> partialReportTime = new List<float> { 0.15f };

        ExperimentSetting wholeReportClose =    new ExperimentSetting(1, 8, 0, false, false, 0, this.timeIntervals);
        ExperimentSetting wholeReportFar =      new ExperimentSetting(2, 8, 0, true, false, 2f, this.timeIntervals);
        ExperimentSetting partialReportFF =     new ExperimentSetting(3, 4, 4, false, false, 0, partialReportTime);
        ExperimentSetting partialReportTF =     new ExperimentSetting(4, 4, 4, true, false, 2f, partialReportTime);
        ExperimentSetting partialReportFT =     new ExperimentSetting(5, 4, 4, false, true, 2f, partialReportTime);
        ExperimentSetting partialReportTT =     new ExperimentSetting(6, 4, 4, true, true, 2f, partialReportTime);
        settings = new List<IExperimentSetting>() { wholeReportClose, wholeReportFar, partialReportFF, partialReportFT, partialReportTF, partialReportTT };//{ partialReportFF, partialReportFT, partialReportTF, partialReportTT };


        esc = new ExperimentSettingContainer(settings, trialAmount);
        esc.Populate();
        this.trials = esc.Shuffle();
        this.totalTrials = trials.Count;
        
    }

    // Start is called before the first frame update
    void Start() {
        string preData = esc.LogData(this.trials);
        Debug.Log(preData);
        WriteLoggedData(preData, "RECIPE");
        Application.targetFrameRate = 120;
        //QualitySettings.vSyncCount = 1;

        this.canvasCenter = canvas.gameObject.GetComponent<RectTransform>().transform;
        this.symbolsT = LoadSymbols("LettersRed");
        this.symbolsD = LoadSymbols("LettersBlue");
        this.stateMachine = new StateMachine();
        this.states = new List<ExperimentState>() { awaitState, stimuliState, maskState, coolDownPostTrialState  };
        this.currentTrial = trials[trialsSoFar];

        // Find FixationCross
        FixationCrossObject = GameObject.Find("FixationCross");
        FixationCrossObject.transform.position = new Vector3(FixationCrossObject.transform.position.x, FixationCrossObject.transform.position.y, -stimuliDistance);
        // Initialize circular array and set it's position.
        //float radius = canvas.planeDistance * DistanceToArraySizeRatio;
        float radius = stimuliDistance * DistanceToArraySizeRatio;
        array.Init(radius, numbOfSlots, FixationCrossObject, stimuliDistance, canvasCenter);
        array.transform.position = canvasCenter.position;
        
        // Initialize states with their durations
        maskState.Initialize(this.patternMaskDuration);
        coolDownPostTrialState.Initialize(this.coolDownPostTrialDuration);
        
        // Generate stimuli, spawn them and set the given time
        var ret = GenerateSymbols(currentTrial.numbOfTargets, currentTrial.numbOfDistractors, this.symbolsT, this.symbolsD);
        array.PutIntoSlots(ret.Item1, ret.Item2, currentTrial.targetsFarAway, currentTrial.distractorsFarAway, currentTrial.depth);
        states[(int)States.Stimuli].Initialize(currentTrial.timeInterval);
    }

    void Update() {
        if (isFinished) { 
            return;
        }

        // If current state is finished, go to next state and execute it.
        if (states[(int)currentState].IsFinished()) {

            // If current trial is over, initialize new trial.
            if (currentState == States.AfterTrialCoolDown) {
                if (trialsSoFar <= this.totalTrials-1) {
                    this.currentTrial = trials[trialsSoFar];
                    this.trialsSoFar += 1;
                    this.isTrialDataLogged = false;
                    //Debug.Log(currentTrial.ToString());

                    array.Clear();
                    var ret = GenerateSymbols(currentTrial.numbOfTargets, currentTrial.numbOfDistractors, this.symbolsT, this.symbolsD);
                    array.PutIntoSlots(ret.Item1, ret.Item2, currentTrial.targetsFarAway, currentTrial.distractorsFarAway, currentTrial.depth);
                    states[(int)States.Stimuli].Initialize(currentTrial.timeInterval);
                } else {
                    isFinished = true;
                }
            }
            currentState = stateMachine.NextState();
            states[(int)currentState].Run();
        }

        if (currentState == States.WaitingForInput && !isTrialDataLogged &&
            states[(int)States.AfterTrialCoolDown].timer.isExtraFrameLogged) {
            loggedData += TrialInfo();
            WriteLoggedData(loggedData, "data");
            isTrialDataLogged = true;
        }
    }

    public int getTrialsSoFar() => this.trialsSoFar;

    public (List<GameObject>, List<GameObject>) GenerateSymbols(int numbOfTargets, int numbOfDistractors, List<GameObject> symbolsT, List<GameObject> symbolsD) {
        List<GameObject> targets = new List<GameObject>();
        List<GameObject> distractors = new List<GameObject>(); 
        System.Random rnd = new System.Random();
        List<int> usedIndex = new List<int>();
        int c = rnd.Next(0, symbolsT.Count);

        for (int i = 0; i < numbOfTargets; i++) {
            while(usedIndex.Contains(c)) {
                c = rnd.Next(0, symbolsT.Count);
            }
            usedIndex.Add(c);
            targets.Add(Instantiate(symbolsT[c]));
        }

        c = rnd.Next(0, symbolsD.Count);

        for (int i = 0; i < numbOfDistractors; i++) {
            while (usedIndex.Contains(c)) {
                c = rnd.Next(0, symbolsD.Count);
            }
            usedIndex.Add(c);
            distractors.Add(Instantiate(symbolsD[c]));
        }
        return (targets, distractors);
    }

    public List<GameObject> LoadSymbols(string parentName) {
        List<GameObject> retLst = new List<GameObject>();
        GameObject parent = GameObject.Find(parentName);
        for (int i = 0; i< parent.transform.childCount; i++) {
            retLst.Add(parent.transform.GetChild(i).gameObject);
        }
        return retLst;
    }

    public string TrialInfo() {
        string s = "";
        if (!isFirstLoggedFinished) {
            isFirstLoggedFinished = true;
            s += "settingID, T,D,Tfar,Dfar,Depth,time,trialNumb,TargetsPresented,DistractorsPresented," +
                "StimuliErr," +"MaskErr" + "\n";
            return s;
        }
        s += currentTrial.settingID + ",";
        s += currentTrial.numbOfTargets + ",";
        s += currentTrial.numbOfDistractors + ",";
        s += Convert.ToInt32(currentTrial.targetsFarAway).ToString() + ",";
        s += Convert.ToInt32(currentTrial.distractorsFarAway).ToString() + ",";
        s += currentTrial.depth + ","; 
        s += currentTrial.timeInterval + ",";
        s += trialsSoFar + ",";
        foreach (GameObject target in  array.targets) {
            s += target.name[0];
        }
        s += ",";
        if (array.distractors.Count == 0) { s += "!"; } 
        else {
            foreach (GameObject distractor in array.distractors) {
                s += distractor.name[0];
            }
        }
        s += ",";
        s += Math.Round(stimuliState.timer.realTotalTime * 1000,3) + ",";
        s += Math.Round(maskState.timer.realTotalTime * 1000, 3);
        s += "\n";

        return s;
    }

    public void WriteLoggedData(string data, string fileName) {
        string path;
        //path = "C:/Users/hccco/Desktop/Mariusz_Uffe_BachelorProject/BachelorMain/BachelorProj/Assets/" + fileName + ".txt";
        //FileStream streamPC = new FileStream(path, FileMode.OpenOrCreate);
        //using (var w = new StreamWriter(streamPC)) {
        //    w.WriteLine(data);
        //}
        //streamPC.Close();
        path = Path.Combine(Application.persistentDataPath) + fileName + ".txt";
        FileStream streamVr = new FileStream(path, FileMode.OpenOrCreate);
        using (var w = new StreamWriter(streamVr)) {
            w.WriteLine(data);
        }
        streamVr.Close();
    }

    //public List<Trial> TrialsFromTxt(string path1, string path2) 
    //{

    //}
}

/// REMOVED, REMOVE

    //public ExperimentState coolDownPostCrossState;
    //public FixationCrossState fixCrossState;
    //private bool isFixCrossShown = false;
    //private bool isDebugEnabled;
    //public bool isPractice;
    //public float coolDownPostCrossDuration;
    //public float fixationCrossDuration;
    //private int currentSettingNumber;
    //private int currentTimeIntervalNumber;
    //private float currentTimeInterval;
    //private List<float> randomTrials;
    //private int trialsSoFar = 0;
    //private ExperimentSetting currentSetting;

    /*public void TryToUpdateSetting() {
        if(trialsSoFar == trialAmount) {
            if(currentTimeIntervalNumber != currentSetting.timeIntervals.Count-1) {
                currentTimeIntervalNumber += 1;
                currentTimeInterval = currentSetting.timeIntervals[currentTimeIntervalNumber];
            } else {
                if(currentSettingNumber != settings.Count -1) {
                    currentSettingNumber += 1;
                    currentSetting = settings[currentSettingNumber];
                } else {
                    isFinished = true;
                    WriteLoggedData("data");
                }
            }
            trialsSoFar = 0;
        }
        trialsSoFar += 1;
    }*/

    //s += Math.Round(fixCrossState.timer.estimatedTotalTime * 1000, 3) + "," + Math.Round(fixCrossState.timer.realTotalTime * 1000, 3) + ",";
    //s += Math.Round(coolDownPostCrossState.timer.estimatedTotalTime * 1000, 3) + "," + Math.Round(coolDownPostCrossState.timer.realTotalTime * 1000, 3) + ",";
    //s += Math.Round(maskState.timer.estimatedTotalTime * 1000, 3) + "," + Math.Round(maskState.timer.realTotalTime * 1000, 3) + ",";


    //public void ShowFixationCross() {
    //    isFixCrossShown = true;
    //    FixationCrossObject.SetActive(true);
    //}

    //public void HideFixationCross() {
    //    isFixCrossShown = false;
    //    FixationCrossObject.SetActive(false);
    //}

    /*public void InputHandler() {
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown) &&
                OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown)) {
            if (!isDebugEnabled) {
                isDebugEnabled = true;
                timer.PauseTimer(isDebugEnabled);
                foreach (GameObject t in array.targets) {
                    t.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
                }
            }
        }

        if (isDebugEnabled) {
            if (OVRInput.Get(OVRInput.Button.One)) {
                array.ChangeRadius(-1);
            }
            else if (OVRInput.Get(OVRInput.Button.Two)) {
                array.ChangeRadius(1);
            }
            else if (OVRInput.Get(OVRInput.Button.Three)){
                array.ChangeDepth(-1);
            }
            else if (OVRInput.Get(OVRInput.Button.Four)) {
                array.ChangeDepth(1);
            }
            else if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger)) {
                array.ChangeSymbolsSize(1);
            }
            else if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger)) {
                array.ChangeSymbolsSize(-1);
            }
            else if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)) {
                array.ChangeDistance(1);
            }
            else if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger)) {
                array.ChangeDistance(-1);
            }
        }
    }*/


    //InputHandler();


    /*if(isPractice) { 
        SceneManager.LoadScene("mockup"); 
    }*/



    /*var ret = GenerateSymbols(currentSetting.numbOfTargets, currentSetting.numbOfDistractors, this.symbolsT, this.symbolsD);
    array.PutIntoSlots(ret.Item1, ret.Item2, currentSetting.targetsFarAway, currentSetting.distractorsFarAway, currentSetting.depth);
    states[(int)States.Stimuli].Initialize(this.currentTimeInterval);*/

    //fixCrossState.Initialize(this.fixationCrossDuration);
    //coolDownPostCrossState.Initialize(this.coolDownPostCrossDuration);


    //this.states = new List<ExperimentState>() { awaitState, fixCrossState,
    //                                       coolDownPostCrossState, stimuliState,
    //                                       maskState, coolDownPostTrialState  };


    //HideFixationCross();

    
    //ExperimentSetting calibration = new ExperimentSetting(4, 4, false, false, 1, timeIntervals);
    //settings = new List<ExperimentSetting>() { wholeReportClose, wholeReportFar, partialReportFF, partialReportFT, partialReportTF, partialReportTT };
    
    /*ExperimentSetting setting1 = new ExperimentSetting(4, 4, true, false, 0.2f, this.timeIntervals);
    settings = new List<ExperimentSetting>() { setting1 };
    currentSetting = settings[currentSettingNumber];
    currentTimeInterval = currentSetting.timeIntervals[currentTimeIntervalNumber];

    if(isPractice) {
        ExperimentSetting settingPractice = new ExperimentSetting(4, 4, false, false, 0.2f, new List<float> { 1f, 2f, 5f, 10f });
        settings = new List<ExperimentSetting> { settingPractice };
    }*/