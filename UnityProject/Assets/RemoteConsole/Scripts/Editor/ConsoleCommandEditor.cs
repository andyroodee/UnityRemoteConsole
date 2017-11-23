using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RemoteConsole
{
    public class ConsoleCommandEditor : EditorWindow
    {
        private ConsoleCommandManager commandManager;

        //[SerializeField]
       // private Dictionary<string, Dictionary<ParameterInfo, object>> commandParamValues;// = new Dictionary<string, Dictionary<ParameterInfo, object>>();

        // These inner classes and their functions could be replaced by a single Dictionary
        // of type Dictionary<string, Dictionary<ParameterInfo, object>>, but Unity cannot serialize Dictionaries.
        [Serializable]
        class ParamValue
        {
            [SerializeField]
            public string type;// { get; set; }

            [SerializeField]
            public object Value;// { get; set; }
        }

        [Serializable]
        class CommandData
        {
            [SerializeField]
            public string Name;// { get; set; }

            [SerializeField]
            public List<ParamValue> ParamValues;// { get; set; }

            public CommandData(string name)
            {
                Name = name;
                ParamValues = new List<ParamValue>();
            }
        }

        [Serializable]
        class Commands
        {
            [SerializeField]
            private List<CommandData> commands;

            public Commands()
            {
                commands = new List<CommandData>();
            }

            public void AddCommandData(CommandData data)
            {
                commands.Add(data);
            }

            public CommandData GetCommandData(string name)
            {
                return commands.Find(command => command.Name == name);
            }
        }

        private Commands commands;

        [MenuItem("Window/Remote Console Commands")]
        private static void Create()
        {;
            var window = GetWindow<ConsoleCommandEditor>();
            window.LoadAvailableConsoleCommands();
            window.Show();
        }

        private void LoadAvailableConsoleCommands()
        {
            //commandParamValues = new Dictionary<string, Dictionary<ParameterInfo, object>>();
            Debug.Log("Creating");
            hideFlags = HideFlags.HideAndDontSave;
            commands = new Commands();
            commandManager = new ConsoleCommandManager();
            foreach (var command in commandManager.ConsoleCommandMap)
            {
                CommandData data = new CommandData(command.Key);
                foreach (var parameter in command.Value.GetParameters())
                {
                    ParamValue paramVal = new ParamValue();
                    paramVal.type = parameter.ParameterType.Name;
                    paramVal.Value = parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null;
                    data.ParamValues.Add(paramVal);
                }
                commands.AddCommandData(data);
            }
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
            if (commandManager == null) 
            {
                Debug.Log("Recreating commandmanager");
                commandManager = new ConsoleCommandManager();
            }

            foreach (var command in commandManager.ConsoleCommandMap)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(command.Key);
                EditorGUILayout.LabelField("signature: " + GetMethodSignature(command.Value));

                foreach (var parameter in command.Value.GetParameters())
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(parameter.Name + ": ");
                    UpdatePropertyValue(command.Key, parameter);
                    EditorGUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Send"))
                {
                    using (TcpClient client = new TcpClient())
                    {
                        client.Connect("192.168.1.107", 64064);
                        using (var stream = client.GetStream())
                        {
                            string message = GetCommandString(command.Key);
                            byte[] buffer = Encoding.UTF8.GetBytes(message);
                            Debug.Log("Sending: " + message);
                            stream.Write(buffer, 0, buffer.Length);
                            stream.Close();
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Separator();
            }
        }

        private string GetCommandString(string commandKey)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(commandKey);

            var command = commands.GetCommandData(commandKey);
            foreach (var paramValue in command.ParamValues)
            {
                builder.Append(' ');
                builder.Append(paramValue.Value.ToString());
            }

            return builder.ToString();
        }

        private void UpdatePropertyValue(string commandKey, ParameterInfo paramInfo)
        {
            var command = commands.GetCommandData(commandKey);
            var targetParam = command.ParamValues.Find(param => param.type == paramInfo.ParameterType.Name);
            var currentValue = targetParam.Value;

            if (paramInfo.ParameterType == typeof(int))
            {
                targetParam.Value = EditorGUILayout.IntField((int)currentValue);
            }
            else if (paramInfo.ParameterType == typeof(long))
            {
                targetParam.Value = EditorGUILayout.LongField((long)currentValue);
            }
            else if (paramInfo.ParameterType == typeof(float))
            {
                targetParam.Value = EditorGUILayout.FloatField((float)currentValue);
            }
            else if (paramInfo.ParameterType == typeof(double))
            {
                targetParam.Value = EditorGUILayout.DoubleField((double)currentValue);
            }
            else if (paramInfo.ParameterType == typeof(string))
            {
                targetParam.Value = EditorGUILayout.TextField((string)currentValue);
            }
            else if (paramInfo.ParameterType == typeof(Color))
            {
                targetParam.Value = EditorGUILayout.ColorField((Color)currentValue);
            }
            else if (paramInfo.ParameterType.IsEnum)
            {
                targetParam.Value = EditorGUILayout.EnumPopup((Enum)currentValue);
            }
            else
            {
                Debug.Log("Can't handle: " + currentValue);
                //commandParamValues[commandKey][paramInfo] = EditorGUILayout.TextField((string)currentValue);
            }
        }
    }
}