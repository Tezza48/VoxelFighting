using Lidgren.Network;
using UnityEngine;
using zapnet;

public class PlayerState : ControllableState
{
    [HideInInspector] public BitFlags inputFlags;
    [HideInInspector] public Vector3 velocity;

    public override void Write(BaseEntity entity, NetOutgoingMessage buffer, bool isSpawning)
    {
        base.Write(entity, buffer, isSpawning);

        buffer.WriteCompressedVector3(velocity);
        buffer.Write((byte)inputFlags.Value);
    }

    public override void Read(BaseEntity entity, NetIncomingMessage buffer, bool isSpawning)
    {
        base.Read(entity, buffer, isSpawning);

        velocity = buffer.ReadCompressedVector3();
        inputFlags.Set(buffer.ReadByte());
    }
}
