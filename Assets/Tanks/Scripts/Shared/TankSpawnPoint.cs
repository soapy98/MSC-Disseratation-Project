using MLAPI;
using UnityEngine;

namespace Tanks.Shared
{
    //Is responsible for spawning the tanks and setting the correct information for them alongside the UI
    public class TankSpawnPoint : NetworkBehaviour
    {
        [SerializeField] private Transform m_Team1Spawn;
        [SerializeField] private Transform m_Team2Spawn;
        [SerializeField] private NetworkObject Tank;
        public TankOnServer m_Tanks;

        public void SpawnTanks()
        {
            m_Tanks = GetComponent<TankOnServer>();
            for (var teamValue = 1; teamValue < 3;)
            {
                var t = Instantiate(Tank);
                t.transform.position = teamValue == 1
                    ? m_Team1Spawn.transform.position
                    : m_Team2Spawn.transform.position;
                t.GetComponent<NetworkTankState>().Team = teamValue;
                t.name = "TankTeam" + teamValue;
                t.GetComponent<NetworkTankState>().m_TankNameUI.text = t.name;
                t.GetComponent<NetworkTankState>().HealthBarValue = 40;
                t.GetComponent<UIHealthLocalInfo>().InitalizeHealth(t.GetComponent<NetworkTankState>().Health.HealthPoints,40);
                if (!NetworkManager.Singleton.IsListening) continue;
                
                if (!t.IsSpawned) t.GetComponent<NetworkObject>().Spawn(null, true);
                m_Tanks.AddTankToServerListList(t);
                teamValue++;
            }
        }
    }
}