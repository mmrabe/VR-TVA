using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Experiment : MonoBehaviour {
    List<ExperimentSetting> settings;
    CircularArray array;
    public GameObject canvas;
    bool isStarted = false;

    private GameObject fixCross;

    public Experiment() 
    {
        Debug.Log("Contructor called");
        int numbOfSettings = 3;
        int trialAmount = 50;
        List<double> times = new List<double>(){5.0, 10.0, 20.0, 50.0};
        ExperimentSetting setting1 = new ExperimentSetting(8, 2, false, false);
        ExperimentSetting setting2 = new ExperimentSetting(5, 5, false, false);
        ExperimentSetting setting3 = new ExperimentSetting(2, 8, false, false);
        //array = new CircularArray();
        //canvas = GameObject.Find("MainCanvas");
        //array.myObj.transform.SetParent(canvas.transform);
        

        List<ExperimentSetting> settings = new List<ExperimentSetting>(){setting1, setting2, setting3};
        this.settings = settings;
    }

    /*public Execute() {
        foreach (ExperiementSetting setting in this.settings) {
            for (int i = 0; i < times.Count; i++) {
                //Populate canvas for times[i] ms
                //Wait for 'ready' check again, to populate new experiment
            }
        }
        //Write configurations to some object
    }

    public Save(string configurations) {
        //Write configurations to .csv file
    }

    public Populate() {
        
    }*/
 
 
    // Start is called before the first frame update
    void Start()
    {
        fixCross = GameObject.Find("FixationCross");

        array = new CircularArray();
        //centerPos = canvas.transform.position;
        array.Init(3,10);
        //array.myObj.transform.SetParent(canvas.transform);
        array.myObj.transform.SetParent(fixCross.transform);
        array.myObj.transform.position = new Vector3(0,0,0); //canvas.transform.position;

        int numbOfObjects = 10; 
        List<GameObject> objects = new List<GameObject>();

        for(int i = 0; i <numbOfObjects; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            objects.Add(sphere);
        }
        
        array.PutIntoSlots(objects);

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("canvas pos: " + canvas.transform.position);
        /*
        if (isReady) {
            Experiment exp = new Experiment()
            exp.Execute()
            exp.Save()
            exp.Delete()
        }
        */

        // if (!isStarted && OVRInput.GetDown(OVRInput.Button.One)) {
        //     isStarted = true;
        // }
        
        // //Debug.Log(array.ToString());

        // array.Init(3,4);

        // int numbOfObjects = 4; 
        // List<GameObject> objects = new List<GameObject>();

        // for(int i = 0; i <numbOfObjects; i++)
        // {
        //     GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //     objects.Add(sphere);
        // }
        
        // array.PutIntoSlots(objects);

    }
}

public class ExperimentSetting {
    int targets;
    int distractors;
    bool targetsFarAway; 
    bool distractorsFarAway;
    
    public ExperimentSetting(int targets, int distractors, bool targetsFarAway,bool distractorsFarAway) {
        this.targets = targets;
        this.distractors = distractors;
        this.targetsFarAway = targetsFarAway;
        this.distractorsFarAway = distractorsFarAway;
    }
}

/*public class Timing {
    // double start_time = Time.time +deltaTime
    // curr_time = time.getTotalTime()
    // if((curr_time - (start_time + deltatime)))
    // {
    //IsTimeUp(){};
    //stopShowingSymbols();

    //Timing.Run();
    //if(Timing.isTimeUp()){        }
    /*if (OVRInput.GetDown(OVRInput.Button.One)) {
        rog.Populate(centerObject);
    }*/


public class CircularArray
{
    public GameObject myObj;
    private Vector3[] slots;
    private int numbOfSlots;
    private List<GameObject> symbols;

    public CircularArray() {
    }

    public void Init(float radius, int NumbOfSlots){
        myObj = new GameObject(); 
        numbOfSlots = NumbOfSlots;

        slots = new Vector3[NumbOfSlots];
        Debug.Log(slots.Length);
        float angleStep = 360/NumbOfSlots;
        float currAngle = 0f;
        //Math.PI / 180 *
        //Math.PI / 180
        for (int i = 0; i < NumbOfSlots; i++){
            float x = (float) (radius * Math.Cos( currAngle)); 
            float y = (float) (radius * Math.Sin(  currAngle)); 
            slots[i] = new Vector3(x,y,0.0f);
            currAngle += angleStep;
            //slots[i] = new Vector3(1,1,0.0f);
            Debug.Log("slot "+i+" = "+"("+x+";"+y+")");
        }
    }

    public void PutIntoSlots(List<GameObject> objects)
    {
        List<int> found = new List<int>(); 
        symbols = objects;
        // for (int i = 0; i < objects.Count; i++) {
        //     objects[i].transform.SetParent(myObj.transform);
        //     System.Random rnd = new System.Random();
        //     int c = rnd.Next(0,numbOfSlots);
        //     if(!found.Contains(c))
        //     {
        //         found.Add(c);
        //         objects[i].transform.position = slots[c];
                
        //     }
        // }
        for (int i = 0; i < objects.Count; i++) {
            objects[i].transform.SetParent(myObj.transform);
            objects[i].transform.position = slots[i];
        }
    }
}
