using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleArray : MonoBehaviour {
    // GameObjects
    public List<GameObject> targets;
    public List<GameObject> distractors;
    private GameObject patternMask;
    private GameObject receptiveField;
    private GameObject[] patternMasks;
    private GameObject[] receptiveFields;
    private GameObject FixationCrossObject;
    public GameObject stimuliContainer;
    public GameObject maskContainer;
    public GameObject receptiveFieldContainer;
    private Transform canvasTransform;
    private Transform canvasUpTrans;
    private Transform canvasRightTrans;

    // Utility
    private Vector3[] slots;
    private int numbOfSlots;
    private float radius;
    // private bool targetsPushed = false;
    // private bool distractorsPushed = false;
    public bool patternMaskEnabled { get; private set; } = false;
    public bool receptiveFieldEnabled { get; private set; } = false;
    public bool stimuliEnabled { get; private set; } = false;
   
    public CircleArray() {

    }

    private void Start() {

    }

    void Update() {

    }

    public void Init(float radius, int NumbOfSlots, GameObject FixationCrossObject, float distance, Transform canvasTransform = null) {
        this.radius = radius;
        this.numbOfSlots = NumbOfSlots;
        this.canvasTransform = canvasTransform;
        this.FixationCrossObject = FixationCrossObject;
        this.slots = new Vector3[NumbOfSlots];
        this.canvasRightTrans = GameObject.Find("right").transform;
        this.canvasUpTrans = GameObject.Find("up").transform;
        this.patternMask = GameObject.Find("PatternMask");
        this.receptiveField = GameObject.Find("ReceptiveField");
        this.patternMasks = new GameObject[numbOfSlots];
        this.receptiveFields = new GameObject[numbOfSlots];
        
        float angleStep = 360 / NumbOfSlots;
        float currAngle = 0f;
        
        for (int i = 0; i < NumbOfSlots; i++) {
            float x = (float)(radius * Math.Cos(currAngle * (Math.PI / 180)));
            float y = (float)(radius * Math.Sin(currAngle * (Math.PI / 180)));
            slots[i] = new Vector3(x, y, distance);
            currAngle += angleStep;
        }

        RadiusToScaleBoxes();
        CreatePatternMask();
        CreateReceptiveField();
    }

    public void CreatePatternMask() {
        for (int i = 0; i < slots.Length; i++) {
            GameObject clone = GameObject.Instantiate(patternMask);
            //clone.transform.position = canvasTransform.position + slots[i];
            clone.transform.SetParent(maskContainer.transform);
            patternMasks[i] = clone;
        }
        //for (int i = 0; i < targets.Count; i++) 
        //{
        //    GameObject clone = GameObject.Instantiate(patternMask);
        //    clone.transform.position = targets[i].transform.position;
        //    clone.transform.SetParent(maskContainer.transform);
        //    patternMasks[i] = clone;

        //}
        //for (int i = 0; i < distractors.Count; i++) {
        //    GameObject clone = GameObject.Instantiate(patternMask);
        //    clone.transform.position = distractors[i].transform.position;
        //    clone.transform.SetParent(maskContainer.transform);
        //    patternMasks[i+targets.Count] = clone;
        //}
            //for (int i = 0; i < slots.Length; i++) {
            //    GameObject clone = GameObject.Instantiate(patternMask);
            //    clone.transform.position = canvasTransform.position + slots[i];
            //    clone.transform.SetParent(maskContainer.transform);
            //    patternMasks[i] = clone;
            //}
        HidePatternMask();
    }

    

    public void CreateReceptiveField() {
        for (int i = 0; i < slots.Length; i++) {
            GameObject clone = GameObject.Instantiate(receptiveField);
            //clone.transform.position = canvasTransform.position + slots[i];
            clone.transform.SetParent(receptiveFieldContainer.transform);
            receptiveFields[i] = clone;
        }
        HideReceptiveField();
    }

    public void ShowReceptiveField() {
        receptiveFieldContainer.SetActive(true);
        receptiveFieldEnabled = true;
    }

    public void HideReceptiveField() {
        receptiveFieldContainer.SetActive(false);
        receptiveFieldEnabled = false;
    }

    public void ShowPatternMask() {
        maskContainer.SetActive(true);
        patternMaskEnabled = true;
    }

    public void HidePatternMask() {
        maskContainer.SetActive(false);
        patternMaskEnabled = false;
    }

    public void ShowStimuli() {
        stimuliContainer.SetActive(true);
        stimuliEnabled = true;
    }

    public void HideStimuli() {
        stimuliContainer.SetActive(false);
        stimuliEnabled = false;
    }

    public void PutIntoSlots(List<GameObject> Targets, List<GameObject> Distractors, bool targetsAway, bool distractorsAway, float depth) {
        this.distractors = Distractors;
        this.targets = Targets;
        
        List<int> found = new List<int>();

       
        System.Random rnd = new System.Random();
        float initialScale = GameObject.Find("PatternMask").transform.localScale.x;
        Vector3 centeringOffset = new Vector3();//0.25f * initialScale, -0.5f * initialScale, 0);
        int c = rnd.Next(0, numbOfSlots);


        for (int i = 0; i < targets.Count; i++) {
            while (found.Contains(c)){ 
                c = rnd.Next(0, numbOfSlots); 
            }
            found.Add(c);
            //usedStimuli.Add(targets[i].name);
            Vector3 offsetUp = (FixationCrossObject.transform.position - canvasUpTrans.position).normalized * slots[c].y;
            Vector3 offsetRight = (FixationCrossObject.transform.position - canvasRightTrans.position).normalized * slots[c].x;
            

            targets[i].transform.position = canvasTransform.position + offsetUp + offsetRight + centeringOffset;
            targets[i].transform.rotation = FixationCrossObject.transform.rotation;
            targets[i].transform.SetParent(stimuliContainer.transform);
            
        }
        for (int i = 0; i < distractors.Count; i++) {
            while (found.Contains(c)) { 
                c = rnd.Next(0, numbOfSlots); 
            }
            found.Add(c);

            Vector3 offsetUp = (FixationCrossObject.transform.position - canvasUpTrans.position).normalized * slots[c].y;
            Vector3 offsetRight = (FixationCrossObject.transform.position - canvasRightTrans.position).normalized * slots[c].x;

            distractors[i].transform.position = canvasTransform.position + offsetUp + offsetRight + centeringOffset;
            distractors[i].transform.rotation = FixationCrossObject.transform.rotation;
            distractors[i].transform.SetParent(stimuliContainer.transform);
            
        }

        if (targetsAway) { 
            PushBack(depth, "T"); 
        } else if (distractorsAway) { 
            PushBack(depth, "D"); 
        }

        for(int i = 0; i < patternMasks.Length; i++) {
            Debug.Log("INDEX: " + i);
            patternMasks[i].transform.position = stimuliContainer.transform.GetChild(i).transform.position - centeringOffset;
            receptiveFields[i].transform.position = stimuliContainer.transform.GetChild(i).transform.position - centeringOffset;
        }

    }

    public void PushBack(float depth, String TorD) {
        Vector3 v = new Vector3(0, 0, depth);

        if (TorD == "T") {
            for (int i = 0; i < targets.Count; i++) {
                targets[i].transform.position -= v;
            }
        } else if (TorD == "D") {
            for (int i = 0; i < distractors.Count; i++) {
                distractors[i].transform.position -= v;
            }
        }

        //float mask_depth = Math.Max(targets[0].transform.position.z, distractors[0].transform.position.z);

        //for (int i = 0; i < patternMasks.Length; i++) {
        //    patternMasks[i].transform.position = new Vector3(patternMasks[i].transform.position.x,
        //                                                     patternMasks[i].transform.position.y,
        //                                                     mask_depth);
        //}
    }

    public void ChangeSymbolsSize(float s) {
        Debug.Log("Scale Factor = "+s);
        GameObject maskPrefab = GameObject.Find("PatternMask");
        Vector3 maskPrefabScale = maskPrefab.transform.localScale;
        maskPrefab.transform.localScale = new Vector3(maskPrefabScale.x*s,maskPrefabScale.y*s,maskPrefabScale.z*s);

        GameObject boxPrefab = GameObject.Find("ReceptiveField");
        Vector3 boxPrefabScale = boxPrefab.transform.localScale;
        boxPrefab.transform.localScale = new Vector3(boxPrefabScale.x*s,boxPrefabScale.y*s,boxPrefabScale.z*s);

        GameObject lettersRedContainer = GameObject.Find("LettersRed");
        for(int i = 0; i < lettersRedContainer.transform.childCount; i++)
        {
            GameObject lettersRedPrefab = lettersRedContainer.transform.GetChild(i).gameObject;
            Vector3 lettersRedPrefabScale = lettersRedPrefab.transform.localScale;
            lettersRedPrefab.transform.localScale = new Vector3(lettersRedPrefabScale.x*s,lettersRedPrefabScale.y*s,lettersRedPrefabScale.z*s);            
        }

        GameObject lettersBlueContainer = GameObject.Find("LettersBlue");
        for(int i = 0; i < lettersRedContainer.transform.childCount; i++) 
        {
            GameObject lettersBluePrefab = lettersBlueContainer.transform.GetChild(i).gameObject;
            Vector3 lettersBluePrefabScale = lettersBluePrefab.transform.localScale;
            lettersBluePrefab.transform.localScale = new Vector3(lettersBluePrefabScale.x*s,lettersBluePrefabScale.y*s,lettersBluePrefabScale.z*s);
        }

        Vector3 local = FixationCrossObject.transform.localScale;
        FixationCrossObject.transform.localScale = new Vector3(local.x*s,local.y*s,local.z*s);
    }

    public void ChangeRadius(int incr) {
        this.radius += incr / 10;
        
        Init(this.radius, numbOfSlots, FixationCrossObject,0f);
        
        for (int i = 0; i < targets.Count; i++) {
            targets[i].transform.position = this.transform.position + slots[i];
        }
        
        for (int i = 0; i < distractors.Count; i++) {
            distractors[i].transform.position = this.transform.position + slots[i + targets.Count - 1];
        }
    }

    public void ChangeDepth(int incr) {
        PushBack(incr / 10, "T");
        PushBack(incr / 10, "D");
    }

    public void ChangeDistance(int incr) {
        gameObject.transform.position += new Vector3(0, 0, incr / 10);
    }

    public void Clear() {
        for (int i = 0; i < targets.Count; i++) {
            Destroy(targets[i]);
        }

        for (int i = 0; i < distractors.Count; i++) {
            Destroy(distractors[i]);
        }
    }

    public void RadiusToScaleBoxes()
    {
        float O = (float) (2f *Math.PI * radius);
        Debug.Log("Omkreds = "+O);
        float s = (2*O) / (3*8);
        Debug.Log("target size  = "+s);
        float scale = s / GameObject.Find("PatternMask").transform.localScale.x;
        ChangeSymbolsSize(scale);
    } 
}
