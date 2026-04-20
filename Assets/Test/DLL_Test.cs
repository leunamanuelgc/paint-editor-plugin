using System.Runtime.InteropServices;
using UnityEngine;

public class DLL_Test : MonoBehaviour
{
    [DllImport("Dll1", EntryPoint = "TestSort")]
    public static extern void TestSort(int[] a, int length);

    public int[] a = { 10,2,5,6,1,9 };

    private void Start()
    {
        Debug.Log("Before\n------------------------------------------------------");
        PrintArray(a);
        TestSort(a, a.Length);
        Debug.Log("After\n------------------------------------------------------");
        PrintArray(a);
    }

    private void PrintArray(int[] a)
    {
        for (int i=0; i<a.Length; i++)
        {
            Debug.Log(a[i]);
        }
    }
}
