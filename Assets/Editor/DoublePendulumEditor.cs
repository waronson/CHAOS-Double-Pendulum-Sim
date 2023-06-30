using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DoublePendulum))]
public class DoublePendulumEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Toggle Sim") && Application.isPlaying)
        {
            var pendulum = (target as DoublePendulum);

            if (!pendulum) return;
            
            if (pendulum.IsSimRunning)
                pendulum.StopSim();
            else
                pendulum.InitSim(false);
        }
    }
}
