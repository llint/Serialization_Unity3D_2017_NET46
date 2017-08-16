using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;

using UnityEngine;

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

    static class SerializationHelper<T>
    {
        internal delegate SerializationOutput Delegate_Serialize(SerializationOutput so, T o);
        internal delegate SerializationInput Delegate_Deserialize(SerializationInput si, out T o);

        internal static Delegate_Serialize Serialize { get; private set; }
        internal static Delegate_Deserialize Deserialize { get; private set; }

        // (so, o) => so.Serialize(o.i).Serialize(o.s).Serialize(o.a);
        internal static void CreateDelegate_Serialize()
        {
            var type = typeof(T);
            ParameterExpression so = Expression.Parameter(typeof(SerializationOutput), "so");
            ParameterExpression o = Expression.Parameter(type, "o");

            MethodCallExpression serializeExpression = null;

            foreach (var fi in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                MemberExpression m = Expression.Field(o, fi);
                serializeExpression = Expression.Call(serializeExpression == null ? (Expression)so : (Expression)serializeExpression,
                    TypeSerializationMethodMapping.TypeSerializeMethodMapping[fi.FieldType], m);
            }

            Debug.Log(serializeExpression.ToString());

            var lambda = Expression.Lambda<Delegate_Serialize>(serializeExpression, so, o);
            Debug.Log(lambda.ToString());

            Serialize = lambda.Compile();
        }

        // (si, out o) => { o = new Base(); return si.Deserialize(o.i).Deserialize(o.s); }
        // it could also be "(si, out o) => { o = new Base(); si.Deserialize(o.i).Deserialize(o.s); return si; }"
        internal static void CreateDelegate_Deserialize()
        {
            var type = typeof(T);
            ParameterExpression si = Expression.Parameter(typeof(SerializationInput), "si");
            ParameterExpression o = Expression.Parameter(type.MakeByRefType(), "o");

            BinaryExpression instantiateExpression = Expression.Assign(o, Expression.New(type));
            Debug.Log(instantiateExpression.ToString());

            MethodCallExpression deserializeExpression = null;

            foreach (var fi in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                MemberExpression m = Expression.Field(o, fi);
                deserializeExpression = Expression.Call(deserializeExpression == null ? (Expression)si : (Expression)deserializeExpression,
                    TypeSerializationMethodMapping.TypeDeserializeMethodMapping[fi.FieldType], m);
            }

            Debug.Log(deserializeExpression.ToString());

            LabelTarget lableTarget = Expression.Label(typeof(SerializationInput));
            GotoExpression returnExpression = Expression.Return(lableTarget, deserializeExpression, typeof(SerializationInput));
            LabelExpression lableExpression = Expression.Label(lableTarget, si);

            BlockExpression block = Expression.Block(typeof(SerializationInput),
                instantiateExpression,
                returnExpression, // NB: 'deserializeExpression' is embedded in the 'returnExpression"!
                lableExpression);
            Debug.Log(block.ToString());

            var lambda = Expression.Lambda<Delegate_Deserialize>(block, si, o);
            Debug.Log(lambda.ToString());

            Deserialize = lambda.Compile();
        }
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
