using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjectGenerator : MonoBehaviour
{
    public float ratio;  // The ratio of targets to distractors
    public int dimension;   // The dimensionality of the objects (1, 2, or 3)
    public int xObjects;    // The number of objects on the x-axis
    public int yObjects;    // The number of objects on the y-axis
    public int zObjects;    // The number of objects on the z-axis
    public GameObject spherePrefab; // The prefab for the target object
    public GameObject cubePrefab;   // The prefab for the distractor object
    public float objectSize = 0.1f;
    public GameObject centerObject;
    private int numObjects;
    private GameObject[,,] objectMatrix;  // The matrix of objects
    private RandomObjectGenerator rog;

    public RandomObjectGenerator(float ratio, int dimension, int xObjects, int yObjects, int zObjects) {
        this.dimension = dimension;
        this.ratio = ratio;
        this.xObjects = xObjects;
        this.yObjects = yObjects;
        this.zObjects = zObjects;
        this.numObjects = xObjects * yObjects * zObjects;
    }

    public void Populate(GameObject centerObject) {
        objectMatrix = new GameObject[this.xObjects, this.yObjects, this.zObjects];
        float spacing = Mathf.Max(1 - ratio, ratio) * 2f;

        if (this.dimension == 3) {
            for (int x = 0; x < this.xObjects; x++) {
                for (int y = 0; y < this.yObjects; y++) {
                    for (int z = 0; z < this.zObjects; z++) {
                        spacing *= z;
                        
                        if (Random.Range(0f, 1f) < this.ratio) {
                            Vector3 position = new Vector3(centerObject.transform.position.x + x,centerObject.transform.position.y + y, centerObject.transform.position.z + z);
                            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            sphere.transform.position = position;
                            sphere.transform.localScale = Vector3.one * objectSize;
                            
                            Instantiate(sphere, sphere.transform);
                            objectMatrix[x,y,z] = sphere;
                        } else {
                            Vector3 position = new Vector3(centerObject.transform.position.x + x, centerObject.transform.position.y + y, centerObject.transform.position.z + z);
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.transform.position = position;
                            cube.transform.localScale = Vector3.one * objectSize;

                            Instantiate(cube, cube.transform);
                            objectMatrix[x, y, z] = cube;
                        }
                    }
                }
            }
        }
        this.objectMatrix = objectMatrix;
    }

    public void DePopulate() {
        for (int x = 0; x < this.xObjects; x++) {
            for (int y = 0; y < this.yObjects; y++) {
                for (int z = 0; z < this.zObjects; z++) {
                    Destroy(this.objectMatrix[x,y,z]);
                }
            }
        }
    }

    void Start() {
        rog = new RandomObjectGenerator(0.5f, 3, 4, 4, 4);
    }

    void Update() {
        if (OVRInput.GetDown(OVRInput.Button.One)) {
            rog.Populate(centerObject);
        }
        if (OVRInput.GetDown(OVRInput.Button.Two)) {
            rog.DePopulate();
        }
    }
}