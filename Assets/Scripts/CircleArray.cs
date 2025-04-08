using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleArray : MonoBehaviour {
    // GameObjects
    public List<GameObject> displayedObjects;
    private GameObject patternMask;
    private GameObject highlight;
    private GameObject receptiveField;
    private GameObject[] patternMasks;
    private GameObject[] receptiveFields;
    private GameObject[] highlights;
    private GameObject FixationCrossObject;
    public GameObject stimuliContainer;
    public GameObject highlightContainer;
    public GameObject maskContainer;
    public GameObject receptiveFieldContainer;
    private Transform canvasTransform;
    private Transform canvasUpTrans;
    private Transform canvasRightTrans;
    private Transform canvasBackTrans;
    private Transform canvasCenterTrans;

    // Utility
    private Vector3[] slots;
    private int numbOfSlots;
    private float radius;
    private float stimsize;
    private float distance;
    public bool patternMaskEnabled { get; private set; } = false;
    public bool receptiveFieldEnabled { get; private set; } = false;
    public bool stimuliEnabled { get; private set; } = false;

    private Experiment e;

    public CircleArray() {

    }

    private void Start() {

    }

    void Update() {

    }

    public void Init(float radius, float stimsize, int NumbOfSlots, GameObject FixationCrossObject, float distance, string whichMask, Experiment e, Transform canvasTransform = null) {
        this.radius = (float)(distance*Math.Tan(Math.PI*radius/180f));
        this.stimsize = (float)(2f*(distance*Math.Tan(Math.PI*(radius+stimsize/2f)/180f)-this.radius));
        this.e = e;
        Debug.Log("radius: "+this.radius+", stimsize: "+this.stimsize);
        this.numbOfSlots = NumbOfSlots;
        this.canvasTransform = canvasTransform;
        this.FixationCrossObject = FixationCrossObject;
        this.slots = new Vector3[NumbOfSlots];
        this.canvasRightTrans = GameObject.Find("right").transform;
        this.canvasUpTrans = GameObject.Find("up").transform;
        this.canvasBackTrans = GameObject.Find("back").transform;
        this.canvasCenterTrans = GameObject.Find("center").transform;
        this.patternMask = GameObject.Find("PatternMask"+whichMask);
        this.receptiveField = GameObject.Find("ReceptiveField");
        this.highlight = GameObject.Find("Sphere");
        this.patternMasks = new GameObject[numbOfSlots];
        this.receptiveFields = new GameObject[numbOfSlots];
        this.highlights = new GameObject[numbOfSlots];
        this.distance = distance;
        float angleStep = (float) (2.0f * Math.PI / NumbOfSlots);
        float currAngle = (NumbOfSlots+2)*(-0.25f)*angleStep;
        for (int i = 0; i < NumbOfSlots; i++) {
            float x = (float)(this.radius * Math.Cos(currAngle));
            float y = (float)(this.radius * Math.Sin(currAngle));
            slots[i] = new Vector3(x, y, distance);
            currAngle -= angleStep;
        }

        RadiusToScaleBoxes();
        CreatePatternMask();
        CreateReceptiveField();
        CreateHighlights();
    }

    public void CreatePatternMask() {
        for (int i = 0; i < slots.Length; i++) {
            GameObject clone = GameObject.Instantiate(patternMask);
            clone.transform.SetParent(maskContainer.transform);
            patternMasks[i] = clone;
        }
        
        HidePatternMask();
    }
    public void CreateHighlights() {
        for (int i = 0; i < slots.Length; i++) {
            GameObject clone = GameObject.Instantiate(highlight);
            clone.transform.SetParent(highlightContainer.transform);
            highlights[i] = clone;
        }
        
        HideHighlights();
    }

    

    public void CreateReceptiveField() {
        for (int i = 0; i < slots.Length; i++) {
            GameObject clone = GameObject.Instantiate(receptiveField);
            clone.transform.SetParent(receptiveFieldContainer.transform);
            receptiveFields[i] = clone;
        }
    }

    public void ShowHighlight(int position) {
        highlights[position-1].SetActive(true);
    }

    public void HideHighlights() {
        for(int i = 1; i <= slots.Length; i++) HideHighlight(i);
    }

    public void HideHighlight(int position) {
        highlights[position-1].SetActive(false);
    }

    public void ShowPatternMask()
    {
        maskContainer.SetActive(true);
        patternMaskEnabled = true;
    }

    public void HidePatternMask()
    {
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

    public void ShowFields() {
        foreach(GameObject field in receptiveFields) field.SetActive(true);
    }

    public void ShowField(int position) {
        receptiveFields[position-1].SetActive(true);
    }

    public void HideFields() {
        foreach(GameObject field in receptiveFields) field.SetActive(false);
    }

    public void PutIntoSlot(GameObject obj, int position, float mult, float depth) {

        obj.SetActive(true);

        int c = position - 1;

        Vector3 offsetUp = (canvasCenterTrans.transform.position - canvasUpTrans.position).normalized * slots[c].y;
        Vector3 offsetRight = (canvasCenterTrans.transform.position - canvasRightTrans.position).normalized * slots[c].x;
        Vector3 offsetBack = (canvasCenterTrans.transform.position - canvasBackTrans.position).normalized * slots[c].z;


        obj.transform.position = canvasTransform.position + offsetUp + offsetRight + offsetBack;
        obj.transform.rotation = canvasCenterTrans.transform.rotation;
        obj.transform.SetParent(stimuliContainer.transform);

        this.displayedObjects.Add(obj);

        Vector3 forward = (canvasCenterTrans.transform.position - canvasBackTrans.position).normalized;

        obj.transform.position -= mult * depth * patternMasks[0].transform.lossyScale.x * forward;

        Vector3 backVec = (canvasCenterTrans.transform.position - canvasBackTrans.position).normalized;
        float initialScale = GameObject.Find("ReceptiveField").transform.localScale.z;

        patternMasks[c].transform.position = obj.transform.position;
        highlights[c].transform.position = obj.transform.position;
        receptiveFields[c].transform.position = obj.transform.position;// + (0.1f*backVec);
        obj.transform.position -= backVec * (initialScale / 2);



    }


    public void ChangeSymbolsSize(float s) {
        //Debug.Log("Scale Factor = "+s);
        GameObject maskPrefab = patternMask;
        Vector3 maskPrefabScale = maskPrefab.transform.localScale;
        maskPrefab.transform.localScale = new Vector3(maskPrefabScale.x*s,maskPrefabScale.y*s,maskPrefabScale.z*s);

        GameObject boxPrefab = GameObject.Find("ReceptiveField");
        Vector3 boxPrefabScale = boxPrefab.transform.localScale;
        boxPrefab.transform.localScale = new Vector3(boxPrefabScale.x*s,boxPrefabScale.y*s,boxPrefabScale.z*s);


        foreach(GameObject prefab in e.symbols.Values) {
            Vector3 prefabScale = prefab.transform.localScale;
            prefab.transform.localScale = new Vector3(prefabScale.x*s,prefabScale.y*s,prefabScale.z*s);
        }

        Vector3 local = FixationCrossObject.transform.localScale;
        FixationCrossObject.transform.localScale = new Vector3(local.x*s,local.y*s,local.z*s);
    }

    public void Clear() {

        for (int i = 0; i < displayedObjects.Count; i++) {
            displayedObjects[i].SetActive(false);
            Destroy(displayedObjects[i]);
        }

        displayedObjects.Clear();

        stimuliContainer.transform.DetachChildren();
    }

    public void RadiusToScaleBoxes()
    {
        //float O = (float) (2f *Math.PI * radius);
       //Debug.Log("Omkreds = "+O);
        //float s = (2*O) / (3*8);
        float s = stimsize;
        //Debug.Log("target size  = "+s);
        float scale = s / patternMask.transform.localScale.x;
        ChangeSymbolsSize(scale);
    } 
}
