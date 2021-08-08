using System;

public interface IDebugCommand
{
    public string CommandId { get; set; }

    public string CommandDescription { get; set; }

    public string CommandFormat { get; set; }

    public void Invoke ();
}

public class DebugCommand : IDebugCommand
{
    public string CommandId { get; set; }

    public string CommandDescription { get; set; }

    public string CommandFormat { get; set; }

    private readonly Action m_command;

    public DebugCommand ( string id, string description, string format, Action command )
    {
        CommandId = id;
        CommandDescription = description;
        CommandFormat = format;
        m_command = command;
    }

    void IDebugCommand.Invoke ()
    {
        m_command?.Invoke ();
    }
}

public class DebugCommand<T> : IDebugCommand
{
    public string CommandId { get; set; }

    public string CommandDescription { get; set; }

    public string CommandFormat { get; set; }

    private readonly Action<T> m_command = null;
    private T m_value = default;

    public DebugCommand ( string id, string description, string format, Action<T> command )
    {
        CommandId = id;
        CommandDescription = description;
        CommandFormat = format;
        m_command = command;
    }

    public void Invoke ( T value )
    {
        if ( value == null ) return;
        m_value = value;
        Invoke ();
    }

    public void Invoke ()
    {
        m_command?.Invoke ( m_value );
    }
}
