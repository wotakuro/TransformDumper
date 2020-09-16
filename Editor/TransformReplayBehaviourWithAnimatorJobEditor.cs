using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UTJ
{

    [CustomEditor(typeof(TransformReplayWithAnimatorJobBehaviour))]
    public class TransformReplayBehaviourWithAnimatorJobEditor : Editor
    {
        private string fileName = "";
        private TransformReplayWithAnimatorJobBehaviour previeBehaviour = null;

        public override void OnInspectorGUI()
        {
            TransformReplayWithAnimatorJobBehaviour replayBehaviour = target as TransformReplayWithAnimatorJobBehaviour;
            if (previeBehaviour != replayBehaviour)
            {
                fileName = replayBehaviour.gameObject.name + ".utd";
            }
            previeBehaviour = replayBehaviour;

            fileName = EditorGUILayout.TextField("ファイル名", fileName);
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("プレイ中でないと再生出来ません");
                return;
            }
            if (GUILayout.Button("Load"))
            {
                replayBehaviour.LoadDumpFile(fileName);
            }
            if (replayBehaviour.RecordFrame > 0)
            {
                replayBehaviour.curentFrame = EditorGUILayout.IntSlider("フレーム",
                    replayBehaviour.curentFrame, 0, replayBehaviour.RecordFrame-1);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("<-", GUILayout.Width(30)))
                {
                    replayBehaviour.curentFrame -= 1;
                }
                GUILayout.FlexibleSpace();
                GUILayout.Label("" + replayBehaviour.RecordFrame + "フレーム");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("->", GUILayout.Width(30)))
                {
                    replayBehaviour.curentFrame += 1;
                }
                EditorGUILayout.EndHorizontal();
                if (!replayBehaviour.IsPlaying)
                {
                    if (GUILayout.Button("再生", GUILayout.Width(80)))
                    {
                        replayBehaviour.PlayAnimation();
                    }
                }
                else
                {
                    if (GUILayout.Button("停止", GUILayout.Width(80)))
                    {
                        replayBehaviour.PauseAnimation();
                    }
                }
            }
        }
    }
}
