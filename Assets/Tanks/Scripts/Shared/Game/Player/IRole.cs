using MLAPI.NetworkVariable;
using UnityEngine;
namespace Tanks.Shared
{
    public abstract class Role : MonoBehaviour
    {
        protected Role() { }
        public abstract void SetParent();
        public virtual void Start()
        {
        }
        
        public virtual void Update()
        {
        }
    }
}