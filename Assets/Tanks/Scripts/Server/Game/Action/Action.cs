using System;
using UnityEngine;

namespace Tanks.Server
{
    //Abstract class which actions inherit from and enable an action to occur on the server
    public abstract class Action
    {
        protected ServerTank m_parent;
        protected ActionData m_RequestData;
        
        public float StartTime { get; set; }
        
        public float RunTime => Time.time - StartTime;

        public ref ActionData Data => ref m_RequestData;

        public ActionInfo ActionInfo
        {
            get
            {
                GameData.Instance.ActionsByDataType.TryGetValue(Data.WhichAction, out _);
                return GameData.Instance.ActionsByDataType[Data.WhichAction];
            }
        }

        protected Action(ServerTank parent, ref ActionData data)
        {
            m_parent = parent;
            m_RequestData = data;
        }

        public abstract bool Start();
        public abstract bool Update();

        public bool NonBlockingAction()
        {
            return ActionInfo.BlockMode == ActionInfo.BlockingMode.EntireAction &&
                   RunTime >= ActionInfo.ExecutionTime;
        }

        public virtual void End()
        {
            Cancel();
        }

        public void Cancel(){ }

        public bool ActionChain(ref ActionData newAction) { return false; }

        public virtual void OnCollisionEnter(Collision other) { }
        public enum GameplayActivity
        {
            EnemyAttack,
            Heal,
            StopAction,
            UseAttackAction,
            EnergyAction
        }
        public virtual void PlayActivity(GameplayActivity activity) { }

        public static Action EnableAction(ServerTank parent, ref ActionData data)
        {
            if (!GameData.Instance.ActionsByDataType.TryGetValue(data.WhichAction, out var actionDescription))
            {
                throw new SystemException($"Action{data.WhichAction} not exist");
            }

            var logic = actionDescription.ActionLogic;
            return logic switch
            {
                ActionLogic.Shoot => new ShootAction(parent, ref data),
                ActionLogic.TurnLeft => new TurnAction(parent, ref data),
                ActionLogic.TurnRight => new TurnAction(parent, ref data),
                ActionLogic.SpeedUp => new SpeedAction(parent, ref data),
                ActionLogic.Stop => new SpeedAction(parent, ref data),
                ActionLogic.Enable => new EnergyHealthAction(parent, ref data),
                ActionLogic.Disable => new EnergyHealthAction(parent, ref data),
                _ => throw new System.NotImplementedException()
            };
        }
    }
}