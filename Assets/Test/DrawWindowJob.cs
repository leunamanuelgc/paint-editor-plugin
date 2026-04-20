using Unity.Jobs;
using UnityEngine;
using System.Runtime.InteropServices;

public class DrawWindowJob: IJob
{
    [DllImport("Dll2", EntryPoint = "DrawWindow")]
    public static extern void DrawWindow();
    public void Execute()
    {
        DrawWindow();
    }
}
