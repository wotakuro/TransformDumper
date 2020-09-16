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
    public class TransformReplayWithAnimatorJobBehaviour : MonoBehaviour
    {
        private DumpFileParser fileParser;
        public List<Transform> targets;

        public int curentFrame;

        private AnimationTransformApplyJob transformApplyJob;
        private AnimationScriptPlayable scriptPlayable;
        private bool isPlaying = false;

        public bool IsPlaying { get { return isPlaying; } }

        public int RecordFrame
        {
            get
            {
                if (fileParser == null) { return 0; }
                return fileParser.frameNum;
            }
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

            SetupAnimationJob();
        }


        public void PlayAnimation()
        {
            isPlaying = true;
        }
        public void PauseAnimation()
        {
            isPlaying = false;
        }
        private void Update()
        {
            if (isPlaying) {
                curentFrame++;
                if (curentFrame >= this.RecordFrame) { PauseAnimation(); }
            }
            ApplyFrameToJob(curentFrame);
        }


        void SetupAnimationJob()
        {
            int cnt = this.targets.Count;
            var animator = GetComponent<Animator>();
            var graph = PlayableGraph.Create("Bind");

            var output = AnimationPlayableOutput.Create(graph, "ouput", GetComponent<Animator>());
            transformApplyJob = new AnimationTransformApplyJob();
            transformApplyJob.Initialize(animator, this.targets);


            scriptPlayable = AnimationScriptPlayable.Create(graph, transformApplyJob);
            output.SetSourcePlayable(scriptPlayable);
            graph.Play();
        }

        private void ApplyFrameToJob(int idx)
        {
            if (fileParser == null || fileParser.frameNum == 0) { return; }
            if (idx < 0) { idx = 0; }
            idx %= fileParser.frameNum;

            int cnt = transformApplyJob.positions.Length;
            for (int i = 0; i < cnt; ++i)
            {
                Vector3 localPos;
                Quaternion localRot;
                Vector3 localScale;
                fileParser.GetData(idx, i, out localPos, out localRot, out localScale);
                transformApplyJob.positions[i] = localPos;
                transformApplyJob.rotations[i] = localRot;
                transformApplyJob.scales[i] = localScale;
            }
        }

        private void OnDestroy()
        {
            if (fileParser != null)
            {
                fileParser.Dispose();
            }
            fileParser = null;
            transformApplyJob.Dispose();
        }
    }

    public class AnimationTransformApplyPreparePlayable : PlayableBehaviour
    {
        private DumpFileParser fileParser;
        private int index;
        private AnimationTransformApplyJob transformApplyJob;

        public void SetDumpFileParser(DumpFileParser parser)
        {
            this.fileParser = parser;
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            base.PrepareFrame(playable, info);
            var input = playable.GetInput(0);
            ApplyFrameToJob(index);
            ++index;
        }

        private void ApplyFrameToJob(int idx)
        {
            if (fileParser == null || fileParser.frameNum == 0) { return; }
            if (idx < 0) { idx = 0; }
            idx %= fileParser.frameNum;

            int cnt = transformApplyJob.positions.Length;
            for (int i = 0; i < cnt; ++i)
            {
                Vector3 localPos;
                Quaternion localRot;
                Vector3 localScale;
                fileParser.GetData(idx, i, out localPos, out localRot, out localScale);
                transformApplyJob.positions[i] = localPos;
                transformApplyJob.rotations[i] = localRot;
                transformApplyJob.scales[i] = localScale;
            }
        }

    }


    public struct AnimationTransformApplyJob : IAnimationJob, System.IDisposable
    {
        public NativeArray<TransformStreamHandle> transStreamHanles;
        [ReadOnly]
        public NativeArray<Vector3> positions;
        [ReadOnly]
        public NativeArray<Quaternion> rotations;
        [ReadOnly]
        public NativeArray<Vector3> scales;

        public void Initialize(Animator animator,List<Transform> targets)
        {
            int cnt = targets.Count;
            this.transStreamHanles = new NativeArray<TransformStreamHandle>(cnt, Allocator.Persistent);
            this.positions = new NativeArray<Vector3>(cnt, Allocator.Persistent);
            this.rotations = new NativeArray<Quaternion>(cnt, Allocator.Persistent);
            this.scales = new NativeArray<Vector3>(cnt, Allocator.Persistent);

            for (int i = 0; i < cnt; ++i)
            {
                this.transStreamHanles[i] = animator.BindStreamTransform(targets[i]);
            }
        }

        public void ProcessRootMotion(AnimationStream stream)
        {
        }

        public void ProcessAnimation(AnimationStream stream)
        {
            for(int i=0;i< transStreamHanles.Length; ++i)
            {
                transStreamHanles[i].SetLocalPosition(stream, positions[i]);
                transStreamHanles[i].SetLocalRotation(stream, rotations[i]);
                transStreamHanles[i].SetLocalScale(stream, scales[i]);
            }
        }

        public void Dispose()
        {
            if (transStreamHanles.IsCreated)
            {
                transStreamHanles.Dispose();
            }
            if (positions.IsCreated)
            {
                positions.Dispose();
            }
            if (rotations.IsCreated)
            {
                rotations.Dispose();
            }
            if (scales.IsCreated)
            {
                scales.Dispose();
            }
        }
    }
}