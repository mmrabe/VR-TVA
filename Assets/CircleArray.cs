using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CircleArray : MonoBehaviour
{
    //public GameObject myObj;
    private Vector3[] slots;
    private int numbOfSlots;
    public List<GameObject> targets;
    public List<GameObject> distractors;
    private float radius;
    private bool targetsPushed = false;
    private bool distractorsPushed = false;
    private GameObject patternMask;
    private Transform canvasTransform;
    private GameObject[] patternMasks;
    public bool patternMaskEnabled { get; private set; } = false;
    public bool stimuliEnabled { get; private set; } = false;
    private GameObject FixationCrossObject;
    private Transform canvasUpTrans;
    private Transform canvasRightTrans;
    public GameObject stimuliContainer;
    public GameObject maskContainer;


    public CircleArray()
    {
    }

    private void Start()
    {

    }

    void Update()
    {

    }

    public void Init(float radius, int NumbOfSlots, GameObject FixationCrossObject, Transform canvasTransform = null)
    {
        this.radius = radius;
        //myObj = new GameObject();
        numbOfSlots = NumbOfSlots;
        this.canvasTransform = canvasTransform;
        this.FixationCrossObject = FixationCrossObject;

        slots = new Vector3[NumbOfSlots];
        //Debug.Log(slots.Length);
        float angleStep = 360 / NumbOfSlots;
        float currAngle = 0f;
        for (int i = 0; i < NumbOfSlots; i++)
        {
            float x = (float)(radius * Math.Cos(currAngle * (Math.PI / 180)));
            float y = (float)(radius * Math.Sin(currAngle * (Math.PI / 180)));
            slots[i] = new Vector3(x, y, 0);
            currAngle += angleStep;
//            Debug.Log("slot " + i + " = " + "(" + x + ";" + y + ")");
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
            clone.transform.SetParent(maskContainer.transform);
            patternMasks[i] = clone;
        }
        HidePatternMask();
    }

    public void ShowPatternMask()
    {
        //for (int i = 0; i < patternMasks.Length; i++)
        //{
        //    patternMasks[i].GetComponent<MeshRenderer>().enabled = true;
        //}
        maskContainer.SetActive(true);
        patternMaskEnabled = true;
    }

    public void HidePatternMask()
    {
        //for (int i = 0; i < patternMasks.Length; i++)
        //{
        //    patternMasks[i].GetComponent<MeshRenderer>().enabled = false;
        //}
        maskContainer.SetActive(false);
        patternMaskEnabled = false;
    }

    public void ShowStimuli()
    {
        //for (int i = 0; i < targets.Count; i++)
        //{
        //    targets[i].GetComponent<MeshRenderer>().enabled = true;
        //}
        //for (int i = 0; i < distractors.Count; i++)
        //{
        //    distractors[i].GetComponent<MeshRenderer>().enabled = true;
        //}
        stimuliContainer.SetActive(true);
        stimuliEnabled = true;
    }

    public void HideStimuli()
    {
        //for (int i = 0; i < targets.Count; i++)
        //{
        //    targets[i].GetComponent<MeshRenderer>().enabled = false;
        //}
        //for (int i = 0; i < distractors.Count; i++)
        //{
        //    distractors[i].GetComponent<MeshRenderer>().enabled = false;
        //}

        stimuliContainer.SetActive(false);
        stimuliEnabled = false;
    }


    public void PutIntoSlots(List<GameObject> Targets, List<GameObject> Distractors, float depth)
    {
        List<int> found = new List<int>();
        float initialScale = GameObject.Find("PatternMask").transform.localScale.x;
        distractors = Distractors;
        targets = Targets;
        System.Random rnd = new System.Random();
        int c = rnd.Next(0, numbOfSlots);
        for (int i = 0; i < targets.Count; i++)
        {
            //targets[i].transform.SetParent(canvasTransform);

            while (found.Contains(c))
            { c = rnd.Next(0, numbOfSlots); }

            found.Add(c);
            Vector3 offsetUp = (FixationCrossObject.transform.position - canvasUpTrans.position).normalized * slots[c].y;
            Vector3 offsetRight = (FixationCrossObject.transform.position - canvasRightTrans.position).normalized * slots[c].x;

            targets[i].transform.position = canvasTransform.position + offsetUp + offsetRight + new Vector3(0.25f * initialScale, -0.5f * initialScale, 0);
            targets[i].transform.rotation = FixationCrossObject.transform.rotation;
            targets[i].transform.SetParent(stimuliContainer.transform);
        }
        for (int i = 0; i < distractors.Count; i++)
        {
            //distractors[i].transform.SetParent(canvasTransform);
            while (found.Contains(c))
            { c = rnd.Next(0, numbOfSlots); }
            found.Add(c);
            Vector3 offsetUp = (FixationCrossObject.transform.position - canvasUpTrans.position).normalized * slots[c].y;
            Vector3 offsetRight = (FixationCrossObject.transform.position - canvasRightTrans.position).normalized * slots[c].x;
            distractors[i].transform.position = canvasTransform.position + offsetUp + offsetRight + new Vector3(0.25f * initialScale, -0.5f * initialScale, 0);
            distractors[i].transform.rotation = FixationCrossObject.transform.rotation;
            distractors[i].transform.SetParent(stimuliContainer.transform);
        }

        if (targetsPushed) { PushBack(depth, true); }
        else if (distractorsPushed) { PushBack(depth, false); }
    }

    public void PushBack(float depth, bool pushTargets)
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
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].transform.localScale += new Vector3(s, s, s);
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
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].transform.position = this.transform.position + slots[i];
        }
        for (int i = 0; i < distractors.Count; i++)
        {
            distractors[i].transform.position = this.transform.position + slots[i + targets.Count - 1];
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
