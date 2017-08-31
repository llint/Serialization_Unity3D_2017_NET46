using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using UnityEngine;

namespace Serialization
{
    public partial class SerializationOutput
    {
        public SerializationOutput Serialize(Boolean value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }
        public SerializationOutput Serialize(Char value)
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
        public SerializationOutput Serialize(Base value)
        {
            return SerializationHelper<Base>.Serialize(this, value);
        }
        public SerializationOutput Serialize(Derived value)
        {
            return SerializationHelper<Derived>.Serialize(this, value);
        }
        public SerializationOutput Serialize(Struct value)
        {
            return SerializationHelper<Struct>.Serialize(this, value);
        }
    }
    public partial class SerializationInput
    {
        public SerializationInput Deserialize(out Boolean value)
        {
            position = stream.Position;
            var buffer = BitConverter.GetBytes(default(Boolean));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToBoolean(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }
        public SerializationInput Deserialize(out Char value)
        {
            position = stream.Position;
            var buffer = BitConverter.GetBytes(default(Char));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToChar(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }
        public SerializationInput Deserialize(out Int16 value)
        {
            position = stream.Position;
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
            position = stream.Position;
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
            position = stream.Position;
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
            position = stream.Position;
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
            position = stream.Position;
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
            position = stream.Position;
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
            position = stream.Position;
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
            position = stream.Position;
            var buffer = BitConverter.GetBytes(default(Double));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToDouble(buffer, 0);
                return this;
            }
            throw new SerializationException();
        }
        public SerializationInput Deserialize(out Base value)
        {
            position = stream.Position;
            return SerializationHelper<Base>.Deserialize(this, out value);
        }
        public SerializationInput Deserialize(out Derived value)
        {
            position = stream.Position;
            return SerializationHelper<Derived>.Deserialize(this, out value);
        }
        public SerializationInput Deserialize(out Struct value)
        {
            position = stream.Position;
            return SerializationHelper<Struct>.Deserialize(this, out value);
        }
    }
    public static partial class TypeSerializationMethodMapping
    {
        static partial void InitializeMapping()
        {
            TypeSerializeMethodMapping = new Dictionary<Type, MethodInfo>
            {
                { typeof(Boolean), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Boolean)}) },
                { typeof(Char), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Char)}) },
                { typeof(Int16), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Int16)}) },
                { typeof(UInt16), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(UInt16)}) },
                { typeof(Int32), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Int32)}) },
                { typeof(UInt32), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(UInt32)}) },
                { typeof(Int64), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Int64)}) },
                { typeof(UInt64), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(UInt64)}) },
                { typeof(Single), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Single)}) },
                { typeof(Double), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Double)}) },
                { typeof(Byte), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Byte)}) },
                { typeof(SByte), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(SByte)}) },
                { typeof(String), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(String)}) },
                { typeof(Byte[]), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Byte[])}) },
                { typeof(Base), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Base)}) },
                { typeof(Derived), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Derived)}) },
                { typeof(Struct), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(Struct)}) },
            };
            TypeDeserializeMethodMapping = new Dictionary<Type, MethodInfo>
            {
                { typeof(Boolean), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Boolean).MakeByRefType()}) },
                { typeof(Char), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Char).MakeByRefType()}) },
                { typeof(Int16), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Int16).MakeByRefType()}) },
                { typeof(UInt16), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(UInt16).MakeByRefType()}) },
                { typeof(Int32), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Int32).MakeByRefType()}) },
                { typeof(UInt32), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(UInt32).MakeByRefType()}) },
                { typeof(Int64), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Int64).MakeByRefType()}) },
                { typeof(UInt64), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(UInt64).MakeByRefType()}) },
                { typeof(Single), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Single).MakeByRefType()}) },
                { typeof(Double), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Double).MakeByRefType()}) },
                { typeof(Byte), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Byte).MakeByRefType()}) },
                { typeof(SByte), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(SByte).MakeByRefType()}) },
                { typeof(String), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(String).MakeByRefType()}) },
                { typeof(Byte[]), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Byte[]).MakeByRefType()}) },
                { typeof(Base), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Base).MakeByRefType()}) },
                { typeof(Derived), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Derived).MakeByRefType()}) },
                { typeof(Struct), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(Struct).MakeByRefType()}) },
            };
        }
    }
    public static partial class SerializableTypesRegistry
    {
        static partial void Initialize()
        {
            serializableTypes = new Type[]
            {
                typeof(Base),
                typeof(Derived),
                typeof(Struct),
            };
            typeIndexMapping = new Dictionary<Type, int>
            {
                { typeof(Base), 0 },
                { typeof(Derived), 1 },
                { typeof(Struct), 2 },
            };
            typeIndexedSerializeDelegates = new Serialize[]
            {
                (SerializationOutput so, object o) => so.Serialize((Base)o),
                (SerializationOutput so, object o) => so.Serialize((Derived)o),
                (SerializationOutput so, object o) => so.Serialize((Struct)o),
            };
            typeIndexedDeserializeDelegates = new Deserialize[]
            {
                (SerializationInput si, out object o) => { Base x; si.Deserialize(out x); o = x; return si; },
                (SerializationInput si, out object o) => { Derived x; si.Deserialize(out x); o = x; return si; },
                (SerializationInput si, out object o) => { Struct x; si.Deserialize(out x); o = x; return si; },
            };
        }
    }
    public static partial class AssemblyManager
    {
        static partial void LoadAssemblyImpl(Module module)
        {
            SerializationHelper<Base>.LoadAssembly(module);
            SerializationHelper<Derived>.LoadAssembly(module);
            SerializationHelper<Struct>.LoadAssembly(module);
        }
        static partial void CreateAssemblyImpl(ModuleBuilder moduleBuilder)
        {
            SerializationHelper<Base>.CreateAssembly(moduleBuilder);
            SerializationHelper<Derived>.CreateAssembly(moduleBuilder);
            SerializationHelper<Struct>.CreateAssembly(moduleBuilder);
        }
    }
}
