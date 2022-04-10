using System;
using UnityEngine;

namespace Tanks
{
    //Information about action which can be sent to the server
    [CreateAssetMenu(menuName = "Data/ActionInfo", order = 1)]
    public class ActionInfo : ScriptableObject
    {
        public ActionOption ActionOption;
        public ActionLogic ActionLogic;
        public int Total;
        public int Duration;
        public float ExecutionTime;
        public float ReuseTime;
        public bool Interuptable;
        [Serializable]
        public enum BlockingMode
        {
            EntireAction,
            OnlyExecTime
        }

        public BlockingMode BlockMode;
        
        [Serializable]
        public struct BulletInfo
        {
            public GameObject BulletObj;
            public float Speed;
            public int Damage;
            public float Distance;
        }
        public BulletInfo[] Bullets;
        public bool Friendly;
    }
}