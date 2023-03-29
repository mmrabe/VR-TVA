using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Experiment : MonoBehaviour {
    private List<ExperimentSetting> settings;
    CircularArray array;

    bool isStarted = false;
    List<string> lettersD;
    List<string> lettersT;
    int trialAmount;
    bool isFinished = false;

    bool isDebugEnabled;


    ExperimentSetting currentSetting;
    float currentTimeInterval;
    int trialsSoFar = 0;
    TimeCounter timer = new TimeCounter();

    public Canvas canvas;
    private Transform fixCross;


    int currentSettingNumber;
    int currentTimeIntervalNumber;
    bool isCurrentTrialFinished = true;


    public Experiment()
    {
        trialAmount = 2;
        lettersT = new List<string>() { "a", "b", "c" };
        lettersD = new List<string>() { "o" };
        ExperimentSetting setting1 = new ExperimentSetting(4, 4, false, false, 1);
        settings = new List<ExperimentSetting>() { setting1 };
        currentSetting = settings[currentSettingNumber];
        currentTimeInterval = currentSetting.timeIntervals[currentTimeIntervalNumber];
    }

    // Start is called before the first frame update
    void Start()
    {
        array = new CircularArray();
        array.Init(1.5f, 10);
        //array.myObj.transform.SetParent(fixCross.transform);
        fixCross = canvas.transform;
        array.myObj.transform.position = fixCross.position;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(fixCross.rotation);
        Debug.Log(array.myObj.transform.position);
        array.myObj.transform.rotation = fixCross.rotation;
        array.myObj.transform.position = fixCross.position;
        InputHandler();

        if(!isFinished)
        {
            timer.Update();
            if (!timer.isRunning && isCurrentTrialFinished)
            {
                if ( true ||OVRInput.Get(OVRInput.Button.One))
                {
                    Debug.Log("setting " + currentSettingNumber + " time: " + currentSetting.timeIntervals[currentTimeIntervalNumber] + " trial " + trialsSoFar);
                    var ret = GenerateSymbols(currentSetting.numbOfTargets, currentSetting.numbOfDistractors, lettersT, lettersD);
                    //Debug.Log("Symbols Generated");
                    array.PutIntoSlots(ret.Item1, ret.Item2);
                    //Debug.Log("Symbols in Slots");
                    if (currentSetting.targetsFarAway) { array.PushBack(currentSetting.depth, true); }
                    else if (currentSetting.distractorsFarAway) { array.PushBack(currentSetting.depth, false); }
                    //Debug.Log("Symbols Pushed");
                    timer.StartCounting(currentTimeInterval);
                    //Debug.Log("Timer Started");
                    isCurrentTrialFinished = false;
                }
            }
            else if (!timer.isRunning)
            {
                isCurrentTrialFinished = true;
                //Debug.Log("time diff timer:" + (Time.time - timer.startTime));
                Debug.Log("estimated time diff:" + (Time.time - timer.startTime + Time.deltaTime));
                array.Clear();
                //Debug.Log("Cleared");
                TryToUpdateSetting();

            }

        }
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

    public CircularArray() {
    }

    public void Init(float radius, int NumbOfSlots)
    {
        this.radius = radius;
        myObj = new GameObject();
        numbOfSlots = NumbOfSlots;

        slots = new Vector3[NumbOfSlots];
        Debug.Log(slots.Length);
        float angleStep = 360 / NumbOfSlots;
        float currAngle = 0f;
        for (int i = 0; i < NumbOfSlots; i++)
        {
            float x = (float)(radius * Math.Cos(currAngle));
            float y = (float)(radius * Math.Sin(currAngle));
            slots[i] = new Vector3(x, y, 0.0f);
            currAngle += angleStep;
            Debug.Log("slot " + i + " = " + "(" + x + ";" + y + ")");
        }

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
            targets[i].transform.SetParent(myObj.transform);
           
            while (found.Contains(c))
            { c = rnd.Next(0, numbOfSlots); }
                
            found.Add(c);
            targets[i].transform.position = myObj.transform.position +  slots[c];

        }
        for (int i = 0; i < distractors.Count; i++)
        {
            distractors[i].transform.SetParent(myObj.transform);
            while (found.Contains(c))
            { c = rnd.Next(0, numbOfSlots); }
            found.Add(c);
            distractors[i].transform.position = myObj.transform.position + slots[c];


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



