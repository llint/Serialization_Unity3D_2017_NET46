using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Serialization
{
static class BasicSerializationTypes
    {
        internal static readonly Type[] SerializationTypes = new Type[] {
        typeof(Boolean),
        typeof(Char),
        typeof(Int16),
        typeof(UInt16),
        typeof(Int32),
        typeof(UInt32),
        typeof(Int64),
        typeof(UInt64),
        typeof(Single),
        typeof(Double),
    };
    }

    class SerializationException : Exception
    {
    }

    partial class SerializationOutput
    {
        MemoryStream stream;

        public SerializationOutput()
        {
            stream = new MemoryStream();
        }

        public MemoryStream GetStream()
        {
            return stream;
        }

        public SerializationOutput Serialize(Boolean value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        public SerializationOutput Serialize(Byte value)
        {
            stream.WriteByte(value);
            return this;
        }

        public SerializationOutput Serialize(SByte value)
        {
            stream.WriteByte((Byte)value);
            return this;
        }

        public SerializationOutput Serialize(Int16 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        public SerializationOutput Serialize(UInt16 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        public SerializationOutput Serialize(Int32 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        public SerializationOutput Serialize(UInt32 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        public SerializationOutput Serialize(Int64 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        public SerializationOutput Serialize(UInt64 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        public SerializationOutput Serialize(Single value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        public SerializationOutput Serialize(Double value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        public SerializationOutput Serialize(String s)
        {
            var buffer = Encoding.UTF8.GetBytes(s);
            Serialize((Int16)buffer.Length);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        public SerializationOutput Serialize<T>(T o)
        {
            SerializationHelper<T>.Serialize(this, o);
            return this;
        }
    }

    partial class SerializationInput
    {
        MemoryStream stream;

        public SerializationInput(MemoryStream stream_)
        {
            stream = stream_;
            stream.Seek(0, SeekOrigin.Begin);
        }

        public MemoryStream GetStream()
        {
            return stream;
        }

        public SerializationInput Deserialize(out bool value)
        {
            var buffer = BitConverter.GetBytes(default(bool));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToBoolean(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }

        public SerializationInput Deserialize(out Byte value)
        {
            var r = stream.ReadByte();
            if (r == -1)
            {
                throw new SerializationException();
            }
            value = (Byte)r;
            return this;
        }

        public SerializationInput Deserialize(out SByte value)
        {
            var r = stream.ReadByte();
            if (r == -1)
            {
                throw new SerializationException();
            }
            value = (SByte)r;
            return this;
        }

        public SerializationInput Deserialize(out Int16 value)
        {
            var buffer = BitConverter.GetBytes(default(Int16));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToInt16(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }

        public SerializationInput Deserialize(out UInt16 value)
        {
            var buffer = BitConverter.GetBytes(default(UInt16));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToUInt16(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }

        public SerializationInput Deserialize(out Int32 value)
        {
            var buffer = BitConverter.GetBytes(default(Int32));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToInt32(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }

        public SerializationInput Deserialize(out UInt32 value)
        {
            var buffer = BitConverter.GetBytes(default(UInt32));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToUInt32(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }

        public SerializationInput Deserialize(out Int64 value)
        {
            var buffer = BitConverter.GetBytes(default(Int64));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToInt64(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }

        public SerializationInput Deserialize(out UInt64 value)
        {
            var buffer = BitConverter.GetBytes(default(UInt64));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToUInt64(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }

        public SerializationInput Deserialize(out Single value)
        {
            var buffer = BitConverter.GetBytes(default(Single));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToSingle(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }

        public SerializationInput Deserialize(out Double value)
        {
            var buffer = BitConverter.GetBytes(default(Double));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToDouble(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }

        public SerializationInput Deserialize(out String value)
        {
            UInt16 len = 0;
            Deserialize(out len);
            var buffer = new byte[len];
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = Encoding.UTF8.GetString(buffer);
                return this;
            }
            throw new SerializationException();
        }

        public SerializationInput Deserialize<T>(out T o)
        {
            SerializationHelper<T>.Deserialize(this, out o);
            return this;
        }
    }
}
