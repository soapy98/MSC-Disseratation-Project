using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tanks
{
    [CreateAssetMenu(menuName = "Data/NameGenerate", order = 2)]
    public class NameGenerate : ScriptableObject
    {
        [SerializeField]
        public string[] Names;
    }
}