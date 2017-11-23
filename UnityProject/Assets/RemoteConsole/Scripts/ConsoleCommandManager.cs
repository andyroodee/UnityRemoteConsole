using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine;

public class ConsoleCommandManager
{
    private Dictionary<string, MethodInfo> consoleCommandMap = new Dictionary<string, MethodInfo>();

    public void BuildCommandMap()
    {
        foreach (var type in GetType().Assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (method.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).Any())
                {
                    var attr = (ConsoleCommandAttribute)method.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).First();

                    if (consoleCommandMap.ContainsKey(attr.CommandAlias))
                    {
                        Debug.LogErrorFormat("Duplicate command alias found for: \"{0}\" in type \"{1}\". Already defined in type \"{2}\".",
                                             attr.CommandAlias, type.FullName, consoleCommandMap[attr.CommandAlias].DeclaringType.FullName);
                    }
                    else if (attr.CommandAlias.Any(c => char.IsWhiteSpace(c)))
                    {
                        Debug.LogErrorFormat("Command alias cannot contain whitespace. Found in alias \"{0}\" in type \"{1}\"", 
                                             attr.CommandAlias, type.FullName);
                    }
                    else
                    {
                        Debug.LogFormat("Adding \"{0}\" in type \"{1}\" to the list of console commands.", 
                                        attr.CommandAlias, type.FullName);
                        consoleCommandMap.Add(attr.CommandAlias, method);
                    }
                }
            }
        }
    }

    public string ExecuteCommand(string commandString)
    {
        var parser = new ConsoleCommandParser(commandString);

        MethodInfo method;
        if (!consoleCommandMap.TryGetValue(parser.CommandName, out method))
        {
            Debug.LogErrorFormat("[REMOTE] Unable to find command of type \"{0}\"", parser.CommandName);
            return "";
        }

        var methodParams = method.GetParameters();
        if (parser.CommandParams.Count != methodParams.Length)
        {
            Debug.LogErrorFormat("[REMOTE] Mismatch in parameter count for command \"{0}\". Expected {1}, you provided {2}.", 
                                 parser.CommandName, methodParams.Length, parser.CommandParams.Count);
            return "";
        }

        // Parameter counts match, see if the types are compatible.
        object[] convertedParams = null;
        List<object> convertedParamList = new List<object>(parser.CommandParams.Count);
        try
        {
            for (int i = 0; i < parser.CommandParams.Count; i++)
            {
                var convertedParam = Convert.ChangeType(parser.CommandParams[i], methodParams[i].ParameterType);
                if (convertedParam == null)
                {
                    Debug.LogErrorFormat("Unable to convert parameter at index {0}. The function is expecting a {1}.", 
                                         i, methodParams[i].ParameterType.Name);
                    return "";
                }
                convertedParamList.Add(convertedParam);
            }
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("Unable to convert the parameters in command \"{0}\". Exception: {1}", 
                                 parser.CommandName, e);
            return "";
        }

        // Parameter counts match, and we can convert to the correct types.
        convertedParams = convertedParamList.ToArray();

        try
        {
            var output = method.Invoke(null, convertedParams);
            if (output is string)
            {
                return (string)output;
            }
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("Exception thrown while trying to execute command \"{0}\". Exception: {1}", 
                                 parser.CommandName, e);
            return "";
        }
        return "";
    }
}
