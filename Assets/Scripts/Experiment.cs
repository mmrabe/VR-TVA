



using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Threading.Tasks;

public class Experiment : MonoBehaviour
{

    // GameObjects
    public Canvas canvas;
    public Transform canvasCenter;
    public OVRCameraRig camRig;
    public Dictionary<(string, string), GameObject> symbols = new Dictionary<(string,string), GameObject>();
    public GameObject FixationCrossObject;

    public List<TextAsset> Recipes;
    private int currentRecipe = 0;

    // Booleans

    public bool UsePhysicalKeyboard = false;

    // ExperimentSettings

    public OVRPassthroughLayer passthroughLayer;

    // Utility
    public CircleArray array;

    public Text textBox;
    public Text textBoxTop;
    public Text textBoxBottom;




    public ExperimentRoot Procedure;

    public Experiment()
    {



    }

    public float CurrentFrameRate { get => OVRPlugin.systemDisplayFrequency != null && OVRPlugin.systemDisplayFrequency >= 1f ? OVRPlugin.systemDisplayFrequency : Application.targetFrameRate ;}

    
    private void DisplayRefreshRateChanged (float fromRefreshRate, float ToRefreshRate)
    {
        // Handle display refresh rate changes
        Debug.Log(string.Format("Refresh rate changed from {0} to {1}", fromRefreshRate, ToRefreshRate));
    }

    private TouchScreenKeyboard subjectNumberKeyboard;

    void Start() 
    {
        

        passthroughLayer.enabled = false;

        Debug.Log("Starting experiment...");
        OVRManager.DisplayRefreshRateChanged += DisplayRefreshRateChanged;
        
        Application.targetFrameRate = 120;
        OVRPlugin.systemDisplayFrequency = 120.0f;
        try {
            UnityEngine.Debug.Log("Current fps: "+OVRPlugin.systemDisplayFrequency+", available fps: " +  OVRPlugin.systemDisplayFrequenciesAvailable);
        } catch {
            UnityEngine.Debug.Log("Could not determine supported fps!");
        }





        this.canvasCenter = canvas.gameObject.GetComponent<RectTransform>().transform;

        string[] letters = new string[]{"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
        string[] colors = new string[]{"red","blue","yellow","green"};
        foreach(string color in colors) {
            GameObject parent = GameObject.Find(color + "Letters");
            foreach(string letter in letters) {
                symbols[(color,letter)] = GameObject.Find($"{color}Letters/{letter}").gameObject;
            }
        }

        // Find FixationCross
        FixationCrossObject = GameObject.Find("FixationCross");
        FixationCrossObject.SetActive(false);
        // Initialize circular array and set it's position.
        //float radius = canvas.planeDistance * DistanceToArraySizeRatio;
        
        FixationCrossObject.transform.position = new Vector3(FixationCrossObject.transform.position.x, FixationCrossObject.transform.position.y, -5);
        textBox.transform.position = FixationCrossObject.transform.position;
        textBoxTop.transform.position = FixationCrossObject.transform.position;
        textBoxBottom.transform.position = FixationCrossObject.transform.position;


        textBoxTop.gameObject.SetActive(true);
        textBoxTop.text = "Enter subject number:";
        textBox.text = "";
        textBox.gameObject.SetActive(true);

        subjectNumberKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad, false);

        //Procedure.Prepare();
        //Procedure.Start();

    }


    private void RunBackgroundTasks() {
        while(ScheduledBackgroundTasks.Count > 0) ScheduledBackgroundTasks.Dequeue()();
    }

    public List<Stopclock> CurrentTimers {get;} = new List<Stopclock>();

    private Queue<Action> TaskOnNextFrame = new Queue<Action>();

    private Queue<Action> ScheduledBackgroundTasks = new Queue<Action>();

    private Task? BackgroundTaskWorker = null;
    private object BackgroundTaskWorkerLock = new object();

    public void EnqueueTaskOnNextFrame(Action a) {
        TaskOnNextFrame.Enqueue(a);
    }

    public void EnqueueBackgroundTask(Action a) {
        ScheduledBackgroundTasks.Enqueue(a);
    }

    void Update()
    {



        // calculate time in ms since last frame
        float deltaTime = Time.deltaTime*1000f;

        // increment all active timers by that duration
        foreach(var item in CurrentTimers) {
            item.Increment(deltaTime);
        }

        // execute queued time-critical tasks if any (e.g., record trial display duration)
        while(TaskOnNextFrame.Count > 0) TaskOnNextFrame.Dequeue()();

        // execute queued non-critical tasks if any (e.g., write to output file)
        if(ScheduledBackgroundTasks.Count > 0) {
            lock(BackgroundTaskWorkerLock) {
                if(BackgroundTaskWorker == null || BackgroundTaskWorker.Status != TaskStatus.Running) {
                    BackgroundTaskWorker = Task.Run(this.RunBackgroundTasks);
                }
            }
        }

        
        
        if (Procedure != null && Procedure.IsFinished) {
            return;
        } else if(Procedure != null &&Procedure.State >= TrialType.TrialState.Started) {
            // if there is an unfinished experimental procedure, propagate update signal to active trial
            Procedure.Update();
        } else {
            textBox.text = "Subject: " + subjectNumberKeyboard.text + "\r\nProcedure: "+Recipes[currentRecipe].name;
            if(OVRInput.GetUp(OVRInput.Button.One) || Input.GetKeyUp(KeyCode.RightArrow)) {
                currentRecipe++;
                if(currentRecipe >= Recipes.Count()) currentRecipe = 0;
            }
            if(OVRInput.GetUp(OVRInput.Button.Two) || Input.GetKeyUp(KeyCode.LeftArrow)) {
                currentRecipe--;
                if(currentRecipe < 0) currentRecipe = Recipes.Count() - 1;
            }
            if(OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger) || Input.GetKeyUp(KeyCode.Return)) {
                if(Input.GetKeyUp(KeyCode.Return)) UsePhysicalKeyboard = true;
                Procedure = ExperimentRoot.Load(Recipes[currentRecipe], this);
                FixationCrossObject.transform.position = new Vector3(FixationCrossObject.transform.position.x, FixationCrossObject.transform.position.y, -Procedure.ArrayDistance);
                textBox.transform.position = FixationCrossObject.transform.position;
                textBoxTop.transform.position = FixationCrossObject.transform.position;
                textBoxBottom.transform.position = FixationCrossObject.transform.position;
                array.Init(Procedure.ArrayRadius, Procedure.StimulusSize, Procedure.NumberOfSlots, FixationCrossObject, Procedure.ArrayDistance, Procedure.WhichMask, this, canvasCenter);
                array.transform.position = canvasCenter.position;
                Debug.Log(Procedure.ToString());
                Debug.Log("Entered subject number: "+subjectNumberKeyboard.text);
                Procedure.OutputFilePath = "Output_" + subjectNumberKeyboard.text + ".csv";
                textBox.gameObject.SetActive(false);
                textBoxTop.gameObject.SetActive(false);
                Procedure.Prepare();
                Procedure.Start();
            }
        }


    }


}
