using System.Buffers.Binary;

namespace USRTG.F1_2020
{
    /// <summary>Sequential little-endian reader over a packet buffer.</summary>
    internal ref struct SpanReader
    {
        private readonly ReadOnlySpan<byte> _data;
        private int _pos;

        public SpanReader(ReadOnlySpan<byte> data, int offset = 0)
        {
            _data = data;
            _pos = offset;
        }

        public int Position => _pos;
        public int Remaining => _data.Length - _pos;

        public byte U8() => _data[_pos++];
        public sbyte I8() => (sbyte)_data[_pos++];

        public ushort U16()
        {
            var v = BinaryPrimitives.ReadUInt16LittleEndian(_data.Slice(_pos, 2));
            _pos += 2;
            return v;
        }

        public short I16()
        {
            var v = BinaryPrimitives.ReadInt16LittleEndian(_data.Slice(_pos, 2));
            _pos += 2;
            return v;
        }

        public uint U32()
        {
            var v = BinaryPrimitives.ReadUInt32LittleEndian(_data.Slice(_pos, 4));
            _pos += 4;
            return v;
        }

        public ulong U64()
        {
            var v = BinaryPrimitives.ReadUInt64LittleEndian(_data.Slice(_pos, 8));
            _pos += 8;
            return v;
        }

        public float F32()
        {
            var v = BinaryPrimitives.ReadSingleLittleEndian(_data.Slice(_pos, 4));
            _pos += 4;
            return v;
        }

        public void Skip(int bytes) => _pos += bytes;

        public float[] F32Array(int count)
        {
            var result = new float[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = F32();
            }
            return result;
        }

        public ushort[] U16Array(int count)
        {
            var result = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = U16();
            }
            return result;
        }

        public byte[] U8Array(int count)
        {
            var result = _data.Slice(_pos, count).ToArray();
            _pos += count;
            return result;
        }

        /// <summary>Null-terminated UTF-8 string in a fixed-size field.</summary>
        public string FixedString(int length)
        {
            var slice = _data.Slice(_pos, length);
            _pos += length;
            int end = slice.IndexOf((byte)0);
            if (end >= 0)
            {
                slice = slice[..end];
            }
            return System.Text.Encoding.UTF8.GetString(slice);
        }
    }
}
