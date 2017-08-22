using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Legacy
{
    class SerializationException : Exception
    {
    }

    public static class SerializableTypeRegistry
    {
        static Dictionary<Type, UInt16> type2id = new Dictionary<Type, UInt16>();
        static Dictionary<UInt16, Type> id2type = new Dictionary<UInt16, Type>();

        private static bool initialized = false;

        public static bool Initialize()
        {
#if UNITY_EDITOR //so validation can check for collisions
            if (!UnityEngine.Application.isPlaying)
            {
                initialized = false;
                type2id.Clear();
                id2type.Clear();
            }
#endif
            if (initialized)
            {
                return true;
            }
            initialized = true;
            SortedDictionary<String, Type> types = new SortedDictionary<String, Type>();
            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in allTypes)
            {
                if (type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0)
                {
                    types.Add(type.ToString(), type);
                }
            }
            foreach (var type in types)
            {
                if (!AddType(type.Value))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AddType(Type type)
        {
            if (type2id.ContainsKey(type))
            {
                // Debug.LogErrorFormat("Duplicate type detected! | Type : {0}", type);
                return false;
            }

            UInt16 nextTypeId = 0;
            using (var md5Hasher = System.Security.Cryptography.MD5.Create())
            {
                var data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(type.ToString()));
                nextTypeId = BitConverter.ToUInt16(data, 0);
            }

            if (id2type.ContainsKey(nextTypeId))
            {
                // Debug.LogErrorFormat("Duplicate id detected! | Added Type : {0} | Other Type : {1}", type, id2type[nextTypeId]);
                return false;
            }

            // Debug.LogFormat(d.Game, "### AddType: {0}, {1}", nextTypeId, type);
            type2id.Add(type, nextTypeId);
            id2type.Add(nextTypeId, type);

            return true;
        }

        public static UInt16 GetIdByType(Type type)
        {
            UInt16 id;
            if (type2id.TryGetValue(type, out id))
            {
                return id;
            }
            throw new Exception();
        }

        public static Type GetTypeById(UInt16 id)
        {
            Type type;
            if (id2type.TryGetValue(id, out type))
            {
                return type;
            }
            throw new Exception();
        }
    }

    class SerializationOutput
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

        public void Serialize(Boolean value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Serialize(Byte value)
        {
            stream.WriteByte(value);
        }

        public void Serialize(SByte value)
        {
            stream.WriteByte((Byte)value);
        }

        public void Serialize(Int16 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Serialize(UInt16 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Serialize(Int32 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Serialize(UInt32 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Serialize(Int64 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Serialize(UInt64 value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Serialize(Single value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Serialize(Double value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Serialize(String s)
        {
            var buffer = Encoding.UTF8.GetBytes(s);
            Serialize((Int16)buffer.Length);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Serialize(Object o)
        {
            Serialize(o != null);
            if (o != null)
            {
                if (o.GetType() == typeof(Boolean)) // needs to use the actual type for serialization purposes
                {
                    Serialize((Boolean)o);
                }
                else if (o.GetType() == typeof(Byte))
                {
                    Serialize((Byte)o);
                }
                else if (o.GetType() == typeof(SByte))
                {
                    Serialize((SByte)o);
                }
                else if (o.GetType() == typeof(Int16))
                {
                    Serialize((Int16)o);
                }
                else if (o.GetType() == typeof(UInt16))
                {
                    Serialize((UInt16)o);
                }
                else if (o.GetType() == typeof(Int32))
                {
                    Serialize((Int32)o);
                }
                else if (o.GetType() == typeof(UInt32))
                {
                    Serialize((UInt32)o);
                }
                else if (o.GetType() == typeof(Int64))
                {
                    Serialize((Int64)o);
                }
                else if (o.GetType() == typeof(UInt64))
                {
                    Serialize((UInt64)o);
                }
                else if (o.GetType() == typeof(Single))
                {
                    Serialize((Single)o);
                }
                else if (o.GetType() == typeof(Double))
                {
                    Serialize((Double)o);
                }
                else if (o.GetType() == typeof(String))
                {
                    Serialize((String)o);
                }
                else if (o.GetType().IsEnum)
                {
                    Serialize(Convert.ChangeType(o, Enum.GetUnderlyingType(o.GetType())));
                }
                else if (o.GetType().IsArray) // we support [] of all the supported element types (resursive)
                {
                    var a = o as Array;
                    Serialize((UInt16)a.Length);
                    foreach (var x in a)
                    {
                        Serialize(x);
                    }
                }
                else if (o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition() == typeof(List<>)) // we support List<> of all the supported element types (recursive)
                {
                    var l = o as IList;
                    Serialize((UInt16)l.Count);
                    foreach (var x in l)
                    {
                        Serialize(x);
                    }
                }
                else if (o.GetType().GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0)
                {
                    if (!o.GetType().IsValueType)
                    {
#if FP_DEBUG
					try
#endif
                        {
                            ushort serializeType = SerializableTypeRegistry.GetIdByType(o.GetType());
                            Serialize(serializeType);
                        }
#if FP_DEBUG
					catch (Exception ex)
					{
						Debug.LogError("Failed to get type to serialize : " + o.GetType().Name + " !");
						throw ex; // Crash and burn
					}
#endif
                    }
                    foreach (var fi in o.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (Attribute.GetCustomAttribute(fi, typeof(NonSerializedAttribute)) == null)
                        {
                            Serialize(fi.GetValue(o));
                        }
                    }
                    foreach (var fi in o.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (Attribute.GetCustomAttribute(fi, typeof(UnityEngine.SerializeField)) != null)
                        {
                            Serialize(fi.GetValue(o));
                        }
                    }
                }
                else // unsupported type
                {
                    UnityEngine.Debug.LogError(string.Format("Serialization Exception: {0}", o));
                    throw new SerializationException();
                }
            }
        }

        public void Serialize(byte[] buffer)
        {
            var n = buffer.Length;
            Serialize(n);
            stream.Write(buffer, 0, buffer.Length);
        }
    }

    class SerializationInput
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

        public void Serialize(out bool value)
        {
            var buffer = BitConverter.GetBytes(default(bool));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToBoolean(buffer, 0);
                return;
            }
            throw new SerializationException();
        }

        public void Serialize(out Byte value)
        {
            var r = stream.ReadByte();
            if (r == -1)
            {
                throw new SerializationException();
            }
            value = (Byte)r;
        }

        public void Serialize(out SByte value)
        {
            var r = stream.ReadByte();
            if (r == -1)
            {
                throw new SerializationException();
            }
            value = (SByte)r;
        }

        public void Serialize(out Int16 value)
        {
            var buffer = BitConverter.GetBytes(default(Int16));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToInt16(buffer, 0);
                return;
            }
            throw new SerializationException();
        }

        public void Serialize(out UInt16 value)
        {
            var buffer = BitConverter.GetBytes(default(UInt16));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToUInt16(buffer, 0);
                return;
            }
            throw new SerializationException();
        }

        public void Serialize(out Int32 value)
        {
            var buffer = BitConverter.GetBytes(default(Int32));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToInt32(buffer, 0);
                return;
            }
            throw new SerializationException();
        }

        public void Serialize(out UInt32 value)
        {
            var buffer = BitConverter.GetBytes(default(UInt32));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToUInt32(buffer, 0);
                return;
            }
            throw new SerializationException();
        }

        public void Serialize(out Int64 value)
        {
            var buffer = BitConverter.GetBytes(default(Int64));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToInt64(buffer, 0);
                return;
            }
            throw new SerializationException();
        }

        public void Serialize(out UInt64 value)
        {
            var buffer = BitConverter.GetBytes(default(UInt64));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToUInt64(buffer, 0);
                return;
            }
            throw new SerializationException();
        }

        public void Serialize(out Single value)
        {
            var buffer = BitConverter.GetBytes(default(Single));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToSingle(buffer, 0);
                return;
            }
            throw new SerializationException();
        }

        public void Serialize(out Double value)
        {
            var buffer = BitConverter.GetBytes(default(Double));
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = BitConverter.ToDouble(buffer, 0);
                return;
            }
            throw new SerializationException();
        }

        public void Serialize(out String value)
        {
            UInt16 len = 0;
            Serialize(out len);
            var buffer = new byte[len];
            if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                value = Encoding.UTF8.GetString(buffer);
                return;
            }
            throw new SerializationException();
        }

        public void Serialize(out object o, Type type = null)
        {
            o = null;
            bool isNotNull;
            Serialize(out isNotNull);
            if (isNotNull)
            {
                if (type == typeof(bool))
                {
                    bool val;
                    Serialize(out val);
                    o = val;
                }
                else if (type == typeof(Byte))
                {
                    Byte val;
                    Serialize(out val);
                    o = val;
                }
                else if (type == typeof(SByte))
                {
                    SByte val;
                    Serialize(out val);
                    o = val;
                }
                else if (type == typeof(Int16))
                {
                    Int16 val;
                    Serialize(out val);
                    o = val;
                }
                else if (type == typeof(UInt16))
                {
                    UInt16 val;
                    Serialize(out val);
                    o = val;
                }
                else if (type == typeof(Int32))
                {
                    Int32 val;
                    Serialize(out val);
                    o = val;
                }
                else if (type == typeof(UInt32))
                {
                    UInt32 val;
                    Serialize(out val);
                    o = val;
                }
                else if (type == typeof(Int64))
                {
                    Int64 val;
                    Serialize(out val);
                    o = val;
                }
                else if (type == typeof(UInt64))
                {
                    UInt64 val;
                    Serialize(out val);
                    o = val;
                }
                else if (type == typeof(Single))
                {
                    Single val;
                    Serialize(out val);
                    o = val;
                }
                else if (type == typeof(Double))
                {
                    Double val;
                    Serialize(out val);
                    o = val;
                }
                else if (type == typeof(String))
                {
                    String val;
                    Serialize(out val);
                    o = val;
                }
                else if (type.IsEnum)
                {
                    Object x;
                    Serialize(out x, Enum.GetUnderlyingType(type));
                    o = Enum.ToObject(type, x);
                }
                else if (type.IsArray)
                {
                    UInt16 n;
                    Serialize(out n);
                    o = Array.CreateInstance(type.GetElementType(), n);
                    var a = o as Array;
                    for (int i = 0; i < (int)n; ++i)
                    {
                        object x;
                        Serialize(out x, type.GetElementType());
                        a.SetValue(x, i);
                    }
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    UInt16 n;
                    Serialize(out n);
                    o = Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]));
                    var l = o as IList;
                    for (int i = 0; i < (int)n; ++i)
                    {
                        object x;
                        Serialize(out x, type.GetGenericArguments()[0]);
                        l.Add(x);
                    }
                }
                else if (type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0)
                {
                    if (!type.IsValueType)
                    {
                        UInt16 typeId;
                        Serialize(out typeId);
                        type = SerializableTypeRegistry.GetTypeById(typeId);
                    }
                    o = Activator.CreateInstance(type);
                    foreach (var fi in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (Attribute.GetCustomAttribute(fi, typeof(NonSerializedAttribute)) == null)
                        {
                            Object x;
                            Serialize(out x, fi.FieldType);
                            fi.SetValue(o, x);
                        }
                    }
                    foreach (var fi in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (Attribute.GetCustomAttribute(fi, typeof(UnityEngine.SerializeField)) != null)
                        {
                            Object x;
                            Serialize(out x, fi.FieldType);
                            fi.SetValue(o, x);
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError(string.Format("Serialization Exception o:{0} Type : {1}", o, type));
                    throw new SerializationException();
                }
            }
        }

        public object Serialize(Type type)
        {
            object o;
            Serialize(out o, type);
            return o;
        }

        public void Serialize(out byte[] buffer)
        {
            int n = 0;
            Serialize(out n);
            buffer = new byte[n];
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SerializationException();
            }
        }
    }
}
