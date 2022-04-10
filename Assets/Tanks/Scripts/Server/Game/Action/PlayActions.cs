using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//ActionPlayer
namespace Tanks.Server
{
    public class PlayActions
    {
        private ServerTank m_Parent;
        private List<Action> m_ActionQueue;
        private List<Action> m_NonBlockActions;
        private Dictionary<ActionOption, float> m_LastActionTime;
        private const float m_MaxQueueTime = 1.8f;
        private ActionData m_PendingAction;
        private bool m_AnyPendingActions;


        public PlayActions(ServerTank tank)
        {
            m_Parent = tank;
            m_ActionQueue = new List<Action>();
            m_NonBlockActions = new List<Action>();
            m_LastActionTime = new Dictionary<ActionOption, float>();
        }

        public void Play(ref ActionData actionRequest)
        {
            if (!actionRequest.AddToQueue && m_ActionQueue.Count > 0 && m_ActionQueue[0].ActionInfo.Interuptable)
                ClearActions(false);
            if (TotalQTime() >= m_MaxQueueTime)
                return;
            var newaction = Action.EnableAction(m_Parent, ref actionRequest);
            m_ActionQueue.Add(newaction);
            if (m_ActionQueue.Count == 1)
            {
                StartAction();
            }
        }

        public void ClearActions(bool cancel)
        {
            if (m_ActionQueue.Count > 0)
            {
                m_LastActionTime.Remove(m_ActionQueue[0].ActionInfo.ActionOption);
                m_ActionQueue[0].Cancel();
            }

            m_ActionQueue.Clear();
            if (!cancel) return;
            foreach (var blockAction in m_NonBlockActions)
            {
                blockAction.Cancel();
            }

            m_NonBlockActions.Clear();
        }

        public bool GetActiveActionData(out ActionData actionData)
        {
            if (m_ActionQueue.Count > 0)
            {
                actionData = m_ActionQueue[0].Data;
                return true;
            }

            actionData = new ActionData();
            return false;
        }

        public bool IsReuseTimeElapsed(ActionOption actionOption)
        {
            if (!m_LastActionTime.TryGetValue(actionOption, out var lastTimeUsed)) return true;
            if (!GameData.Instance.ActionsByDataType.TryGetValue(actionOption, out var description))
                return true;
            var reuseTime = description.ReuseTime;
            return !(reuseTime > 0) || !(Time.time - lastTimeUsed < reuseTime);
        }

        public int RunningActionCount => m_NonBlockActions.Count + (m_ActionQueue.Count > 0 ? 1 : 0);


        private void StartAction()
        {
            if (m_ActionQueue.Count <= 0) return;
            var reuseTime = m_ActionQueue[0].ActionInfo.ReuseTime;
            if (reuseTime > 0 && m_LastActionTime.TryGetValue(m_ActionQueue[0].ActionInfo.ActionOption, out var lastTimeUsed)
                              && Time.time - lastTimeUsed < reuseTime)
            {
                AdvanceQueue(false);
                return;
            }
 
            m_ActionQueue[0].StartTime = Time.time;
            var play = m_ActionQueue[0].Start();
            if (!play)
            {
                AdvanceQueue(false);
                return;
            }
            m_LastActionTime[m_ActionQueue[0].ActionInfo.ActionOption] = Time.time;

            if (!(Math.Abs(m_ActionQueue[0].ActionInfo.ExecutionTime) < 0.02f) ||
                m_ActionQueue[0].ActionInfo.BlockMode != ActionInfo.BlockingMode.OnlyExecTime) return;
            m_NonBlockActions.Add(m_ActionQueue[0]);
            AdvanceQueue(false);
        }

     

        private void AdvanceQueue(bool endRemoved)
        {
            if (m_ActionQueue.Count > 0)
            {
                if (endRemoved)
                {
                    m_ActionQueue[0].End();
                    if (m_ActionQueue[0].ActionChain(ref m_PendingAction))
                    {
                        m_AnyPendingActions = true;
                    }
                }

                m_ActionQueue.RemoveAt(0);
            }
            if (!m_AnyPendingActions || m_PendingAction.AddToQueue)
            {
                StartAction();
            }
        }

        public void Update()
        {
            if (m_AnyPendingActions)
            {
                m_AnyPendingActions = false;
                Play(ref m_PendingAction);
            }

            if (m_ActionQueue.Count > 0 && m_ActionQueue[0].NonBlockingAction())
            {
                m_NonBlockActions.Add(m_ActionQueue[0]);
                AdvanceQueue(false);
            }

            if (m_ActionQueue.Count > 0)
            {
                if (!UpdateAction(m_ActionQueue[0]))
                {
                    AdvanceQueue(true);
                }
            }

            for (var i = m_NonBlockActions.Count - 1; i >= 0; --i)
            {
                var runningAction = m_NonBlockActions[i];
                if (UpdateAction(runningAction)) continue;
                runningAction.End();
                m_NonBlockActions.RemoveAt(i);
            }
        }

        private static bool UpdateAction(Action action)
        {
            var keepGoing = action.Update();
            var expirable = action.ActionInfo.Duration > 0f;
            var timeElapsed = Time.time - action.StartTime;
            var timeExpired = expirable && timeElapsed >= action.ActionInfo.Duration;
            return keepGoing && !timeExpired;
        }

        private float TotalQTime()
        {
            if (m_ActionQueue.Count == 0)
                return 0;
            var totalTime = m_ActionQueue.Select(action => action.ActionInfo).Select(info =>
                info.BlockMode switch
                {
                    ActionInfo.BlockingMode.OnlyExecTime => info.ExecutionTime,
                    ActionInfo.BlockingMode.EntireAction => info.Duration,
                    _ => throw new System.Exception($"Unrecognized blocking mode: {info.BlockMode}")
                }).Sum();
            return totalTime - m_ActionQueue[0].RunTime;
        }

        public void OnCollisionEnter(Collision other)
        {
            if (m_ActionQueue.Count > 0) m_ActionQueue[0].OnCollisionEnter(other);
        }


        public void OnGameplayActivity(Action.GameplayActivity activityThatOccurred)
        {
            if (m_ActionQueue.Count > 0)
                m_ActionQueue[0].PlayActivity(activityThatOccurred);
            foreach (var action in m_NonBlockActions) 
                action.PlayActivity(activityThatOccurred);
        }

    }
}