using UnityEngine;

namespace RemoteConsole
{
    public class Example : MonoBehaviour
    {
        [ConsoleCommand("DoIt")]
        public static string DoSomething(string s, int n, float f, CameraType camType, Color col)
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
}
