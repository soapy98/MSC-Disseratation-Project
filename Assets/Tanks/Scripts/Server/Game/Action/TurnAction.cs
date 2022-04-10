using Tanks;
using Tanks.Server;
using Tanks.Shared;

public class TurnAction : Action
{
    private ServerTankMovement m_Move;

    public TurnAction(ServerTank parent, ref ActionData data) : base(parent, ref data)
    {
    }

    public override bool Start()
    {
        m_Move = m_parent.GetComponent<ServerTankMovement>();
        Turn();
        return true;
    }

    private void Turn()
    {
        var state = m_RequestData.WhichAction == ActionOption.DriverLeftAction
            ? MoveState.Left
            : MoveState.Right;
        m_Move.SetMoveValues(ActionInfo.Duration, state);
        foreach (var player in m_parent.NetworkTankState._PlayerInTeam)
        {
            if (player._NetworkRole.PRole == DifferentRoles.Driver)
            {
                m_parent.NetworkTankState.PlayerInfoByRole[DifferentRoles.Driver].EnergyPoints -= 10;
                return;
            }
        }
    }

    public override bool Update()
    {
        return true;
    }
}