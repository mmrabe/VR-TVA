using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public class Experiment : MonoBehaviour {
    // GameObjects
    public Canvas canvas;
    private Transform fixCross;
    public OVRCameraRig camRig;

    // Booleans
    bool isStarted = false;
    bool isFinished = false;
    bool isDebugEnabled;
    bool isCurrentTrialFinished = true;

    // ExperimentSettings
    private List<ExperimentSetting> settings;
    ExperimentSetting currentSetting;
    List<string> lettersD;
    List<string> lettersT;

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

    public Experiment() {
        trialAmount = 2;
        //lettersT = new List<string>() { "a", "b", "c" };
        //lettersD = new List<string>() { "o" };
        timeIntervals = new List<float> { 10f, 20f, 50f, 100f, 150f, 200f };

        ExperimentSetting wholeReportClose = new ExperimentSetting(8, 0, false, false, 0, timeIntervals);
        ExperimentSetting wholeReportFar = new ExperimentSetting(8, 0, true, false, 2, timeIntervals);

        ExperimentSetting partialReportFF = new ExperimentSetting(4, 4, false, false, 0, new List<float> { 150 });
        ExperimentSetting partialReportTF = new ExperimentSetting(4, 4, true, false, 1, new List<float> { 150 });
        ExperimentSetting partialReportFT = new ExperimentSetting(4, 4, false, true, 1, new List<float> { 150 });
        ExperimentSetting partialReportTT = new ExperimentSetting(4, 4, true, true, 1, new List<float> { 150 });

        ExperimentSetting setting1 = new ExperimentSetting(3, 5, true, false, 1);
        settings = new List<ExperimentSetting>() { setting1 };
        //settings = new List<ExperimentSetting>() { wholeReportClose,wholeReportFar,
        //                partialReportFF,partialReportFT,partialReportTF,partialReportTT };

        currentSetting = settings[currentSettingNumber];
        currentTimeInterval = currentSetting.timeIntervals[currentTimeIntervalNumber];
        patternMaskDuration = 0.5f;
        
    }

    // Start is called before the first frame update
    void Start() {

        fixCross = canvas.gameObject.GetComponent<RectTransform>().transform;
        array = new CircularArray();
        array.Init(2f, 8, fixCross);
        array.myObj.transform.position = fixCross.position;
        
    }

    // Update is called once per frame
    void Update()
    {

        array.myObj.transform.rotation = fixCross.rotation;
        array.myObj.transform.position = fixCross.position;
        InputHandler();

        if (!isFinished)
        {
            timer.Update();
            if (!timer.isRunning && isCurrentTrialFinished)
            {
                //if (OVRInput.Get(OVRInput.Button.One)) 
                if (Input.GetKeyDown(KeyCode.Space))
                    {
                    Debug.Log("Key was pressed");
                    NewExperiment();
                }
            }
            else if (!timer.isRunning)
            {
                
                //Debug.Log("time diff timer:" + (Time.time - timer.startTime));
                //Debug.Log("estimated time diff:" + (Time.time - timer.startTime + Time.deltaTime));
                array.Clear();

                //Debug.Log("Cleared");
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
        canvas.transform.LookAt(camRig.transform);
        //Debug.Log("UPDATE WORKS");
        //for (int i = 0; i < array.targets.Count; i++)
        //{
        //    array.targets[i].transform.LookAt(camRig.transform);
        //}

        //for (int i = 0; i < array.distractors.Count; i++)
        //{
        //    array.distractors[i].transform.LookAt(camRig.transform);
        //}

    }


    public void NewExperiment() {
        var ret = GenerateSymbols(currentSetting.numbOfTargets, currentSetting.numbOfDistractors, lettersT, lettersD);
        array.PutIntoSlots(ret.Item1, ret.Item2);
        if (currentSetting.targetsFarAway) { array.PushBack(currentSetting.depth, true); }
        else if (currentSetting.distractorsFarAway) { array.PushBack(currentSetting.depth, false); }
        timer.StartCounting(currentTimeInterval);
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


    public (List<GameObject>, List<GameObject>) GenerateSymbols(int numbOfTargets, int numbOfDistractors, List<string> lettersT, List<string> lettersD)
    {
        List<GameObject> targets = new List<GameObject>();
        List<GameObject> distractors = new List<GameObject>();
        System.Random rnd = new System.Random();


        for (int i = 0; i < numbOfTargets; i++)
        {
            int c = rnd.Next(0, lettersT.Count);
            GameObject newTarget = Instantiate(GameObject.Find(lettersT[c]));
            targets.Add(newTarget);
        }
        for (int i = 0; i < numbOfDistractors; i++)
        {
            int c = rnd.Next(0, lettersD.Count);
            GameObject newDistractor = Instantiate(GameObject.Find(lettersD[c])); 
            distractors.Add(newDistractor);
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
    private Transform fixCross;
    private GameObject[] patternMasks;
    public bool patternMaskEnabled { get; private set; } = false; 

    public CircularArray() {
    }

    private void Update()
    {

    }

    public void Init(float radius, int NumbOfSlots, Transform fixCross = null)
    {
        this.radius = radius;
        myObj = new GameObject();
        numbOfSlots = NumbOfSlots;
        this.fixCross = fixCross;

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
    }

    public void CreatePatternMask()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            GameObject clone = GameObject.Instantiate(patternMask);
            clone.transform.position = fixCross.position + slots[i];
            clone.transform.SetParent(fixCross);
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
        distractors = Distractors;
        targets = Targets;
        System.Random rnd = new System.Random();
        int c = rnd.Next(0, numbOfSlots);
        for (int i = 0; i < targets.Count; i++)
        {
            //targets[i].transform.SetParent(myObj.transform);
            targets[i].transform.SetParent(fixCross);

            while (found.Contains(c))
            { c = rnd.Next(0, numbOfSlots); }
                
            found.Add(c);
            targets[i].transform.position = fixCross.position + slots[c] ;
        }
        for (int i = 0; i < distractors.Count; i++)
        {
            distractors[i].transform.SetParent(fixCross);
            while (found.Contains(c))
            { c = rnd.Next(0, numbOfSlots); }
            found.Add(c);
            distractors[i].transform.position = fixCross.position + slots[c];
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
    }

    public void ChangeRadius(int incr)
    {
        this.radius += incr / 10;
        Init(this.radius, numbOfSlots);
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
    float secondsLeft;
    public bool isRunning { private set; get; } = false;
    public bool isPaused { private set; get; } = false;
    bool isFirstFrame = false;

    public float startTime;
    
    public void StartCounting(float secondsToWait)
    {
        secondsLeft = secondsToWait;
        isRunning = true;
        isFirstFrame = true;
    }
    void Start()
    {
    }

    public void Update()
    {
        if (isFirstFrame)
        {
            isFirstFrame = false;
            startTime = Time.time;
            return;
        }

        if( isRunning && !isPaused)
        {
            
            secondsLeft -= Time.deltaTime;
        }
        if( isRunning &&  (secondsLeft - Time.deltaTime)  < 0)
        {
            isRunning = false;
           
        }
    }

    public void PauseTimer(bool pause)
    {
        isPaused = pause;
    }


}






