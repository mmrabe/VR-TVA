using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class Experiment : MonoBehaviour {
    // states
    public StimuliState stimuliState;
    public PatternMaskState maskState;
    public AwaitInputState awaitState;
    public ExperimentState coolDownPostCrossState;
    public ExperimentState coolDownPostTrialState;
    public FixationCrossState fixCrossState;

    private States currentState;
    private List<ExperimentState> states;

    private StateMachine stateMachine;

    // GameObjects
    public Canvas canvas;
    private Transform canvasCenter;
    public OVRCameraRig camRig;

    // Booleans
    bool isFinished = false;
    bool isDebugEnabled;
    bool isCurrentTrialFinished = true;

    // ExperimentSettings
    private List<ExperimentSetting> settings;
    ExperimentSetting currentSetting;

    public CircleArray array;
    TimeCounter timer = new TimeCounter();

    public int trialAmount;
    int trialsSoFar = 0;
    int currentSettingNumber;
    int currentTimeIntervalNumber;
    float currentTimeInterval;
    public float patternMaskDuration;
    private List<float> timeIntervals;

    private List<GameObject> symbolsT;
    private List<GameObject> symbolsD;

    private bool isFixCrossShown = false;
    private GameObject FixationCrossObject;
    public float fixationCrossDuration;

    private string loggedData;

    private bool isFirstLoggedFinished = false;

    public string trailInfo { get; private set; }

    private bool isTrialDataLogged = false;

    public bool isPractice;

    public float coolDownPostTrialDuration;
    public float coolDownPostCrossDuration;

    public Experiment() {
        trialAmount = 3;

        timeIntervals = new List<float> { 2f,4f };//, 20f, 50f };//, 100f, 150f, 200f };

        ExperimentSetting calibration = new ExperimentSetting(4, 4, false, false, 1, timeIntervals);

        ExperimentSetting wholeReportClose = new ExperimentSetting(8, 0, false, false, 0, timeIntervals);
        ExperimentSetting wholeReportFar = new ExperimentSetting(8, 0, true, false, 2, timeIntervals);
        ExperimentSetting partialReportFF = new ExperimentSetting(4, 4, false, false, 0, new List<float> { 150 });
        ExperimentSetting partialReportTF = new ExperimentSetting(4, 4, true, false, 1, new List<float> { 150 });
        ExperimentSetting partialReportFT = new ExperimentSetting(4, 4, false, true, 1, new List<float> { 150 });
        ExperimentSetting partialReportTT = new ExperimentSetting(4, 4, true, true, 1, new List<float> { 150 });
        //settings = new List<ExperimentSetting>() { wholeReportClose,wholeReportFar,
        //                partialReportFF,partialReportFT,partialReportTF,partialReportTT };

        ExperimentSetting setting1 = new ExperimentSetting(3, 5, false, false, 1, timeIntervals);
        settings = new List<ExperimentSetting>() { setting1 };

        
        

        currentSetting = settings[currentSettingNumber];
        currentTimeInterval = currentSetting.timeIntervals[currentTimeIntervalNumber];
        patternMaskDuration = 0.5f;
        fixationCrossDuration = 1f;

        if(isPractice)
        {
            ExperimentSetting settingPractice = new ExperimentSetting(4, 4, false, false, 1, new List<float> { 1f, 2f, 5f, 10f });
            settings = new List<ExperimentSetting> { settingPractice };
        }


        
    }

    // Start is called before the first frame update
    void Start() {
        FixationCrossObject = GameObject.Find("FixationCross");
        HideFixationCross();
        canvasCenter = canvas.gameObject.GetComponent<RectTransform>().transform;
        //new CircularArray();
        array.Init(2f, 8, FixationCrossObject, canvasCenter);
        array.transform.position = canvasCenter.position;
        symbolsT = LoadSymbols("Letters");
        symbolsD = LoadSymbols("Digits");

        
        fixCrossState.Initialize(fixationCrossDuration);
        coolDownPostCrossState.Initialize(coolDownPostCrossDuration);
        maskState.Initialize(patternMaskDuration);
        coolDownPostTrialState.Initialize(coolDownPostTrialDuration);
        states = new List<ExperimentState>() { awaitState, fixCrossState,
                                        coolDownPostCrossState, stimuliState,
                                        maskState, coolDownPostTrialState  };
        stateMachine = new StateMachine();
        var ret = GenerateSymbols(currentSetting.numbOfTargets, currentSetting.numbOfDistractors, symbolsT, symbolsD);
        array.PutIntoSlots(ret.Item1, ret.Item2, currentSetting.depth);
        states[(int)States.Stimuli].Initialize(currentTimeInterval);
    }

    void Update()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name
        if (isFinished ) { 
            if(isPractice) { SceneManager.LoadScene("mockup"); }
            return;
        }

        InputHandler();

        if (states[(int)currentState].IsFinished())
        {
            if (currentState == States.AfterTrialCoolDown)
            {
                TryToUpdateSetting();
                isTrialDataLogged = false;
                array.Clear();
                var ret = GenerateSymbols(currentSetting.numbOfTargets, currentSetting.numbOfDistractors, symbolsT, symbolsD);
                array.PutIntoSlots(ret.Item1, ret.Item2, currentSetting.depth);
                states[(int)States.Stimuli].Initialize(currentTimeInterval);
            }

            currentState = stateMachine.NextState();
            states[(int)currentState].Run();
        }

        if (currentState == States.WaitingForInput && !isTrialDataLogged &&
            states[(int)States.AfterTrialCoolDown].timer.isExtraFrameLogged)
        {
            loggedData += TrialInfo();
            isTrialDataLogged = true;
        }

    }

    public void ShowFixationCross()
    {
        isFixCrossShown = true;
        //FixationCrossObject.GetComponent<MeshRenderer>().enabled = true;
        FixationCrossObject.SetActive(true);
    }

    public void HideFixationCross() {
        isFixCrossShown = false;
        //FixationCrossObject.GetComponent<MeshRenderer>().enabled = false;
        FixationCrossObject.SetActive(false);
    }


    public void InputHandler()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown) &&
                OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown))
        {
            if (!isDebugEnabled) { 
                isDebugEnabled = true;
                timer.PauseTimer(isDebugEnabled);
                foreach (GameObject t in array.targets)
                {
                    t.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
                }
            }
            //isDebugEnabled = !isDebugEnabled;
            //timer.PauseTimer(isDebugEnabled);
        }

        if (isDebugEnabled)
        {
            if (OVRInput.Get(OVRInput.Button.One))
            {
                array.ChangeRadius(-1);
            }
            else if (OVRInput.Get(OVRInput.Button.Two))
            {
                array.ChangeRadius(1);
            }
            else if (OVRInput.Get(OVRInput.Button.Three))
            {
                array.ChangeDepth(-1);
            }
            else if (OVRInput.Get(OVRInput.Button.Four))
            {
                array.ChangeDepth(1);
            }
            else if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger))
            {
                array.ChangeSymbolsSize(1);
            }
            else if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
            {
                array.ChangeSymbolsSize(-1);
            }
            else if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
            {
                array.ChangeDistance(1);
            }
            else if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                array.ChangeDistance(-1);
            }
        }
    }


    public (List<GameObject>, List<GameObject>) GenerateSymbols(int numbOfTargets, int numbOfDistractors, List<GameObject> symbolsT, List<GameObject> symbolsD)
    {
        List<GameObject> targets = new List<GameObject>();
        List<GameObject> distractors = new List<GameObject>(); 
        System.Random rnd = new System.Random();
        List<int> usedIndx = new List<int>();
        int c = rnd.Next(0, symbolsT.Count);

        for (int i = 0; i < numbOfTargets; i++)
        {
            while(usedIndx.Contains(c))
            {
                c = rnd.Next(0, symbolsT.Count);
            }
            usedIndx.Add(c);
            targets.Add(Instantiate(symbolsT[c]));
        }

        c = rnd.Next(0, symbolsD.Count);
        usedIndx = new List<int>();

        for (int i = 0; i < numbOfDistractors; i++)
        {
            while (usedIndx.Contains(c))
            {
                c = rnd.Next(0, symbolsD.Count);
            }
            usedIndx.Add(c);
            distractors.Add(Instantiate(symbolsD[c]));
        }
        return (targets, distractors);
    }


    public void TryToUpdateSetting()
    {
        if(trialsSoFar == trialAmount) 
        {
            if(currentTimeIntervalNumber != currentSetting.timeIntervals.Count-1)
            {
                currentTimeIntervalNumber += 1;
                currentTimeInterval = currentSetting.timeIntervals[currentTimeIntervalNumber];
                
                
            }
            else
            {
                if(currentSettingNumber != settings.Count -1)
                {
                    currentSettingNumber += 1;
                    currentSetting = settings[currentSettingNumber];
                }

                else
                {
                    isFinished = true;
                    WriteLoggedData("data");
                }
            }

            trialsSoFar = 0;
        }
        trialsSoFar += 1;
    }

    public List<GameObject> LoadSymbols(string parentName)
    {
        List<GameObject> retLst = new List<GameObject>();
        GameObject parent = GameObject.Find(parentName);
        for (int i = 0; i< parent.transform.childCount; i++)
        {
            retLst.Add(parent.transform.GetChild(i).gameObject);
        }
        return retLst;
    }

    public string TrialInfo()
    {
        string s = "";
        if (!isFirstLoggedFinished)
        {
            isFirstLoggedFinished = true;
            s += "T,D,TP,DP,TI,N,Targets,FixationCrossTA,FixationCrossTR," +
                "CoolDownPostCrossTA,CoolDownPostCrossTR,StimuliTA,StimuliTR," +
                "MaskTA,MaskTR,CoolDownPostTrialTA,CoolDownPostTrialTR" + "\n";
            return s;
        }


        s += currentSetting.numbOfTargets + ",";
        s += currentSetting.numbOfDistractors + ",";
        s += Convert.ToInt32(currentSetting.targetsFarAway).ToString() + ",";
        s += Convert.ToInt32(currentSetting.distractorsFarAway).ToString() + ",";
        s += currentTimeInterval + ",";
        s += trialsSoFar + ",";

        foreach (GameObject target in  array.targets)
        {
            s += target.name[0];
        }
        s += ",";

        s += Math.Round(fixCrossState.timer.estimatedTotalTime * 1000, 3) + "," + Math.Round(fixCrossState.timer.realTotalTime * 1000, 3) + ",";
        s += Math.Round(coolDownPostCrossState.timer.estimatedTotalTime * 1000, 3) + "," + Math.Round(coolDownPostCrossState.timer.realTotalTime * 1000, 3) + ",";
        s += Math.Round(stimuliState.timer.estimatedTotalTime * 1000,3) + "," + Math.Round(stimuliState.timer.realTotalTime * 1000,3) + ",";
        s += Math.Round(maskState.timer.estimatedTotalTime * 1000, 3) + "," + Math.Round(maskState.timer.realTotalTime * 1000, 3) + ",";
        s += Math.Round(coolDownPostTrialState.timer.estimatedTotalTime * 1000, 3) + "," + Math.Round(coolDownPostTrialState.timer.realTotalTime * 1000, 3);
        s += "\n";
        return s;
    }

    public void WriteLoggedData(string path)
    {
        path = "C:/Users/hccco/Desktop/Mariusz_Uffe_BachelorProject/BachelorProj/Assets";
        path = "BLAH.txt";
        path = Path.Combine(Application.persistentDataPath) + "/BLAH.txt";
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
        using (var w = new StreamWriter(stream))
            {
                w.WriteLine(loggedData);
            }
    }


}
    public class ExperimentSetting {
        public int numbOfTargets { private set; get; }
        public int numbOfDistractors { private set; get; }
        public bool targetsFarAway { private set; get; }
        public bool distractorsFarAway { private set; get; }
        public int depth { private set; get; }
        public List<float> timeIntervals { private set; get; } = new List<float>() { 2.0f, 5.0f, 10.0f };

        public ExperimentSetting(int NumbOftargets, int NumbOfdistractors, bool targetsFarAway, bool distractorsFarAway, int Depth, List<float> TimeIntervals) {
            this.numbOfTargets = NumbOftargets;
            this.numbOfDistractors = NumbOfdistractors;
            this.targetsFarAway = targetsFarAway;
            this.distractorsFarAway = distractorsFarAway;
            this.depth = Depth;
            this.timeIntervals = TimeIntervals;
        }

        public ExperimentSetting(int NumbOftargets, int NumbOfdistractors, bool targetsFarAway, bool distractorsFarAway, int Depth)
        {
            this.numbOfTargets = NumbOftargets;
            this.numbOfDistractors = NumbOfdistractors;
            this.targetsFarAway = targetsFarAway;
            this.distractorsFarAway = distractorsFarAway;
            this.depth = Depth;
        }
    }


