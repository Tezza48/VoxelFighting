﻿using Lidgren.Network;
using UnityEngine;
using zapnet;

public class ClientHandler : IClientHandler
{
    private int _serverVersion;

    public void ReadInitialData(NetIncomingMessage buffer)
    {
        var message = buffer.ReadString();
        Debug.Log("The server said: " + message);
    }

    public INetworkPacket GetCredentialsPacket()
    {
        var packet = Zapnet.Network.CreatePacket<LoginCredentials>();

        packet.ServerVersion = _serverVersion;

        packet.Username = "Player";

        return packet;
    }

    public void OnDisconnected()
    {

    }

    public void OnShutdown()
    {

    }

    public ClientHandler(int serverVersion)
    {
        _serverVersion = serverVersion;
    }
}
