using System.Buffers.Binary;
using USRTG.F1_2020;

namespace USRTG.Tests
{
    /// <summary>Writes synthetic F1 2020 datagrams with the exact spec layout.</summary>
    internal sealed class F1PacketBuilder
    {
        private readonly byte[] _buffer;
        private int _pos;

        public F1PacketBuilder(int size, byte packetId, byte playerCarIndex, uint frame = 1, ushort format = Spec.PacketFormat)
        {
            _buffer = new byte[size];
            U16(format);
            U8(1); U8(18); // game version
            U8(1);         // packet version
            U8(packetId);
            U64(0x1234);
            F32(12.5f);    // session time
            U32(frame);
            U8(playerCarIndex);
            U8(Spec.NoSecondaryPlayer);
        }

        public byte[] Bytes => _buffer;
        public int Position => _pos;

        public F1PacketBuilder Seek(int position) { _pos = position; return this; }
        public F1PacketBuilder U8(byte v) { _buffer[_pos++] = v; return this; }
        public F1PacketBuilder I8(sbyte v) { _buffer[_pos++] = (byte)v; return this; }
        public F1PacketBuilder U16(ushort v) { BinaryPrimitives.WriteUInt16LittleEndian(_buffer.AsSpan(_pos), v); _pos += 2; return this; }
        public F1PacketBuilder I16(short v) { BinaryPrimitives.WriteInt16LittleEndian(_buffer.AsSpan(_pos), v); _pos += 2; return this; }
        public F1PacketBuilder U32(uint v) { BinaryPrimitives.WriteUInt32LittleEndian(_buffer.AsSpan(_pos), v); _pos += 4; return this; }
        public F1PacketBuilder U64(ulong v) { BinaryPrimitives.WriteUInt64LittleEndian(_buffer.AsSpan(_pos), v); _pos += 8; return this; }
        public F1PacketBuilder F32(float v) { BinaryPrimitives.WriteSingleLittleEndian(_buffer.AsSpan(_pos), v); _pos += 4; return this; }
        public F1PacketBuilder Skip(int n) { _pos += n; return this; }

        public F1PacketBuilder F32s(params float[] values) { foreach (var v in values) F32(v); return this; }
        public F1PacketBuilder U16s(params ushort[] values) { foreach (var v in values) U16(v); return this; }
        public F1PacketBuilder U8s(params byte[] values) { foreach (var v in values) U8(v); return this; }

        public F1PacketBuilder Ascii(string s, int fieldLength)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(s);
            Array.Copy(bytes, 0, _buffer, _pos, Math.Min(bytes.Length, fieldLength));
            _pos += fieldLength;
            return this;
        }

        // ------------------------------------------------------------------ canned packets

        public const byte Player = 5;

        public static byte[] Session(byte sessionType = 10, sbyte trackId = 7, byte totalLaps = 52, ushort trackLength = 5891,
                                     sbyte airTemp = 21, sbyte trackTemp = 33, ushort timeLeft = 3456, bool paused = false, byte safetyCar = 0)
        {
            var b = new F1PacketBuilder(Spec.SessionSize, Spec.SessionId, Player);
            b.U8(2);                 // weather overcast
            b.I8(trackTemp);
            b.I8(airTemp);
            b.U8(totalLaps);
            b.U16(trackLength);
            b.U8(sessionType);
            b.I8(trackId);
            b.U8(0);                 // formula
            b.U16(timeLeft);
            b.U16(3600);             // duration
            b.U8(80);                // pit speed limit
            b.U8((byte)(paused ? 1 : 0));
            b.U8(0); b.U8(255);      // spectating, spectator index
            b.U8(0);                 // sli pro
            b.U8(21);                // marshal zones
            b.Skip(Spec.MarshalZoneCount * Spec.MarshalZoneSize);
            b.U8(safetyCar);
            b.U8(1);                 // network game
            b.U8(0);                 // forecast samples
            return b.Bytes;
        }

        public static byte[] LapData(float lastLap = 91.234f, float currentLap = 30.5f, ushort s1 = 28100, ushort s2 = 0, float bestLap = 90.001f,
                                     float lapDistance = 1472.75f, float totalDistance = 25000f, byte position = 4, byte lapNum = 6,
                                     byte pitStatus = 0, byte sector = 1, byte invalid = 0, byte penalties = 5)
        {
            var b = new F1PacketBuilder(Spec.LapDataSize, Spec.LapDataId, Player);
            b.Seek(Spec.HeaderSize + Player * Spec.LapDataEntrySize);
            b.F32(lastLap).F32(currentLap).U16(s1).U16(s2).F32(bestLap).U8(3);
            b.U16(1).U16(2).U16(3);                     // best lap sectors
            b.U16(4).U8(1).U16(5).U8(2).U16(6).U8(3);   // best overall sectors
            b.F32(lapDistance).F32(totalDistance).F32(0f);
            b.U8(position).U8(lapNum).U8(pitStatus).U8(sector).U8(invalid).U8(penalties).U8(9).U8(4).U8(2);
            return b.Bytes;
        }

