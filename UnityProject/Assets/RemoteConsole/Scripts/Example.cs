using UnityEngine;

public class Example : MonoBehaviour
{
    [ConsoleCommand("DoIt")]
    public static string DoSomething(string s, int n, float f)
    {
        Debug.Log("Doing something with " + s + ", " + n + ", " + f);
        return "you did something\n";
    }

    [ConsoleCommand("Simple")]
    public static void AnotherExample()
    {
        Debug.Log("Another example");
    }
}