using System.Net.Sockets;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RemoteConsole
{
    public class ConsoleCommandEditor : EditorWindow
    {
        private ConsoleCommandManager commandManager;

        [MenuItem("Window/Remote Console Commands")]
        private static void Create()
        {;
            var window = GetWindow<ConsoleCommandEditor>();
            window.LoadAvailableConsoleCommands();
            window.Show();
        }

        private void LoadAvailableConsoleCommands()
        {
            commandManager = new ConsoleCommandManager();
        }

        private string GetMethodSignature(MethodInfo method)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(method.Name);
            builder.Append("(");
            List<string> paramStrings = new List<string>();
            foreach (var parameter in method.GetParameters())
            {
                paramStrings.Add(parameter.ParameterType + " " + parameter.Name);
            }
            builder.Append(string.Join(", ", paramStrings.ToArray()));
            builder.Append(")");
            return builder.ToString();
        }

        private void OnGUI()
        {
            foreach (var command in commandManager.ConsoleCommandMap)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(command.Key);
                EditorGUILayout.LabelField("signature: " + GetMethodSignature(command.Value));
                foreach (var parameter in command.Value.GetParameters())
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(parameter.Name + ": ");
                    if (parameter.ParameterType == typeof(int))
                    {
                        EditorGUILayout.IntField(0);
                    }
                    else if (parameter.ParameterType == typeof(string))
                    {
                        EditorGUILayout.TextField("hello");
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Separator();
            }
            if (GUILayout.Button("Send"))
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect("192.168.1.107", 64064);
                    using (var stream = client.GetStream())
                    {
                        string message = "Simple";
                        byte[] buffer = Encoding.UTF8.GetBytes(message);
                        Debug.Log("Sending: " + message);
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Close();
                    }
                }
            }
        }
    }
}