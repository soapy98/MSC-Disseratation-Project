using MLAPI;
using Tanks.Shared;
using UnityEngine;

namespace Tanks.Server
{
    public class ServerTank : NetworkBehaviour, IDamage
    {
        public NetworkTankState NetworkTankState { get; set; }

        public bool IsTankReady;
        [SerializeField] ServerTankMovement m_move;
        private PlayActions m_Actions;
        private ActionOption m_IntialAction = ActionOption.None;

        public PlayActions ActiveActions => m_Actions;


        private void Awake()
        {
            m_move = GetComponent<ServerTankMovement>() != null
                ? GetComponent<ServerTankMovement>()
                : gameObject.AddComponent<ServerTankMovement>();
            NetworkTankState = GetComponent<NetworkTankState>() != null
                ? GetComponent<NetworkTankState>()
                : gameObject.AddComponent<NetworkTankState>();
            m_Actions = new PlayActions(this);
        }

        public override void NetworkStart()
        {
            if (!IsServer) enabled = false;
            else
            {
                NetworkTankState.TankActionEventServer += ActionRequested;
                NetworkTankState.NetworkTankLifeState.TankLifeState.OnValueChanged += OnLifeStateChange;
                if (m_IntialAction == ActionOption.None) return;
                var startAction = new ActionData() {WhichAction = m_IntialAction};
                PlayAction(ref startAction);
            }
        }

        private void OnDestroy()
        {
            if (!NetworkTankState) return;
            NetworkTankState.TankActionEventServer -= ActionRequested;
            NetworkTankState.NetworkTankLifeState.TankLifeState.OnValueChanged -= OnLifeStateChange;
        }

        private void PlayAction(ref ActionData action)
        {
            if (NetworkTankState.LifeState != TankLifeState.Alive) return;
            if (action.Cancel) m_move.CancelMovement();

            m_Actions.Play(ref action);
        }

        private void OnLifeStateChange(TankLifeState previousState, TankLifeState currentstate)
        {
            if (currentstate == TankLifeState.Alive) return;
            m_Actions.ClearActions(true);
            m_move.CancelMovement();
        }

        private void ActionRequested(ActionData data)
        {
            if (!GameData.Instance.ActionsByDataType[data.WhichAction].Friendly)
                m_Actions.OnGameplayActivity(Action.GameplayActivity.UseAttackAction);
            PlayAction(ref data);
        }
        private void Update()
        {
            m_Actions.Update();
        }

        private void OnCollisionEnter(Collision other)
        {
            m_Actions?.OnCollisionEnter(other);
        }

        public void AffectHealth(ServerTank inflict, int HP)
        {
            NetworkTankState.HealthBarValue = Mathf.Min(100, NetworkTankState.HealthBarValue + HP);
            if (NetworkTankState.HealthBarValue <= 0)
            {
                NetworkTankState.LifeState = TankLifeState.Dead;
            }
        }

        public bool TakesDamage()
        {
            return NetworkTankState.LifeState == TankLifeState.Alive;
        }
    }
}