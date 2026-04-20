using UnityEngine;
using DrawingTest;

public class Test1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DrawingClass drawingClass = new DrawingClass();
        drawingClass.AddValues(2, 3);
        print("2 + 3 = " + drawingClass.c);
    }

    // Update is called once per frame
    void Update()
    {
        print(DrawingClass.GenerateRandom(0, 100));
    }
}
