using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CAMapGenerator))]
public class CAGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CAMapGenerator script = (CAMapGenerator)target;
        if (GUILayout.Button("Build Map")) {
            script.BuildMap();
        }

        if (GUILayout.Button("Build Map + Analyze")) {
            script.BuildAndAnalyze();
        }
    }
}

[CustomEditor(typeof(MarioOneOne))]
public class MarioOneOneEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        MarioOneOne script = (MarioOneOne)target;

        if (GUILayout.Button("Analyze")) {
            script.Analyze();
        }
    }
}