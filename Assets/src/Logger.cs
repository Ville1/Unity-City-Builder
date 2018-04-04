using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

public class Logger {
    private static Logger instance;

    private Logger()
    {
        
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    public static Logger Instance
    {
        get {
            if(instance == null) {
                instance = new Logger();
            }
            return instance;
        }
        private set {
            return;
        }
    }

    /// <summary>
    /// Log debug message
    /// </summary>
    /// <param name="message"></param>
    public void Debug(string message)
    {
        StackTrace trace = new StackTrace();
        StackFrame frame = trace.GetFrame(1);
        UnityEngine.Debug.Log("DEBUG - " + frame.GetMethod().ReflectedType.Name + " -> " + frame.GetMethod().Name + ": " + message);
    }

    /// <summary>
    /// Log warning message
    /// </summary>
    /// <param name="message"></param>
    public void Warning(string message)
    {
        StackTrace trace = new StackTrace();
        StackFrame frame = trace.GetFrame(1);
        string log = "WARNING - " + frame.GetMethod().ReflectedType.Name + " -> " + frame.GetMethod().Name + ": " + message;
        UnityEngine.Debug.Log(log);
        ConsoleManager.Instance.Run_Command("echo " + log);
    }

    /// <summary>
    /// Log error message
    /// </summary>
    /// <param name="message"></param>
    public void Error(string message)
    {
        StackTrace trace = new StackTrace();
        StackFrame frame = trace.GetFrame(1);
        string log = "ERROR - " + frame.GetMethod().ReflectedType.Name + " -> " + frame.GetMethod().Name + ": " + message;
        UnityEngine.Debug.Log(log);
        ConsoleManager.Instance.Run_Command("echo " + log);
    }
}
