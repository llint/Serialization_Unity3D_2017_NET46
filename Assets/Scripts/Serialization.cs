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
    public static class SerializationCodeGenerator
    {
        static readonly Type[] types4CodeGen = new Type[] {
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

        static readonly Type[] fundamentalTypes = new Type[] {
            // types that are the same as types4CodeGen
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

            // below are the types that I hand-coded
            typeof(Byte),
            typeof(SByte),
            typeof(String),
        };

        static readonly Type[] serializableTypes = new Type[0];

        static SerializationCodeGenerator()
        {
            serializableTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0)
                .ToArray();
        }

        static string GetStringRep(Type t, bool processDeclaringType = true)
        {
            if (t.IsArray)
            {
                return GetStringRep(t.GetElementType()) + "[]";
            }
            if (t.IsGenericType)
            {
                var genericArgs = t.GetGenericArguments().ToList();

                return GetStringRep(t, genericArgs);
            }
            if (processDeclaringType && t.DeclaringType != null)
            {
                return GetStringRep(t.DeclaringType) + "." + GetStringRep(t, false);
            }

            return t.Name;
        }

        static string GetStringRep(Type t, List<Type> availableArguments)
        {
            if (t.IsGenericType)
            {
                string value = t.Name;
                if (value.IndexOf("`") > -1)
                {
                    value = value.Substring(0, value.IndexOf("`"));
                }

                // Build the type arguments (if any)
                string argString = "";
                var thisTypeArgs = t.GetGenericArguments();
                for (int i = 0; i < thisTypeArgs.Length && availableArguments.Count > 0; i++)
                {
                    if (i != 0) argString += ", ";

                    argString += GetStringRep(availableArguments[0]);
                    availableArguments.RemoveAt(0);
                }

                // If there are type arguments, add them with < >
                if (argString.Length > 0)
                {
                    value += "<" + argString + ">";
                }

                return value;
            }

            return t.Name;
        }

        static void GeneratePartialSerializationOutputClass(CodeGen.CodeBlock bodyNameSpace)
        {
            var bodySerializationOutputClass =
                bodyNameSpace.AddLine("partial class SerializationOutput").AddBlock();

            foreach (var type in types4CodeGen)
            {
                var bodySerializeMethod = bodySerializationOutputClass
                    .AddLine($"public SerializationOutput Serialize({type.Name} value)")
                    .AddBlock();
                bodySerializeMethod.AddLine("var buffer = BitConverter.GetBytes(value);");
                bodySerializeMethod.AddLine("stream.Write(buffer, 0, buffer.Length);");
                bodySerializeMethod.AddLine("return this;");
            }

            // For all the types that are marked with "[Serializable]", add the "Serialize" method
            foreach (var type in serializableTypes)
            {
                var bodySerializeMethod = bodySerializationOutputClass
                    .AddLine($"public SerializationOutput Serialize({GetStringRep(type)} value)")
                    .AddBlock();
                bodySerializeMethod
                    .AddLine($"return SerializationHelper<{GetStringRep(type)}>.Serialize(this, value);");
            }
        }

        static void GeneratePartialSerializationInputClass(CodeGen.CodeBlock bodyNameSpace)
        {
            var bodySerializationInputClass =
                bodyNameSpace.AddLine("partial class SerializationInput").AddBlock();

            foreach (var type in types4CodeGen)
            {
                var bodyDeserializeMethod = bodySerializationInputClass
                    .AddLine($"public SerializationInput Deserialize(out {type.Name} value)")
                    .AddBlock();
                bodyDeserializeMethod.AddLine($"var buffer = BitConverter.GetBytes(default({type.Name}));");
                var bodyIf = bodyDeserializeMethod
                    .AddLine("if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)")
                    .AddBlock();
                bodyIf.AddLine($"value = BitConverter.To{type.Name}(buffer, 0);");
                bodyIf.AddLine("return this;");
                bodyDeserializeMethod.AddLine("throw new SerializationException();");
            }

            // For all the types that are marked with "[Serializable]", add the "Deserialize" method
            foreach (var type in serializableTypes)
            {
                var bodyDeserializeMethod = bodySerializationInputClass
                    .AddLine($"public SerializationInput Deserialize(out {GetStringRep(type)} value)")
                    .AddBlock();
                bodyDeserializeMethod
                    .AddLine($"return SerializationHelper<{GetStringRep(type)}>.Deserialize(this, out value);");
            }
        }

        static void GenerateTypeSerializationMethodMapping(CodeGen.CodeBlock bodyNameSpace)
        {
            var bodyTypeSerializationMethodMappingClass =
                bodyNameSpace.AddLine("static partial class TypeSerializationMethodMapping")
                .AddBlock();

            var bodyInitializeMapping =
                bodyTypeSerializationMethodMappingClass.AddLine("static partial void InitializeMapping()")
                .AddBlock();

            var bodyTypeSerializeMethodMapping =
                bodyInitializeMapping.AddLine("TypeSerializeMethodMapping = new Dictionary<Type, MethodInfo>")
                .AddBlock()
                .WithSemicolon();
            foreach (var type in fundamentalTypes)
            {
                bodyTypeSerializeMethodMapping
                    .AddLine($"{{ typeof({type.Name}), typeof(SerializationOutput).GetMethod(\"Serialize\", new[]{{typeof({type.Name})}}) }},");
            }
            foreach (var type in serializableTypes)
            {
                bodyTypeSerializeMethodMapping
                    .AddLine($"{{ typeof({GetStringRep(type)}), typeof(SerializationOutput).GetMethod(\"Serialize\", new[]{{typeof({GetStringRep(type)})}}) }},");
            }

            var bodyTypeDeserializeMethodMapping =
                bodyInitializeMapping.AddLine("TypeDeserializeMethodMapping = new Dictionary<Type, MethodInfo>")
                .AddBlock()
                .WithSemicolon();
            foreach (var type in fundamentalTypes)
            {
                bodyTypeDeserializeMethodMapping
                    .AddLine($"{{ typeof({type.Name}), typeof(SerializationInput).GetMethod(\"Deserialize\", new[]{{typeof({type.Name}).MakeByRefType()}}) }},");
            }
            foreach (var type in serializableTypes)
            {
                bodyTypeDeserializeMethodMapping
                    .AddLine($"{{ typeof({GetStringRep(type)}), typeof(SerializationInput).GetMethod(\"Deserialize\", new[]{{typeof({GetStringRep(type)}).MakeByRefType()}}) }},");
            }
        }

        static void GenerateGlobalInitializationImpl(CodeGen.CodeBlock bodyNameSpace)
        {
            var bodySerializationClass = bodyNameSpace
                .AddLine("static partial class Serialization")
                .AddBlock();
            var bodyInitializeImplMethod = bodySerializationClass
                .AddLine("static partial void InitializeImpl()")
                .AddBlock();
            foreach (var type in serializableTypes)
            {
                bodyInitializeImplMethod.AddLine($"SerializationHelper<{GetStringRep(type)}>.CreateDelegates();");
            }
        }

        public static void GenerateCode(string file)
        {
            var doc = new CodeGen.CodeGroup();

            doc.AddLine("using System;");
            doc.AddLine("using System.Linq;");
            doc.AddLine("using System.Reflection;");
            doc.AddLine("using System.Collections.Generic;");
            doc.AddLine();
            doc.AddLine("using UnityEngine;");
            doc.AddLine();
            var bodyNameSpace = doc.AddLine("namespace Serialization").AddBlock();

            GeneratePartialSerializationOutputClass(bodyNameSpace);
            GeneratePartialSerializationInputClass(bodyNameSpace);

            GenerateTypeSerializationMethodMapping(bodyNameSpace);

            GenerateGlobalInitializationImpl(bodyNameSpace);

            File.WriteAllText(file, doc.Content);
        }
    }

    static partial class TypeSerializationMethodMapping
    {
        internal static Dictionary<Type, MethodInfo> TypeSerializeMethodMapping
            = new Dictionary<Type, MethodInfo>();
        internal static Dictionary<Type, MethodInfo> TypeDeserializeMethodMapping
            = new Dictionary<Type, MethodInfo>();

        static TypeSerializationMethodMapping()
        {
            InitializeMapping();
        }

        static partial void InitializeMapping();
    }

    static partial class Serialization
    {
        internal static void Initialize()
        {
            InitializeImpl();
        }

        static partial void InitializeImpl();
    }

    static class SerializationHelper<T>
    {
        internal delegate SerializationOutput Delegate_Serialize(SerializationOutput so, T o);
        internal delegate SerializationInput Delegate_Deserialize(SerializationInput si, out T o);

        internal static Delegate_Serialize Serialize { get; private set; }
        internal static Delegate_Deserialize Deserialize { get; private set; }

        internal static void CreateDelegates()
        {
            CreateDelegate_Serialize();
            CreateDelegate_Deserialize();
        }

        // (so, o) => so.Serialize(o.i).Serialize(o.s).Serialize(o.a);
        static void CreateDelegate_Serialize()
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
        static void CreateDelegate_Deserialize()
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

        public SerializationOutput Serialize(String s)
        {
            var buffer = Encoding.UTF8.GetBytes(s);
            Serialize((Int16)buffer.Length);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        public SerializationOutput Serialize(byte[] buffer)
        {
            var n = buffer.Length;
            Serialize(n);
            stream.Write(buffer, 0, buffer.Length);
            return this;
        }

        // NB: for [Serializable] struct types, Serialize would do a struct value copy
        // While we can get around the copying by attaching a "ref" keyword when serializing a struct,
        // I don't really feel the extra complexity is worthwhile - remember, structs are meant to be
        // small and fast copying - complex data types should be using 'class'
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

        public SerializationInput Deserialize(out byte[] buffer)
        {
            int n = 0;
            Deserialize(out n);
            buffer = new byte[n];
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SerializationException();
            }
            return this;
        }
    }
}
