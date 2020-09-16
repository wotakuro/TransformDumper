using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

namespace UTJ
{

    public unsafe class TransformDumpBehaviour : MonoBehaviour
    {
        public int targetFps = 60;
        private List<Transform> targetTransforms;
        private List<string> targetTransPath;

        private StringBuilder stringBuilderBuffer;
        private List<Transform> pathTransformBuffer;

        private byte[] buffer = new byte[512];
        private int bufferIdx = 0;

        private Stream writeFs;
        private Encoding utf8encoding;

        public bool IsRecording
        {
            get
            {
                return this.writeFs != null;
            }
        }


        private void OnDestroy()
        {
            EndRecording();
        }

        public void StartRecord(string path)
        {
            this.InitChildList();
            Application.targetFrameRate = targetFps;
            Time.captureFramerate = targetFps;

            this.writeFs = File.OpenWrite(path);
            WriteInt(targetTransPath.Count);
            foreach (var str in targetTransPath)
            {
                WriteString(str);
            }
        }

        public void EndRecording()
        {
            this.FlushBuffer();
            if (writeFs != null)
            {
                this.writeFs.Close();
            }
            writeFs = null;
        }

        private void LateUpdate()
        {
            if (writeFs != null)
            {
                DumpCurrentSnap();
            }
        }

        private void DumpCurrentSnap()
        {

            foreach (var current in this.targetTransforms)
            {
                var pos = current.localPosition;
                WriteFloat(pos.x);
                WriteFloat(pos.y);
                WriteFloat(pos.z);
                WriteFloat(0.0f);//<-padding
            }
            foreach (var current in this.targetTransforms)
            {
                var rot = current.localRotation;
                WriteFloat(rot.x);
                WriteFloat(rot.y);
                WriteFloat(rot.z);
                WriteFloat(rot.w);
            }
            foreach (var current in this.targetTransforms)
            {
                var scale = current.localScale;
                WriteFloat(scale.x);
                WriteFloat(scale.y);
                WriteFloat(scale.z);
                WriteFloat(0.0f); //<- padding
            }
        }

        private void FlushBuffer()
        {
            if( bufferIdx == 0) { return; }
            writeFs.Write(this.buffer, 0, bufferIdx);
            bufferIdx = 0;
        }


        private void WriteFloat(float val)
        {

            if (this.bufferIdx + 4 >= buffer.Length)
            {
                this.FlushBuffer();
            }
            byte* ptr = (byte*)&val;
            for (int i = 0; i < 4; ++i)
            {
                buffer[bufferIdx] = *ptr;
                ++bufferIdx;
                ++ptr;
            }
        }
        private void WriteInt(int val)
        {
            if (this.bufferIdx + 4 >= buffer.Length)
            {
                this.FlushBuffer();
            }
            byte* ptr = (byte*)&val;
            for (int i = 0; i < 4; ++i)
            {
                buffer[bufferIdx] = *ptr;
                ++bufferIdx;
                ++ptr;
            }
        }
        private void WriteString(string str)
        {
            if (utf8encoding == null)
            {
                utf8encoding = Encoding.GetEncoding("utf-8");
            }
            var bytes = utf8encoding.GetBytes(str);
            WriteInt(bytes.Length);
            this.FlushBuffer();
            writeFs.Write(bytes, 0, bytes.Length);
        }



        #region INIT_TARGET_LIST
        private void InitChildList()
        {
            if (targetTransforms == null)
            {
                targetTransforms = new List<Transform>();
            }
            else
            {
                targetTransforms.Clear();
            }
            CollectTransform(this.transform);
            InitTargetPath();
        }
        private void InitTargetPath()
        {
            int transformCount = targetTransforms.Count;
            if (targetTransPath == null)
            {
                targetTransPath = new List<string>(transformCount);
            }
            else
            {
                targetTransPath.Clear();
            }
            for (int i = 0; i < transformCount; ++i)
            {
                string path = GetPath(targetTransforms[i], this.transform);
                targetTransPath.Add(path);
            }
        }
        private string GetPath(Transform target, Transform stopAt)
        {
            if (stringBuilderBuffer == null)
            {
                stringBuilderBuffer = new StringBuilder(256);
            }
            else
            {
                stringBuilderBuffer.Length = 0;
            }
            if (pathTransformBuffer == null)
            {
                pathTransformBuffer = new List<Transform>(16);
            }
            else
            {
                pathTransformBuffer.Clear();
            }

            for (var current = target; current != null; current = current.parent)
            {
                if (current == stopAt) {
                    break;
                }
                pathTransformBuffer.Add(current);
            }
            pathTransformBuffer.Reverse();

            int count = pathTransformBuffer.Count;

            for (int i = 0; i < count; ++i)
            {
                stringBuilderBuffer.Append(pathTransformBuffer[i].name);
                if (i < count - 1)
                {
                    stringBuilderBuffer.Append('/');
                }
            }
            return stringBuilderBuffer.ToString();
        }
        private void CollectTransform(Transform trans)
        {
            targetTransforms.Add(trans);
            int childCount = trans.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                CollectTransform(trans.GetChild(i));
            }
        }
        #endregion INIT_TARGET_LIST
    }
}