using UnityEngine;
using UnityEngine.UI;
using OculusSampleFramework;

public class RenderRandomObjects : MonoBehaviour
{
    public Camera renderCamera;
    public Canvas canvas;
    public Image canvasImage;
    public OVRInput.Button inputButton;

    public GameObject[] objectsToRender;

    private RectTransform canvasRect;
    private GameObject[] currentObjects;

    void Start()
    {
        // Get the Canvas RectTransform
        canvasRect = canvas.GetComponent<RectTransform>();
        GenerateObjects shape = new GenerateObjects("#FF0000", 5, 10);
        objectsToRender = shape.GeneratedObjects();
        // Initialize the currentObjects array
        currentObjects = new GameObject[objectsToRender.Length];
    }

    void Update()
    {
        if (OVRInput.GetDown(inputButton))
        {
            // Destroy any current objects
            for (int i = 0; i < currentObjects.Length; i++)
            {
                if (currentObjects[i] != null)
                {
                    Destroy(currentObjects[i]);
                }
            }

            // Instantiate new objects
            for (int i = 0; i < objectsToRender.Length; i++)
            {
                // Choose a random object to render
                GameObject objectToRender = objectsToRender[i];

                // Instantiate the object
                currentObjects[i] = Instantiate(objectToRender, transform);

                // Set the canvas Image as the texture for the object material
                currentObjects[i].GetComponent<Renderer>().material.mainTexture = canvasImage.mainTexture;
            }
        }

        // Set the positions of the objects on the canvas
        for (int i = 0; i < currentObjects.Length; i++)
        {
            if (currentObjects[i] != null)
            {
                // Get the screen position of the object
                Vector3 objectScreenPos = renderCamera.WorldToScreenPoint(currentObjects[i].transform.position);

                // Convert screen position to canvas position
                Vector2 canvasPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, objectScreenPos, canvas.worldCamera, out canvasPos);

                // Set the position of the object on the canvas
                RectTransform objectRect = currentObjects[i].GetComponent<RectTransform>();
                objectRect.anchoredPosition = canvasPos;
            }
        }
    }
}