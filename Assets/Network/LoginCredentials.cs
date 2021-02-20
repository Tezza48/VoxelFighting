using Lidgren.Network;
using zapnet;

public class LoginCredentials : INetworkPacket
{
    public int ServerVersion;
    public string Username;

    public virtual void Write(NetOutgoingMessage buffer)
    {
        buffer.Write(ServerVersion);
        buffer.Write(Username);
    }

    public virtual bool Read(NetIncomingMessage buffer)
    {
        ServerVersion = buffer.ReadInt32();
        Username = buffer.ReadString();

        return true;
    }

    public void OnRecycled() { }

    public void OnFetched() { }
}