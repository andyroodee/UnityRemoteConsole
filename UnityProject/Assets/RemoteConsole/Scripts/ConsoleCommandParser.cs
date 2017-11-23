using System.Collections.Generic;
using System.Text;

public class ConsoleCommandParser
{
    public string CommandName { get; private set; }
    public List<string> CommandParams { get; private set; }

    public ConsoleCommandParser(string commandString)
    {
        CommandParams = new List<string>();
        string trimmedCommandString = commandString.Trim();
        ParseCommandString(trimmedCommandString);
    }

    private void ParseCommandString(string commandString)
    {
        int index = 0;
        // The command name is the first string in the command, so find where it ends.
        while (index < commandString.Length && char.IsLetterOrDigit(commandString, index))
        {
            index++;
        }
        CommandName = commandString.Substring(0, index);

        // Read the parameters
        for (; index < commandString.Length; index++)
        {
            if (char.IsWhiteSpace(commandString, index))
            {
                continue;
            }
            if (commandString[index] == '"')
            {
                index++; // Skip the start quote
                ReadStringParam(commandString, ref index);
            }
            else
            {
                ReadParam(commandString, ref index);
            }
        }
    }

    private void ReadStringParam(string commandString, ref int index)
    {
        StringBuilder builder = new StringBuilder();
        for (; index < commandString.Length; index++)
        {
            // Include everything until we hit an unescaped closing quote.
            if (index > 0 && commandString[index] == '"' && commandString[index - 1] != '\\')
            {
                if (builder.Length > 0)
                {
                    CommandParams.Add(builder.ToString());
                }
                return;
            }
            else
            {
                builder.Append(commandString[index]);
            }
        }
    }

    private void ReadParam(string commandString, ref int index)
    {
        // Read until we hit whitespace
        StringBuilder builder = new StringBuilder();
        for (; index < commandString.Length; index++)
        {
            if (!char.IsWhiteSpace(commandString[index]))
            {
                builder.Append(commandString[index]);
            }
            else
            {
                break;
            }
        }
        if (builder.Length > 0)
        {
            CommandParams.Add(builder.ToString());
        }
    }
}