public class StateMachine
{
    States currentState;
    private int maxStateNumber;

    public StateMachine()
    {
        currentState = States.WaitingForInput;
        maxStateNumber = Enum.GetValues(typeof(States)).Length-1;
        
    }

    public States NextState()
    {
        currentState += 1;
        if ((int)currentState > maxStateNumber)
        {
            currentState = States.WaitingForInput;
        }
        return currentState;

    }
    
}



public class TimeCounter: MonoBehaviour
{
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
    
    public void StartCounting(float secondsToWait)
    {
        secondsLeft = secondsToWait;
        isRunning = true;
        isFirstFrame = true;
        //calibrationCount += 1;
    }
    void Start()
    {
        
    }

    public void Update()
    {
        if (isFirstFrame)
        {
            isFirstFrame = false;
            isExtraFrameLogged = false;
            startTime = Time.time;
            return;
        }

        if( isRunning && !isPaused)
        {
            
            secondsLeft -= Time.deltaTime;
        }

        if( isRunning &&  (secondsLeft - Time.deltaTime) < 0)
        {
            isRunning = false;
            estimatedTotalTime = secondsLeft - Time.deltaTime;
            //Debug.Log("Estimated Time: " + estimatedTotalTime);
           
        }
        else if (!isRunning && !isExtraFrameLogged)
        {
            isExtraFrameLogged = true;
            realTotalTime = secondsLeft - Time.deltaTime ;
            //Debug.Log("Real Time: " + realTotalTime);
        }
    }

    public void PauseTimer(bool pause)
    {
        isPaused = pause;
    }


}





