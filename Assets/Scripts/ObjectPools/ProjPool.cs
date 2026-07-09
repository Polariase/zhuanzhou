using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace MyPool
{
    public class ProjPool : MyObjectPool
    {
        public GameObject GetAndSetProj(string key, Vector3 pos, Quaternion rot)
        {
            GameObject obj = Get(key);
            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(pos, rot);
            }
            return obj;
        }

        public GameObject GetAndSetEffect(string key, Vector3 pos, Quaternion rot)
        {
            GameObject obj = Get(key);
            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(pos, rot);
            }
            return obj;
        }
    }
}

