using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using McpUnity.Utils;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for starting a timer in the Unity Editor with a specified duration and unit.
    /// </summary>
    public class TimerTool : McpToolBase
    {
        private static double timerEndTime;
        private static bool timerRunning;

        public TimerTool()
        {
            Name = "timer_tool";
            Description = "Starts a timer for a specified duration and unit (sec, min, hrs)";
        }

        public override JObject Execute(JObject parameters)
        {
            float duration = parameters["duration"]?.ToObject<float>() ?? 0f;
            string unit = parameters["unit"]?.ToObject<string>()?.ToLower();

            if (duration <= 0 || string.IsNullOrEmpty(unit))
            {
                return McpUnitySocketHandler.CreateErrorResponse(
                    "Parameters 'duration' and 'unit' (sec, min, hrs) must be provided.",
                    "validation_error"
                );
            }

            // Convert to seconds
            float durationSeconds = unit switch
            {
                "sec" => duration,
                "min" => duration * 60f,
                "hrs" => duration * 3600f,
                _ => 0f
            };

            if (durationSeconds <= 0)
            {
                return McpUnitySocketHandler.CreateErrorResponse(
                    "Unit must be one of: sec, min, hrs.",
                    "validation_error"
                );
            }

            timerEndTime = EditorApplication.timeSinceStartup + durationSeconds;
            timerRunning = true;
            EditorApplication.update -= TimerUpdate;
            EditorApplication.update += TimerUpdate;

            McpLogger.LogInfo($"[MCP Unity] Timer started for {duration} {unit} ({durationSeconds} seconds).");

            return new JObject
            {
                ["success"] = true,
                ["type"] = "text",
                ["message"] = $"Timer started for {duration} {unit}."
            };
        }

        private static void TimerUpdate()
        {
            if (!timerRunning) return;

            double timeLeft = timerEndTime - EditorApplication.timeSinceStartup;
            if (timeLeft <= 0)
            {
                timerRunning = false;
                EditorApplication.update -= TimerUpdate;
                McpLogger.LogInfo("[MCP Unity] Timer finished!");
                EditorUtility.DisplayDialog("Timer", "Timer is done!", "OK");
            }
        }
    }
}