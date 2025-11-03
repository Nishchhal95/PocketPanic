using UnityEngine;

public static class Logger
{
    private static bool needLogs = true;
    
    private static Color LOG_COLOR = Color.green;
    private static Color ERROR_COLOR = Color.red;
    private static Color WARNING_COLOR = Color.yellow;

    private static string LOG_FORMAT = "<color=#{0}>{1}</color>";

    public static void Log(object message)
    {
        InternalLog(LogType.Log, message);
    }
    
    public static void Warning(object message)
    {
        InternalLog(LogType.Warning, message);
    }
    
    public static void Error(object message)
    {
        InternalLog(LogType.Error, message);
    }

    private static void InternalLog(LogType logType, object message)
    {
        if (!needLogs && logType != LogType.Error)
        {
            return;
        }
        
        Color color = LOG_COLOR;

        switch (logType)
        {
            case LogType.Error:
                color = ERROR_COLOR;
                break;
            case LogType.Warning:
                color = WARNING_COLOR;
                break;
            case LogType.Log:
                color = LOG_COLOR;
                break;
        }

        string hexCodeColor = ColorUtility.ToHtmlStringRGB(color);
        Debug.Log(string.Format(LOG_FORMAT, hexCodeColor, message));
    }
}
