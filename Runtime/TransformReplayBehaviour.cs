using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Animations;
#else
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;
#endif


namespace UTJ
{
    public class TransformReplayBehaviour : MonoBehaviour
    {
        private DumpFileParser fileParser;
        public List<Transform> targets;
        public int curentFrame;
        private bool isPlaying = false;

        public bool IsPlaying { get { return isPlaying; } }


        public int RecordFrame
        {
            get
            {
                if(fileParser == null) { return 0; }
                return fileParser.frameNum;
            }
        }

        public void PlayAnimation()
        {
            isPlaying = true;
        }
        public void PauseAnimation()
        {
            isPlaying = false;
        }

        public void LoadDumpFile(string file)
        {
            if (fileParser == null)
            {
                fileParser = new DumpFileParser();
            }
            fileParser.Read(file);
            var binds = fileParser.GetBinds();
            this.targets = TransformBinder.GetBindTransforms(this.transform, binds);            
        }
        private void LateUpdate()
        {
            if (isPlaying) {
                curentFrame++;
                if(curentFrame >= this.RecordFrame) { PauseAnimation(); }
            }
           ApplyFrame(curentFrame);
        }

        private void ApplyFrame(int idx)
        {
            if (fileParser == null || fileParser.frameNum == 0) { return; }
            if (idx < 0) { idx = 0; }
            idx %= fileParser.frameNum;

            int cnt = this.targets.Count;
            for (int i = 0; i < cnt; ++i) {
                Vector3 localPos;
                Quaternion localRot;
                Vector3 localScale;
                fileParser.GetData(idx, i, out localPos, out localRot, out localScale);
                targets[i].localPosition = localPos;
                targets[i].localRotation = localRot;
                targets[i].localScale = localScale;
            }
        }

        private void OnDestroy()
        {
            if (fileParser != null)
            {
                fileParser.Dispose();
            }
            fileParser = null;
        }
    }

}