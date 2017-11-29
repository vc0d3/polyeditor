using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public static class ExtensionMethods
    {
        public static Vector3 ToXZ(this Vector3 v3)
        {
            return new Vector3(v3.x, v3.z);
        }
    }

