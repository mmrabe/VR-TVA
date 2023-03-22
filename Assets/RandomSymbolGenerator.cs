using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.UI;

public class RandomSymbolGenerator : MonoBehaviour
{
    public Text canvasText;
    public Canvas canvas;
    private float ratio;
    private int rows;
    private int cols;
    private RandomSymbolGenerator g;

    public RandomSymbolGenerator(int rows, int cols, float ratio) {
            this.ratio = ratio;
            this.rows = rows;
            this.cols = cols;

            Debug.Assert(ratio <= 1.0 && ratio >= 0.0);
    }

    public string Populate() {
            char[,] matrix = new char[this.rows, this.cols];
            string matrixStr = "";
            System.Random rnd = new System.Random();

            for (int r = 0; r < this.rows; r++) {
                for (int c = 0; c < this.cols; c++) {
                    double prob = rnd.NextDouble();
            
                    if (prob >= this.ratio) {
                        matrix[r, c] = (char)rnd.Next(65, 91); // generate a random uppercase letter
                        matrixStr += matrix[r, c] + " ";
                    } else {
                        matrix[r, c] = '0';
                        matrixStr += "0 ";
                    }
                }
                matrixStr += "\n";
            }
            return matrixStr;
        }

    void Start() {
        g = new RandomSymbolGenerator(8, 8, 0.8f);
    }

    void Update() {
        if (OVRInput.GetDown(OVRInput.Button.One)){
           canvasText.text = g.Populate();
        }
    }
}
