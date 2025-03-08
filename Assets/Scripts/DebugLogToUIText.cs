using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class DebugLogToUIText : MonoBehaviour
{
    public Text logText;  // Assign this via the Inspector.
    public int maxLines = 50;  // Maximum lines to display.

    private StringBuilder logBuilder = new StringBuilder();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        logBuilder.AppendLine(logString);
        // Limit the total number of lines to avoid performance issues.
        string[] lines = logBuilder.ToString().Split('\n');
        if (lines.Length > maxLines)
        {
            logBuilder.Clear();
            for (int i = lines.Length - maxLines; i < lines.Length; i++)
            {
                logBuilder.AppendLine(lines[i]);
            }
        }
        logText.text = logBuilder.ToString();
    }
}
