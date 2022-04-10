using System.Collections.Generic;
using System.IO;
using MLAPI;
using MLAPI.Spawning;
using Tanks;
using Tanks.Server;
using Tanks.Shared;
using UnityEngine;
//Controls the bullet on the server alongside enabling collision to occur
public class ServerBulletState : NetworkBehaviour
{
    private bool m_Began = false;
    [SerializeField] private NetworkBulletState m_State;
    [SerializeField] private SphereCollider m_Collider;
    private Collider[] m_CollisionCache = new Collider[5];
    private ulong m_OwnerTanksNetworkID;

    private ActionInfo.BulletInfo m_BulletInfo;
    private const float m_WallRemainTime = 2.0f, m_TankRemainTime = 0.2f;

    private float m_DeleteTime;

    private List<GameObject> m_Collisions = new List<GameObject>();
    private bool m_Finished;

    private int m_WallMask, m_TankMask;

    public void Init(ulong tanksOwnerID, in ActionInfo.BulletInfo bullet)
    {
        m_OwnerTanksNetworkID = tanksOwnerID;
        m_BulletInfo = bullet;
    }

    public override void NetworkStart(Stream stream)
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }

        m_Began = true;
        m_DeleteTime = Time.fixedTime + (m_BulletInfo.Distance / m_BulletInfo.Speed);
        m_WallMask = LayerMask.GetMask(new[] {"Wall"});
        m_TankMask = LayerMask.NameToLayer("Tank");
        UpdateNetworkState();
    }

    private void FixedUpdate()
    {
        if (!m_Began) return;
        var transform1 = transform;
        var difference = transform1.forward * (m_BulletInfo.Speed * Time.fixedDeltaTime);
        transform1.position += difference;
        if (m_DeleteTime < Time.fixedTime)
            Destroy(gameObject);
        if (!m_Finished)
            CollisionsDetections();
        UpdateNetworkState();
    }

    private void CollisionsDetections()
    {
        var pos = transform.localToWorldMatrix.MultiplyPoint(m_Collider.center);
        var cols = Physics.OverlapSphereNonAlloc(pos, m_Collider.radius, m_CollisionCache);
        if (cols < 1) return;
        for (var i = 0; i < cols; i++)
        {
            var mask = 1 << m_CollisionCache[i].gameObject.layer;
            if ((mask & m_WallMask) != 0)
            {
                m_BulletInfo.Speed = 0;
                m_Finished = true;
                m_DeleteTime = Time.fixedTime + m_WallRemainTime;
                return;
            }

            if (m_CollisionCache[i].gameObject.layer == m_TankMask || !m_Collisions.Contains(m_Collisions[i].gameObject))
            {
                m_Collisions.Add(m_CollisionCache[i].gameObject);
              
                var enemyTank = m_CollisionCache[i].GetComponentInParent<NetworkObject>();
                if (enemyTank)
                {
                    NetworkSpawnManager.SpawnedObjects.TryGetValue(m_OwnerTanksNetworkID, out NetworkObject owner);
                    var tankOwner = owner != null ? owner.GetComponentInParent<ServerTank>() : null;
                    if (enemyTank.NetworkObjectId != m_OwnerTanksNetworkID)
                    {
                        enemyTank.GetComponent<IDamage>().AffectHealth(tankOwner, -m_BulletInfo.Damage);
                        m_BulletInfo.Speed = 0;
                        m_Finished = true;
                        m_DeleteTime = Time.fixedTime + m_WallRemainTime;
                    }
                }

                if (m_Finished)
                    return;
            }
        }
    }

    private void UpdateNetworkState()
    {
        var transform1 = transform;
        m_State.ObjectNetworkPosition.Value = transform1.position;
        m_State.ObjectNetworkRotationY.Value = transform1.eulerAngles.y;
        m_State.ObjectMovementSpeed.Value = m_BulletInfo.Speed;
    }
}