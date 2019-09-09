#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.VR.Hologram.Editor
{
    [CustomEditor(typeof(PVR_Hologram_Spawner))]
    public class PVR_Hologram_Spawner_Editor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PVR_Hologram_Spawner myScript = (PVR_Hologram_Spawner)target;
            if (GUILayout.Button("Spawn Object"))
            {
                myScript.SpawnObject();
            }
            else if (GUILayout.Button("Destroy Object"))
            {
                myScript.DestroyObject();
            }
        }
    }
} 
#endif
