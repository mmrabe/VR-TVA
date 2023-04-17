using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public class Experiment : MonoBehaviour {
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

    CircularArray array;
    TimeCounter timer = new TimeCounter();

    int trialAmount;
    int trialsSoFar = 0;
    int currentSettingNumber;
    int currentTimeIntervalNumber;
    float currentTimeInterval;
    private float patternMaskDuration;
    private List<float> timeIntervals;

    private List<GameObject> symbolsT;
    private List<GameObject> symbolsD;

    private bool isFixCrossShown = false;
    private GameObject FixationCrossObject;
    private float fixationCrossDuration;
    private bool isCrossCooldownOver = false;



    public Experiment() {
        trialAmount = 2;
        //lettersT = new List<string>() { "a", "b", "c" };
        //lettersD = new List<string>() { "o" };

        


        timeIntervals = new List<float> { 10f, 20f, 50f, 100f, 150f, 200f };

        ExperimentSetting calibration = new ExperimentSetting(4, 4, false, false, 1, timeIntervals);

        ExperimentSetting wholeReportClose = new ExperimentSetting(8, 0, false, false, 0, timeIntervals);
        ExperimentSetting wholeReportFar = new ExperimentSetting(8, 0, true, false, 2, timeIntervals);

        ExperimentSetting partialReportFF = new ExperimentSetting(4, 4, false, false, 0, new List<float> { 150 });
        ExperimentSetting partialReportTF = new ExperimentSetting(4, 4, true, false, 1, new List<float> { 150 });
        ExperimentSetting partialReportFT = new ExperimentSetting(4, 4, false, true, 1, new List<float> { 150 });
        ExperimentSetting partialReportTT = new ExperimentSetting(4, 4, true, true, 1, new List<float> { 150 });

        
        ExperimentSetting setting1 = new ExperimentSetting(3, 5, false, false, 1);
        settings = new List<ExperimentSetting>() { setting1 };
        //settings = new List<ExperimentSetting>() { wholeReportClose,wholeReportFar,
        //                partialReportFF,partialReportFT,partialReportTF,partialReportTT };

        currentSetting = settings[currentSettingNumber];
        currentTimeInterval = currentSetting.timeIntervals[currentTimeIntervalNumber];
        patternMaskDuration = 0.5f;
        fixationCrossDuration = 1f;
        
    }

    // Start is called before the first frame update
    void Start() {
        FixationCrossObject = GameObject.Find("FixationCross");
        HideFixationCross();
        canvasCenter = canvas.gameObject.GetComponent<RectTransform>().transform;
        array = new CircularArray();
        array.Init(2f, 8, FixationCrossObject, canvasCenter);
        array.myObj.transform.position = canvasCenter.position;
        symbolsT = LoadSymbols("Letters");
        symbolsD = LoadSymbols("Digits");


    }


    // Update is called once per frame
    void Update()
    {

        array.myObj.transform.rotation = canvasCenter.rotation;
        array.myObj.transform.position = canvasCenter.position;
        InputHandler();

        if (!isFinished)
        {
            timer.Update();

            if (!timer.isRunning && isCurrentTrialFinished)
            {
                if (isFixCrossShown  || isCrossCooldownOver)
                {
                    if (!isCrossCooldownOver)
                    {
                        HideFixationCross();
                        timer.StartCounting(1.0f);
                        isCrossCooldownOver = true;
                    } else
                    {
                        NewExperiment();
                        timer.StartCounting(currentTimeInterval);
                        isCrossCooldownOver = false;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Space)) // // //
                    //if ((OVRInput.Get(OVRInput.Button.One)))
                    {
                        ShowFixationCross();
                        timer.StartCounting(fixationCrossDuration);
                    }
                    
                }
            }
            else if (!timer.isRunning)
            {
                array.Clear();
                if(array.patternMaskEnabled)
                {
                    isCurrentTrialFinished = true;
                    array.HidePatternMask();
                    TryToUpdateSetting();
                }
                else
                {
                    array.ShowPatternMask();
                    timer.StartCounting(patternMaskDuration);
                }
                

            }

        }

    }

    public void ShowFixationCross()
    {
        isFixCrossShown = true;
        FixationCrossObject.GetComponent<MeshRenderer>().enabled = true;
    }

    public void HideFixationCross() {
        isFixCrossShown = false;
        FixationCrossObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void NewExperiment() {
        var ret = GenerateSymbols(currentSetting.numbOfTargets, currentSetting.numbOfDistractors, symbolsT, symbolsD);
        array.PutIntoSlots(ret.Item1, ret.Item2);
        if (currentSetting.targetsFarAway) { array.PushBack(currentSetting.depth, true); }
        else if (currentSetting.distractorsFarAway) { array.PushBack(currentSetting.depth, false); }
        //timer.StartCounting(currentTimeInterval);
        isCurrentTrialFinished = false;
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

    public string TrailInfo()
    {
        string s = "";
        s += currentSetting.numbOfTargets + "/";
        s += currentSetting.numbOfDistractors + "/";
        s += Convert.ToInt32(currentSetting.targetsFarAway).ToString() + "/";
        s += Convert.ToInt32(currentSetting.distractorsFarAway).ToString() + "/";
        s += currentTimeInterval + "/";
        s += trialsSoFar;
        s += "\n";
        s += Math.Round(timer.estimatedTotalTime * 1000,3) + "/" + Math.Round(timer.realTotalTime * 1000,3);
        return s;
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



public class CircularArray : MonoBehaviour
{
    public GameObject myObj;
    private Vector3[] slots;
    private int numbOfSlots;
    public List<GameObject> targets;
    public List<GameObject> distractors;
    private float radius;
    private bool targetsPushed = false;
    private bool distractorsPushed  = false;
    private GameObject patternMask;
    private Transform canvasTransform;
    private GameObject[] patternMasks;
    public bool patternMaskEnabled { get; private set; } = false;
    private GameObject FixationCrossObject;
    private Transform canvasUpTrans;
    private Transform canvasRightTrans;


    public CircularArray() {
    }

    private void Update()
    {

    }

    public void Init(float radius, int NumbOfSlots, GameObject FixationCrossObject, Transform canvasTransform = null)
    {
        this.radius = radius;
        myObj = new GameObject();
        numbOfSlots = NumbOfSlots;
        this.canvasTransform = canvasTransform;
        this.FixationCrossObject = FixationCrossObject;

        slots = new Vector3[NumbOfSlots];
        Debug.Log(slots.Length);
        float angleStep = 360 / NumbOfSlots;
        float currAngle = 0f;
        for (int i = 0; i < NumbOfSlots; i++)
        {
            float x = (float)(radius * Math.Cos(currAngle*(Math.PI/180)));
            float y = (float)(radius * Math.Sin(currAngle * (Math.PI / 180)));
            slots[i] = new Vector3(x, y, 0);
            currAngle += angleStep;
            Debug.Log("slot " + i + " = " + "(" + x + ";" + y + ")");
        }

        patternMask = GameObject.Find("PatternMask");
        patternMasks = new GameObject[numbOfSlots];
        CreatePatternMask();
        canvasRightTrans = GameObject.Find("right").transform;
        canvasUpTrans = GameObject.Find("up").transform;


    }

    public void CreatePatternMask()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            GameObject clone = GameObject.Instantiate(patternMask);
            clone.transform.position = canvasTransform.position + slots[i];
            clone.transform.SetParent(canvasTransform);
            patternMasks[i] = clone;
        }
        HidePatternMask();
    }

    public void ShowPatternMask()
    {
        for (int i = 0; i < patternMasks.Length; i++)
        {
            patternMasks[i].GetComponent<MeshRenderer>().enabled = true;
        }
        patternMaskEnabled = true;
    }

    public void HidePatternMask()
    {
        for (int i = 0; i < patternMasks.Length; i++)
        {
            patternMasks[i].GetComponent<MeshRenderer>().enabled = false;
        }
        patternMaskEnabled = false;
    }

    public void PutIntoSlots(List<GameObject> Targets, List<GameObject> Distractors)
    {
        List<int> found = new List<int>();
        float initialScale = GameObject.Find("PatternMask").transform.localScale.x;
        distractors = Distractors;
        targets = Targets;
        System.Random rnd = new System.Random();
        int c = rnd.Next(0, numbOfSlots);
        for (int i = 0; i < targets.Count; i++)
        {
            //targets[i].transform.SetParent(myObj.transform);
            targets[i].transform.SetParent(canvasTransform);

            while (found.Contains(c))
            { c = rnd.Next(0, numbOfSlots); }
                
            found.Add(c);
            Vector3 offsetUp = (FixationCrossObject.transform.position - canvasUpTrans.position).normalized * slots[c].y;
            Vector3 offsetRight = (FixationCrossObject.transform.position - canvasRightTrans.position).normalized * slots[c].x;
            
            targets[i].transform.position = canvasTransform.position + offsetUp + offsetRight + new Vector3(0.25f * initialScale, -0.5f * initialScale,0);
            targets[i].transform.rotation = FixationCrossObject.transform.rotation;

        }
        for (int i = 0; i < distractors.Count; i++)
        {
            distractors[i].transform.SetParent(canvasTransform);
            while (found.Contains(c))
            { c = rnd.Next(0, numbOfSlots); }
            found.Add(c);
            Vector3 offsetUp = (FixationCrossObject.transform.position - canvasUpTrans.position).normalized * slots[c].y;
            Vector3 offsetRight = (FixationCrossObject.transform.position - canvasRightTrans.position).normalized * slots[c].x;
            distractors[i].transform.position = canvasTransform.position + offsetUp + offsetRight + new Vector3(0.25f * initialScale, -0.5f * initialScale, 0);
            distractors[i].transform.rotation = FixationCrossObject.transform.rotation;
        }
    }

    public void PushBack(int depth, bool pushTargets)
    {
        targetsPushed = pushTargets;
        distractorsPushed = !pushTargets;
        Vector3 v = new Vector3(0, 0, depth);
        if (pushTargets)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].transform.position -= v;
            }
        }
        else
        {
            for (int i = 0; i < distractors.Count; i++)
            {
                distractors[i].transform.position -= v;
            }
        }

        float mask_depth = Math.Max(targets[0].transform.position.z, distractors[0].transform.position.z);

        for (int i = 0; i < patternMasks.Length; i++)
        {
            patternMasks[i].transform.position = new Vector3(
                                                    patternMasks[i].transform.position.x,
                                                    patternMasks[i].transform.position.y,
                                                    mask_depth);
        }

    }

    public void ChangeSymbolsSize(int scale)
    {
        float s = scale / 10;
        for (int i=0; i<targets.Count; i++)
        {
            targets[i].transform.localScale += new Vector3(s,s,s);
        }
        for (int i = 0; i < distractors.Count; i++)
        {
            distractors[i].transform.localScale += new Vector3(s, s, s);
        }

        FixationCrossObject.transform.localScale += new Vector3(s, s, s);
    }

    public void ChangeRadius(int incr)
    {
        this.radius += incr / 10;
        Init(this.radius, numbOfSlots, FixationCrossObject);
        for (int i = 0; i<targets.Count; i++)
        {
            targets[i].transform.position = myObj.transform.position + slots[i];
        }
        for (int i = 0; i < distractors.Count; i++)
        {
            distractors[i].transform.position = myObj.transform.position + slots[i+targets.Count-1];
        }
    }

    public void ChangeDepth(int incr)
    {
        PushBack(incr / 10, targetsPushed);
    }

    public void ChangeDistance(int incr)
    {
        gameObject.transform.position += new Vector3(0, 0, incr / 10);
    }

    public void Clear()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Destroy(targets[i]);

        }
        for (int i = 0; i < distractors.Count; i++)
        {
            Destroy(distractors[i]);
        }
    }
}

public class TimeCounter: MonoBehaviour
{
    private float dynamicOffset = 0f;
    private float dynamicOffset_buffer;
    private int calibrationTrailCount;

    float secondsLeft;
    public bool isRunning { private set; get; } = false;
    public bool isPaused { private set; get; } = false;
    bool isFirstFrame = false;

    bool isExtraFrameLogged = false;

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

        if( isRunning &&  (secondsLeft - Time.deltaTime) -0.0005f < 0)
        {
            isRunning = false;
            estimatedTotalTime = secondsLeft - Time.deltaTime - 0.0005f;
            Debug.Log("Estimated Time: " + estimatedTotalTime);
           
        }
        else if (!isRunning && !isExtraFrameLogged)
        {
            isExtraFrameLogged = true;
            realTotalTime = secondsLeft - Time.deltaTime ;
            Debug.Log("Real Time: " + realTotalTime);
        }
    }

    public void PauseTimer(bool pause)
    {
        isPaused = pause;
    }


}






