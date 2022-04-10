using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using Tanks.Server;
using UnityEngine;
public class PlayersOnServer : NetworkBehaviour
{
    static readonly List<ServerPlayer> m_ActivePlayers = new List<ServerPlayer>();

    [SerializeField] private ServerPlayer m_CachedPlayer;
    public override void NetworkStart()
    {
        if (!IsServer) enabled = false;
    }

    private void OnEnable()
    {
        m_ActivePlayers.Add(m_CachedPlayer);
    }

    private void OnDisable()
    {
        m_ActivePlayers.Remove(m_CachedPlayer);
    }

    public static List<ServerPlayer> GetServerPlayers()
    {
        return m_ActivePlayers;
    }
}