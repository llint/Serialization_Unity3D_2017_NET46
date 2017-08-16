using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Serialization
{
	partial class SerializationOutput
	{
        public SerializationOutput Serialize(Boolean value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
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

        public SerializationOutput Serialize(Base o)
        {
            SerializationHelper<Base>.Serialize(this, o);
            return this;
        }
    }

	partial class SerializationInput
	{
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

        public SerializationInput Deserialize(out Base o)
        {
            SerializationHelper<Base>.Deserialize(this, out o);
            return this;
        }
    }

    static partial class TypeSerializationMethodMapping
    {
        static partial void InitializeMapping()
        {
            TypeSerializeMethodMapping = new Dictionary<Type, MethodInfo>
            {
                {typeof(int), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(int)})},
                {typeof(string), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(string)})},
            };
            TypeDeserializeMethodMapping = new Dictionary<Type, MethodInfo>
            {
                {typeof(int), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(int).MakeByRefType()})},
                {typeof(string), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(string).MakeByRefType()})},
            };
        }
    }
}
