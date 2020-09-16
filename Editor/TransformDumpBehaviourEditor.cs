using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UTJ
{
    [CustomEditor(typeof(TransformDumpBehaviour))]
    public class TransformDumpBehaviourEditor : Editor
    {
        public string fileName = "";
        private TransformDumpBehaviour previeBehaviour =null;

        public override void OnInspectorGUI()
        {
            TransformDumpBehaviour transformDumpBehaviour = target as TransformDumpBehaviour;
            if(previeBehaviour != transformDumpBehaviour)
            {
                fileName = transformDumpBehaviour.gameObject.name + ".utd";
            }
            previeBehaviour = transformDumpBehaviour;

            fileName = EditorGUILayout.TextField("ファイル名", fileName);
            if ( !EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("再生中にならないとRecord出来ません");
                return;
            }
            if(!transformDumpBehaviour.IsRecording)
            {
                if (GUILayout.Button("Record") ){
                    transformDumpBehaviour.StartRecord(fileName);
                }
            }
            else
            {
                if (GUILayout.Button("Stop"))
                {
                    transformDumpBehaviour.EndRecording();
                }
            }
        }
    }
}