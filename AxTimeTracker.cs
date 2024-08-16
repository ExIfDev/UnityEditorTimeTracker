//Written by Aexadev on 16/08/2024
//ver 1.0
using UnityEditor;
using UnityEngine;
using System;

[InitializeOnLoad]
public class ProjectTimeTracker
{
    private static readonly string TotalTimeKey = "ProjectTotalTime";
    private static readonly string LastTrackedDateKey = "ProjectLastTrackedDate";
    private static readonly string TodayTimeKey = "ProjectTodayTime";
    
    private static DateTime startTime;
    private static double totalTime;
    private static double todayTime;
    private static bool isEditorFocused = true;

    private static Vector2 displayPosition = new Vector2(10, 10);
    private static PositionPreset currentPreset = PositionPreset.BottomLeft;

    private static bool forceRedrawSceneView = false;
    private static bool showTotalTimeInSceneView = true;

    public enum PositionPreset
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        TopCenter,
        BottomCenter
    }

    static ProjectTimeTracker()
    {
        LoadTimeData();
        startTime = DateTime.Now;
        EditorApplication.update += UpdateTime;
        EditorApplication.quitting += OnEditorQuitting;
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        if (forceRedrawSceneView)
        {
            EditorApplication.update += ForceRedrawSceneView;
        }
    }

    [MenuItem("Tools/Time Tracker Options")]
    public static void ShowWindow()
    {
        TimeTrackerOptionsWindow window = (TimeTrackerOptionsWindow)EditorWindow.GetWindow(typeof(TimeTrackerOptionsWindow), true, "Time Tracker Options");
        window.Show();
    }

    private static void UpdateTime()
    {
        if (isEditorFocused)
        {
            double deltaTime = (DateTime.Now - startTime).TotalSeconds;
            totalTime += deltaTime;

            if (DateTime.Now.Date != DateTime.Parse(EditorPrefs.GetString(LastTrackedDateKey, DateTime.Now.ToString("yyyy-MM-dd"))).Date)
            {
                todayTime = 0; 
                EditorPrefs.SetString(LastTrackedDateKey, DateTime.Now.ToString("yyyy-MM-dd"));
            }

            todayTime += deltaTime;
            startTime = DateTime.Now;

            EditorPrefs.SetFloat(TotalTimeKey, (float)totalTime);
            EditorPrefs.SetFloat(TodayTimeKey, (float)todayTime);
        }
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!showTotalTimeInSceneView) return;

        Handles.BeginGUI();
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalTime);
        string timeText = $"Time Spent: {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 14;

        
        switch (currentPreset)
        {
            case PositionPreset.TopLeft:
                displayPosition = new Vector2(10, 10);
                break;
            case PositionPreset.TopRight:
                displayPosition = new Vector2(sceneView.position.width - 210, 10);
                break;
            case PositionPreset.BottomLeft:
                displayPosition = new Vector2(10, sceneView.position.height - 60);
                break;
            case PositionPreset.BottomRight:
                displayPosition = new Vector2(sceneView.position.width - 210, sceneView.position.height - 60);
                break;
            case PositionPreset.TopCenter:
                displayPosition = new Vector2((sceneView.position.width / 2) - 100, 10);
                break;
            case PositionPreset.BottomCenter:
                displayPosition = new Vector2((sceneView.position.width / 2) - 100, sceneView.position.height - 60);
                break;
        }

        GUILayout.BeginArea(new Rect(displayPosition.x, displayPosition.y, 200, 30));
        GUILayout.Label(timeText, style);
        GUILayout.EndArea();

        Handles.EndGUI();
    }

    private static void OnEditorQuitting()
    {
        EditorPrefs.SetFloat(TotalTimeKey, (float)totalTime);
        EditorPrefs.SetFloat(TodayTimeKey, (float)todayTime);
        EditorPrefs.SetInt("TimeTrackerPositionPreset", (int)currentPreset);
        EditorPrefs.SetString(LastTrackedDateKey, DateTime.Now.ToString("yyyy-MM-dd"));
    }

    private static void LoadTimeData()
    {
        if (EditorPrefs.HasKey(TotalTimeKey))
        {
            totalTime = EditorPrefs.GetFloat(TotalTimeKey);
        }
        else
        {
            totalTime = 0.0;
        }

        if (EditorPrefs.HasKey(TodayTimeKey))
        {
            todayTime = EditorPrefs.GetFloat(TodayTimeKey);
        }
        else
        {
            todayTime = 0.0;
        }

        if (EditorPrefs.HasKey("TimeTrackerPositionPreset"))
        {
            currentPreset = (PositionPreset)EditorPrefs.GetInt("TimeTrackerPositionPreset");
        }

        forceRedrawSceneView = EditorPrefs.GetBool("TimeTrackerForceRedrawSceneView", false);
        showTotalTimeInSceneView = EditorPrefs.GetBool("TimeTrackerShowTotalTimeInSceneView", true);
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
        {
            isEditorFocused = true;
            startTime = DateTime.Now;
        }
        else if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            isEditorFocused = false;
            UpdateTime(); 
        }
    }

    public static void ResetTimer()
    {
        totalTime = 0.0;
        todayTime = 0.0;
        EditorPrefs.SetFloat(TotalTimeKey, (float)totalTime);
        EditorPrefs.SetFloat(TodayTimeKey, (float)todayTime);
    }

    public static void ModifyTime(double seconds)
    {
        totalTime += seconds;
        todayTime += seconds;
        EditorPrefs.SetFloat(TotalTimeKey, (float)totalTime);
        EditorPrefs.SetFloat(TodayTimeKey, (float)todayTime);
    }

    public static void SetPositionPreset(PositionPreset preset)
    {
        currentPreset = preset;
        EditorPrefs.SetInt("TimeTrackerPositionPreset", (int)preset);
    }

    public static void SetForceRedrawSceneView(bool value)
    {
        forceRedrawSceneView = value;
        EditorPrefs.SetBool("TimeTrackerForceRedrawSceneView", value);

        if (forceRedrawSceneView)
        {
            EditorApplication.update += ForceRedrawSceneView;
        }
        else
        {
            EditorApplication.update -= ForceRedrawSceneView;
        }
    }

    public static void SetShowTotalTimeInSceneView(bool value)
    {
        showTotalTimeInSceneView = value;
        EditorPrefs.SetBool("TimeTrackerShowTotalTimeInSceneView", value);
    }

    private static void ForceRedrawSceneView()
    {
        SceneView.RepaintAll();
    }

    public static TimeSpan GetTodayTimeSpan()
    {
        return TimeSpan.FromSeconds(todayTime);
    }

    public static TimeSpan GetTotalTimeSpan()
    {
        return TimeSpan.FromSeconds(totalTime);
    }
}

