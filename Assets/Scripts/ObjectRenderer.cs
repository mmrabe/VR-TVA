using UnityEngine;

public class ObjectRenderer : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera camera;

    GameObject[] objects;

    public void RenderObjects(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            // Set the parent of the object to the canvas
            obj.transform.SetParent(canvas.transform, false);

            // Attach the canvas to the camera
            canvas.worldCamera = camera;

            // Set the position of the object in screen space
            Vector2 position = camera.WorldToScreenPoint(obj.transform.position);
            obj.GetComponent<RectTransform>().anchoredPosition = position;
        }
    }

        private void Start()
    {
        // Generate objects using the Shape class
        ObjectGenerator objectGenerator = new ObjectGenerator("#FF0000", 5, 10);
        objects = objectGenerator.GenerateObjects();

        // Render objects using the ObjectRenderer class
        RenderObjects(objects);
    }

    private void Update()
    {
        // Rotate objects around the y-axis
        foreach (GameObject obj in objects)
        {
            obj.transform.Rotate(0f, 1f, 0f);
        }
    }
}