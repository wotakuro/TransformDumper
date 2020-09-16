using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UTJ
{
    public class TransformBinder : MonoBehaviour
    {

        public static List<Transform> GetBindTransforms(Transform root, List<string> names)
        {
            List<Transform> binds = new List<Transform>(names.Count);

            foreach (string name in names)
            {
                var trans = root.Find(name);
                binds.Add(trans);
            }
            return binds;
        }
    }
}