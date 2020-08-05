﻿using Deadlocked.Server.Stream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Deadlocked.Server.Messages.Lobby
{
    [MediusApp(MediusAppPacketIds.AccountUpdateStats)]
    public class MediusAccountUpdateStatsRequest : BaseLobbyMessage
    {

        public override MediusAppPacketIds Id => MediusAppPacketIds.AccountUpdateStats;

        public string SessionKey; // SESSIONKEY_MAXLEN
        public string Stats; // ACCOUNTSTATS_MAXLEN

        public override void Deserialize(BinaryReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            SessionKey = reader.ReadString(MediusConstants.SESSIONKEY_MAXLEN);
            Stats = reader.ReadString(MediusConstants.ACCOUNTSTATS_MAXLEN);
        }

        public override void Serialize(BinaryWriter writer)
        {
            // 
            base.Serialize(writer);

            // 
            writer.Write(SessionKey, MediusConstants.SESSIONKEY_MAXLEN);
            writer.Write(Stats, MediusConstants.ACCOUNTSTATS_MAXLEN);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
             $"SessionKey:{SessionKey}" + " " +
$"Stats:{Stats}";
        }
    }
}