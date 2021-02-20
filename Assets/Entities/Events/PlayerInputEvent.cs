using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zapnet;

public class PlayerInputEvent : BaseInputEvent
{
    public ref BitFlags InputFlags
    {
        get => ref _inputFlags;
    }

    public float Pitch { get; set; }

    public float Yaw { get; set; }

    private BitFlags _inputFlags;

    public override void Write(NetOutgoingMessage buffer)
    {
        buffer.Write((byte)InputFlags.Value);
        buffer.Write(Pitch);
        buffer.Write(Yaw);

        base.Write(buffer);
    }

    public override bool Read(NetIncomingMessage buffer)
    {
        InputFlags.Set(buffer.ReadByte());
        Pitch = buffer.ReadSingle();
        Yaw = buffer.ReadSingle();

        return base.Read(buffer);
    }
}
