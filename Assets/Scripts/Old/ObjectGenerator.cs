using System.Collections.Generic;
using UnityEngine;

public class ObjectGenerator {
    private string color;
    private int size;
    private int amount;

    public ObjectGenerator(string color, int size, int amount) {
        this.color = color;
        this.size = size;
        this.amount = amount;
    }

    public List<Vector3> GeneratePositions() {
        // Implement position generation logic based on shape, size and amount
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < amount; i++) {
            // Generate random positions based on size and shape parameters
            Vector3 position = new Vector3(
                Random.Range(-size, size),
                Random.Range(-size, size),
                Random.Range(-size, size)
            );

            positions.Add(position);
        }

        return positions;
    }

    public GameObject[] GenerateObjects() {
        // Instantiate game objects based on position generation and shape/color parameters
        GameObject[] objects = new GameObject[amount];

        for (int i = 0; i < amount; i++) {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.GetComponent<Renderer>().material.color = ColorUtility.TryParseHtmlString(color, out Color parsedColor) ? parsedColor : Color.white;
            obj.transform.localScale = new Vector3(size, size, size);
            obj.transform.position = GeneratePositions()[i];
            objects[i] = obj;
        }

        return objects;
    }
}