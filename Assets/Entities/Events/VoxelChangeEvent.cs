using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zapnet;

public class VoxelChangeEvent : BaseEventData
{
    public Vector3Int location;
    public Color color;

    public override bool Read(NetIncomingMessage buffer)
    {
        location = MathUtils.ToVector3Int(buffer.ReadVector3());
        color = buffer.ReadRgbaColor();

        return true;
    }

    public override void Write(NetOutgoingMessage buffer)
    {
        buffer.Write(location);
        buffer.Write(color);
    }
}
