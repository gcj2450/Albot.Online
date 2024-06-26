﻿using System.Diagnostics;
using System.Runtime.Serialization;
using Barebones.Networking;
using UnityEngine.Networking;

namespace Barebones.MasterServer
{
    public class SaveRoomOptionsPacket : SerializablePacket
    {
        public string RoomId;
        public RoomOptions Options;

        public override void ToBinaryWriter(EndianBinaryWriter writer)
        {
            writer.Write(RoomId);
            writer.Write(Options);
        }

        public override void FromBinaryReader(EndianBinaryReader reader)
        {
            RoomId = reader.ReadString();
            Options = reader.ReadPacket(new RoomOptions());
        }
    }
}