public class TimeTrackerOptionsWindow : EditorWindow
{
    private double customTime = 0.0;
    private ProjectTimeTracker.PositionPreset selectedPreset;

    private bool forceRedrawSceneView;
    private bool showTotalTimeInSceneView;

    void OnEnable()
    {
        selectedPreset = ProjectTimeTracker.PositionPreset.BottomLeft;
        forceRedrawSceneView = EditorPrefs.GetBool("TimeTrackerForceRedrawSceneView", false);
        showTotalTimeInSceneView = EditorPrefs.GetBool("TimeTrackerShowTotalTimeInSceneView", true);
        EditorApplication.update += OnEditorUpdate; 
    }

    void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    void OnEditorUpdate()
    {
        Repaint(); 
    }

    void OnGUI()
    {
        GUILayout.Label("Time Tracker Options", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset Timer"))
        {
            ProjectTimeTracker.ResetTimer();
        }

        GUILayout.Space(10);

        GUILayout.Label("Add/Subtract Time (in seconds):");
        customTime = EditorGUILayout.DoubleField("Time:", customTime);

        if (GUILayout.Button("Modify Time"))
        {
            ProjectTimeTracker.ModifyTime(customTime);
        }

        GUILayout.Space(10);

        GUILayout.Label("Select Display Position:");
        selectedPreset = (ProjectTimeTracker.PositionPreset)EditorGUILayout.EnumPopup("Position Preset:", selectedPreset);

        if (GUILayout.Button("Set Position"))
        {
            ProjectTimeTracker.SetPositionPreset(selectedPreset);
        }

        GUILayout.Space(10);

        GUILayout.Label("Scene View Settings:", EditorStyles.boldLabel);
        forceRedrawSceneView = EditorGUILayout.Toggle("Force Redraw Scene View", forceRedrawSceneView);
        showTotalTimeInSceneView = EditorGUILayout.Toggle("Show Total Time in Scene View", showTotalTimeInSceneView);

        if (GUILayout.Button("Apply Settings"))
        {
            ProjectTimeTracker.SetForceRedrawSceneView(forceRedrawSceneView);
            ProjectTimeTracker.SetShowTotalTimeInSceneView(showTotalTimeInSceneView);
        }

        GUILayout.Space(20);

        GUILayout.Label("Today's Time Spent:", EditorStyles.boldLabel);
        TimeSpan todayTimeSpan = ProjectTimeTracker.GetTodayTimeSpan();
        GUILayout.Label($"Hours: {todayTimeSpan.Hours:D2} | Minutes: {todayTimeSpan.Minutes:D2} | Seconds: {todayTimeSpan.Seconds:D2}");

        GUILayout.Space(10);

        GUILayout.Label("Total Time Spent:", EditorStyles.boldLabel);
        TimeSpan totalTimeSpan = ProjectTimeTracker.GetTotalTimeSpan();
        GUILayout.Label($"Hours: {totalTimeSpan.Hours:D2} | Minutes: {totalTimeSpan.Minutes:D2} | Seconds: {totalTimeSpan.Seconds:D2}");

        GUILayout.FlexibleSpace();

        GUILayout.Label("© 2024 Aexadev ver 1.0", EditorStyles.centeredGreyMiniLabel);
    }
}
