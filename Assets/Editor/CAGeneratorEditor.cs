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
    }
}

[CustomEditor(typeof(CellularAutomataGenerator))]
public class CAEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CellularAutomataGenerator script = (CellularAutomataGenerator)target;
        if (GUILayout.Button("Analyze")) {
            script.Analyze();
        }
    }
}