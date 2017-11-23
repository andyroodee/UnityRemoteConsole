using System;

[AttributeUsage(AttributeTargets.Method)]
public class ConsoleCommandAttribute : Attribute
{
    public string CommandAlias { get; private set; }

    public ConsoleCommandAttribute(string alias)
    {
        CommandAlias = alias;
    }
}