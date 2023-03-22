using UnityEngine;
using System;

public class RenderObjects : MonoBehaviour
{
    [SerializeField] private OVRCameraRig cameraRig;
    private GenerateObjects generator;
    private GameObject[] objects;

    public static void ObjectRenderer(GameObject[] objects, OVRCameraRig cameraRig)
    {
        foreach (GameObject obj in objects)
        {
            obj.transform.SetParent(cameraRig.centerEyeAnchor);
            obj.layer = LayerMask.NameToLayer("UI"); // Make sure the object is on a UI layer

            // Add a canvas component to the object if it doesn't already have one
            if (!obj.GetComponent<Canvas>())
            {
                obj.AddComponent<Canvas>();
            }

            // Set the canvas properties
            Canvas canvas = obj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cameraRig.centerEyeAnchor.GetComponent<Camera>();
            canvas.sortingOrder = 0;

            // Add a graphic component to the canvas
            GameObject graphic = new GameObject();
            graphic.transform.SetParent(obj.transform);
            //graphic.AddComponent<RectTransform>();
            //graphic.AddComponent<UnityEngine.UI.Image>();
            //graphic.GetComponent<UnityEngine.UI.Image>().color = obj.GetComponent<ObjectProperties>().Color;
            //graphic.GetComponent<RectTransform>().sizeDelta = new Vector2(obj.GetComponent<ObjectProperties>().Size, obj.GetComponent<ObjectProperties>().Size);
        }
    }

    private void Start()
    {
        // Generate objects using the ObjectGenerator class
        generator = new GenerateObjects("#FF0000", 5, 10);
        objects = generator.GeneratedObjects();

        // Render objects using the ObjectRenderer class
        //ObjectRenderer renderer = new ObjectRenderer();
        ObjectRenderer(objects, cameraRig);
    }

    private void Update()
    {
        // Rotate objects around the y-axis
        foreach (GameObject obj in objects)
        {
            obj.transform.Rotate(0f, 1f, 0f);
        }

       
       // Reload the scene when the R key is pressed
       // if (Input.GetKeyDown(KeyCode.R))
       // {
       //     Scene scene = SceneManager.GetActiveScene();
       //     SceneManager.LoadScene(scene.name);
       // }*///
    }
}