        public static byte[] CarTelemetry()
        {
            var b = new F1PacketBuilder(Spec.CarTelemetrySize, Spec.CarTelemetryId, Player, frame: 777);
            b.Seek(Spec.HeaderSize + Player * Spec.CarTelemetryDataSize);
            b.U16(287).F32(0.75f).F32(-0.25f).F32(0.1f).U8(50).I8(7).U16(11500).U8(1).U8(80);
            b.U16s(401, 402, 403, 404);      // brakes RL, RR, FL, FR
            b.U8s(91, 92, 93, 94);           // surface temp RL, RR, FL, FR
            b.U8s(101, 102, 103, 104);       // inner temp RL, RR, FL, FR
            b.U16(105);                      // engine temp
            b.F32s(21.1f, 21.2f, 21.3f, 21.4f); // pressure RL, RR, FL, FR
            b.U8s(0, 0, 0, 0);
            b.Seek(Spec.HeaderSize + Spec.MaxCars * Spec.CarTelemetryDataSize);
            b.U32(0).U8(0).U8(255).I8(8);    // buttons, mfd, mfd 2, suggested gear
            return b.Bytes;
        }

        public static byte[] CarStatus(sbyte fiaFlag = 3)
        {
            var b = new F1PacketBuilder(Spec.CarStatusSize, Spec.CarStatusId, Player);
            b.Seek(Spec.HeaderSize + Player * Spec.CarStatusDataSize);
            b.U8(2).U8(1).U8(1).U8(56).U8(1);           // tc, abs, fuel mix, brake bias, pit limiter
            b.F32(42.5f).F32(110f).F32(12.3f);          // fuel in tank, capacity, remaining laps
            b.U16(12000).U16(3500).U8(8).U8(1).U16(0);  // max rpm, idle, gears, drs allowed, drs distance
            b.U8s(11, 12, 13, 14);                      // wear RL, RR, FL, FR
            b.U8(18).U8(17).U8(9);                      // actual C3, visual medium, age
            b.U8s(1, 2, 3, 4);                          // tyre damage RL, RR, FL, FR
            b.U8(5).U8(6).U8(7).U8(0).U8(8).U8(9);      // wings, drs fault, engine, gearbox
            b.I8(fiaFlag);
            b.F32(3_000_000f).U8(2).F32(1f).F32(2f).F32(3f);
            return b.Bytes;
        }

        public static byte[] Motion()
        {
            var b = new F1PacketBuilder(Spec.MotionSize, Spec.MotionId, Player);
            b.Seek(Spec.HeaderSize + Player * Spec.CarMotionDataSize);
            b.F32s(100f, 2f, 300f);          // world position
            b.F32s(10f, 0f, 30f);            // world velocity
            b.I16(0).I16(0).I16(32767).I16(32767).I16(0).I16(0);
            b.F32s(1.5f, -0.8f, 1.0f);       // g lateral, longitudinal, vertical
            b.F32s(0.5f, 0.01f, -0.02f);     // yaw, pitch, roll
            b.Seek(Spec.HeaderSize + Spec.MaxCars * Spec.CarMotionDataSize);
            b.Skip(3 * 16 + 16);             // suspension pos/vel/acc, wheel speed
            b.F32s(0.1f, 0.2f, 0.3f, 0.4f);  // wheel slip RL, RR, FL, FR
            b.F32s(31f, 0f, 1f);             // local velocity
            b.F32s(0.01f, 0.02f, 0.03f);     // angular velocity
            b.F32s(0f, 0f, 0f);              // angular acceleration
            b.F32(0.2f);                     // front wheels angle
            return b.Bytes;
        }

        public static byte[] Participants(byte numActive = 20, byte teamId = 1, string name = "HAMILTON")
        {
            var b = new F1PacketBuilder(Spec.ParticipantsSize, Spec.ParticipantsId, Player);
            b.U8(numActive);
            b.Seek(Spec.HeaderSize + 1 + Player * Spec.ParticipantDataSize);
            b.U8(0).U8(9).U8(teamId).U8(44).U8(10).Ascii(name, Spec.ParticipantNameLength).U8(1);
            return b.Bytes;
        }
    }
}
