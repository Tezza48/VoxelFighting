using Lidgren.Network;
using UnityEngine;
using zapnet;

public class ServerHandler : IServerHandler
{
    private int _serverVersion;

    public void WriteInitialData(Player player, NetOutgoingMessage buffer)
    {
        buffer.Write("Hello, player!");
    }

    public bool CanPlayerAuth(INetworkPacket data)
    {
        var credentials = (LoginCredentials)data;

        if (credentials.ServerVersion < _serverVersion)
        {
            return false;
        }

        return true;
    }

    public void OnPlayerDisconnected(Player player)
    {
        var entity = (player.Entity as BasePlayer);

        if (entity != null)
        {
            Zapnet.Entity.Remove(entity);
        }
    }

    public void OnInitialDataReceived(Player player, INetworkPacket data)
    {
        var credentials = (LoginCredentials)data;

        Debug.Log(credentials.Username + " has connected!");

        CreatePlayer(player, credentials);
    }

    public void OnPlayerConnected(Player player, INetworkPacket credentials)
    {

    }

    public void CreatePlayer(Player player, LoginCredentials credentials)
    {
        Debug.Log("Creating player entity");
        var entity = Zapnet.Entity.Create<BasePlayer>("PlayerEntity");

        entity.Name.Value = credentials.Username;
        entity.AssignControl(player);

        entity.transform.position = new Vector3(16.0f, 34.0f, 16.0f);
    }

    public ServerHandler(int serverVersion)
    {
        _serverVersion = serverVersion;
    }
}
