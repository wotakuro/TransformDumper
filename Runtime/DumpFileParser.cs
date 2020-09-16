using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UTJ
{
    /// <summary>
    /// TransformをDumpしたファイルをパースします
    /// </summary>
    public class DumpFileParser : System.IDisposable
    {
        private List<string> binds;
        private NativeArray<Vector3> localPositions;
        private NativeArray<Quaternion> localRotation;
        private NativeArray<Vector3> localScales;
        private int bindSize;
        private int frameSize;

        private byte[] buffer = new byte[256];
        private System.IO.FileStream readFs;
        private System.Text.Encoding utf8encoding;

        public List<string> GetBinds()
        {
            return binds;
        }

        public int frameNum
        {
            get { return this.frameSize; }
        }


        public void Read(string file)
        {
            readFs = System.IO.File.OpenRead(file);
            this.Dispose();

            ReadBinds();
            ReadBody();
            readFs.Close();
            readFs = null;
        }


        public void GetData(int frame, int bindIdx,
            out Vector3 localPos, out Quaternion localRot, out Vector3 localScale)
        {
            int idx = frame * bindSize + bindIdx;
            localPos = this.localPositions[idx];
            localRot = this.localRotation[idx];
            localScale = this.localScales[idx];
        }

        private void ReadBinds()
        {
            this.bindSize = ReadInt();
            this.binds = new List<string>(this.bindSize);
            for (int i = 0; i < bindSize; ++i)
            {
                string str = ReadString(); ;
                binds.Add(str);
            }
        }
        private void ReadBody()
        {
            int arraySize = GetEstimateSize(readFs);
            this.frameSize = arraySize / bindSize;

            localPositions = new NativeArray<Vector3>(arraySize, Allocator.Persistent);
            localRotation = new NativeArray<Quaternion>(arraySize, Allocator.Persistent);
            localScales = new NativeArray<Vector3>(arraySize, Allocator.Persistent);

            for (int i = 0; i < this.frameSize; ++i)
            {
                for (int j = 0; j < this.bindSize; ++j)
                {
                    int idx = (i * this.bindSize) + j;
                    var position = new Vector3();
                    position.x = ReadFloat();
                    position.y = ReadFloat();
                    position.z = ReadFloat();
                    ReadFloat();
                    localPositions[idx] = position;
                }
                for (int j = 0; j < this.bindSize; ++j)
                {

                    int idx = (i * this.bindSize) + j;
                    var rotate = new Quaternion();
                    rotate.x = ReadFloat();
                    rotate.y = ReadFloat();
                    rotate.z = ReadFloat();
                    rotate.w = ReadFloat();
                    localRotation[idx] = rotate;

                }
                for (int j = 0; j < this.bindSize; ++j)
                {
                    int idx = (i * this.bindSize) + j;
                    var size = new Vector3();
                    size.x = ReadFloat();
                    size.y = ReadFloat();
                    size.z = ReadFloat();
                    ReadFloat();
                    // apply
                    localScales[idx] = size;
                }

            }
        }

        private unsafe int ReadInt()
        {
            readFs.Read(buffer, 0, 4);
            fixed (void* ptr = &buffer[0])
            {
                return *((int*)ptr);
            }
        }
        private unsafe float ReadFloat()
        {
            readFs.Read(buffer, 0, 4);
            fixed (void* ptr = &buffer[0])
            {
                return *((float*)ptr);
            }
        }
        private string ReadString()
        {
            if (utf8encoding == null)
            {
                utf8encoding = System.Text.Encoding.GetEncoding("utf-8");
            }
            int strSize = this.ReadInt();
            readFs.Read(buffer, 0, strSize);
            var str = utf8encoding.GetString(buffer, 0, strSize);
            return str;
        }


        private int GetEstimateSize(System.IO.Stream stream)
        {
            var left = stream.Length - stream.Position;
            int arraySize = (int)(left / (16+16+16) );
            return arraySize;
        }


        public void Dispose()
        {
            if (localPositions.IsCreated)
            {
                localPositions.Dispose();
            }
            if (localRotation.IsCreated)
            {
                localRotation.Dispose();
            }
            if (localScales.IsCreated)
            {
                localScales.Dispose();
            }
        }
    }

}