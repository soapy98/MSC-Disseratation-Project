using System.Collections;
using System.Collections.Generic;
using Tanks;
using UnityEngine;

namespace Tanks
{
    public class TransformRegister : MonoBehaviour
    {
        [SerializeField] TransformVariable m_TransformVariable;
        void OnEnable()
        {
            m_TransformVariable.Value = transform;
        }
        void OnDisable()
        {
            m_TransformVariable.Value = null;
        }
    }
}