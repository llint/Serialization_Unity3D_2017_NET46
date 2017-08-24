using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
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
            typeof(byte[]),
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
                bodyNameSpace.AddLine("public partial class SerializationOutput").AddBlock();

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
                bodyNameSpace.AddLine("public partial class SerializationInput").AddBlock();

            foreach (var type in types4CodeGen)
            {
                var bodyDeserializeMethod = bodySerializationInputClass
                    .AddLine($"public SerializationInput Deserialize(out {type.Name} value)")
                    .AddBlock();
                bodyDeserializeMethod.AddLine("position = stream.Position;");
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
                bodyDeserializeMethod.AddLine("position = stream.Position;");
                bodyDeserializeMethod
                    .AddLine($"return SerializationHelper<{GetStringRep(type)}>.Deserialize(this, out value);");
            }
        }

        static void GenerateTypeSerializationMethodMapping(CodeGen.CodeBlock bodyNameSpace)
        {
            var bodyTypeSerializationMethodMappingClass =
                bodyNameSpace.AddLine("public static partial class TypeSerializationMethodMapping")
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

        static void GenerateSerializableTypesRegistry(CodeGen.CodeBlock bodyNameSpace)
        {
            var bodySerializableTypesRegistry = bodyNameSpace
                .AddLine("public static partial class SerializableTypesRegistry")
                .AddBlock();
            var bodyInitializeMethod = bodySerializableTypesRegistry
                .AddLine("static partial void Initialize()")
                .AddBlock();
            var bodySerializableTypes = bodyInitializeMethod
                .AddLine("serializableTypes = new Type[]")
                .AddBlock()
                .WithSemicolon();
            var bodyTypeIndexMapping = bodyInitializeMethod
                .AddLine("typeIndexMapping = new Dictionary<Type, int>")
                .AddBlock()
                .WithSemicolon();
            var bodyTypeIndexedSerializeDelegates = bodyInitializeMethod
                .AddLine("typeIndexedSerializeDelegates = new Serialize[]")
                .AddBlock()
                .WithSemicolon();
            var bodyTypeIndexedDeserializeDelegates = bodyInitializeMethod
                .AddLine("typeIndexedDeserializeDelegates = new Deserialize[]")
                .AddBlock()
                .WithSemicolon();

            int idx = 0;
            foreach (var type in serializableTypes)
            {
                bodySerializableTypes.AddLine($"typeof({GetStringRep(type)}),");
                bodyTypeIndexMapping.AddLine($"{{ typeof({GetStringRep(type)}), {idx} }},");
                bodyTypeIndexedSerializeDelegates.AddLine($"(SerializationOutput so, object o) => so.Serialize(({GetStringRep(type)})o),");
                bodyTypeIndexedDeserializeDelegates.AddLine($"(SerializationInput si, out object o) => {{ {GetStringRep(type)} x; si.Deserialize(out x); o = x; return si; }},");

                ++idx;
            }
        }

        static void GenerateGlobalInitializationImpl(CodeGen.CodeBlock bodyNameSpace)
        {
            var bodySerializationClass = bodyNameSpace
                .AddLine("public static partial class Serialization")
                .AddBlock();

            var bodyInitializeImplMethod = bodySerializationClass
                .AddLine("static partial void InitializeImpl(Module module)")
                .AddBlock();
            foreach (var type in serializableTypes)
            {
                bodyInitializeImplMethod.AddLine($"SerializationHelper<{GetStringRep(type)}>.CreateDelegates(module);");
            }

            var bodyCreateAssemblyImplMethod = bodySerializationClass
                .AddLine("static partial void CreateAssemblyImpl(ModuleBuilder moduleBuilder)")
                .AddBlock();
            foreach (var type in serializableTypes)
            {
                bodyCreateAssemblyImplMethod.AddLine($"SerializationHelper<{GetStringRep(type)}>.CreateAssembly(moduleBuilder);");
            }
        }

        public static void GenerateCode(string file)
        {
            var doc = new CodeGen.CodeGroup();

            doc.AddLine("using System;");
            doc.AddLine("using System.Linq;");
            doc.AddLine("using System.Reflection;");
            doc.AddLine("using System.Reflection.Emit;");
            doc.AddLine("using System.Collections.Generic;");
            doc.AddLine();
            doc.AddLine("using UnityEngine;");
            doc.AddLine();
            var bodyNameSpace = doc.AddLine("namespace Serialization").AddBlock();

            GeneratePartialSerializationOutputClass(bodyNameSpace);
            GeneratePartialSerializationInputClass(bodyNameSpace);

            GenerateTypeSerializationMethodMapping(bodyNameSpace);
            GenerateSerializableTypesRegistry(bodyNameSpace);

            GenerateGlobalInitializationImpl(bodyNameSpace);

            File.WriteAllText(file, doc.Content);
        }
    }

    public static partial class TypeSerializationMethodMapping
    {
        public static Dictionary<Type, MethodInfo> TypeSerializeMethodMapping
            = new Dictionary<Type, MethodInfo>();
        public static Dictionary<Type, MethodInfo> TypeDeserializeMethodMapping
            = new Dictionary<Type, MethodInfo>();

        static TypeSerializationMethodMapping()
        {
            InitializeMapping();
        }

        static partial void InitializeMapping();
    }

    // All the Serializable types, including value types, not just reference types
    // We need to be able to instantiate a struct type for an interface reference!
    public static partial class SerializableTypesRegistry
    {
        static Type[] serializableTypes = new Type[0];
        static Dictionary<Type, int> typeIndexMapping = new Dictionary<Type, int>();

        public delegate SerializationOutput Serialize(SerializationOutput so, object o);
        public delegate SerializationInput Deserialize(SerializationInput si, out object o);

        static Serialize[] typeIndexedSerializeDelegates = new Serialize[0];
        static Deserialize[] typeIndexedDeserializeDelegates = new Deserialize[0];

        static SerializableTypesRegistry()
        {
            Initialize();
        }

        static partial void Initialize();

        public static Type GetIndexedType(int typeIndex)
        {
            return serializableTypes[typeIndex];
        }

        public static int GetTypeIndex(Type type)
        {
            return typeIndexMapping[type];
        }

        public static Serialize GetSerializeDelegate(int typeIndex)
        {
            return typeIndexedSerializeDelegates[typeIndex];
        }

        public static Deserialize GetDeserializeDelegate(int typeIndex)
        {
            return typeIndexedDeserializeDelegates[typeIndex];
        }
    }

    public static partial class Serialization
    {
        public static void Initialize()
        {
            var assembly = Assembly.LoadFrom(Path.Combine(Application.dataPath, "Assemblies/Serialization.dll"));
            var module = assembly.GetModule("Serialization");
            InitializeImpl(module);
        }

        static partial void InitializeImpl(Module module);

        public static void CreateAssembly(string dir)
        {
            var assemblyName = new AssemblyName("Serialization");
            var assemblyBuilder =
                Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave, dir);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Serialization", "Serialization.dll");
            CreateAssemblyImpl(moduleBuilder);
            assemblyBuilder.Save("Serialization.dll");
        }

        static partial void CreateAssemblyImpl(ModuleBuilder moduleBuilder);
    }

    public static class SerializationHelper<T>
    {
        public delegate SerializationOutput Delegate_Serialize(SerializationOutput so, T o);
        public delegate SerializationInput Delegate_Deserialize(SerializationInput si, out T o);

        public static Delegate_Serialize Serialize { get; private set; }
        public static Delegate_Deserialize Deserialize { get; private set; }

        public static void CreateDelegates(Module module)
        {
            var type = module.GetType($"SerializationHelper_{typeof(T).Name}");

            var miSerialize = type.GetMethod("Serialize", new[] { typeof(SerializationOutput), typeof(T) });
            Serialize = (Delegate_Serialize)Delegate.CreateDelegate(typeof(Delegate_Serialize), miSerialize);

            var miDeserialize = type.GetMethod("Deserialize", new[] { typeof(SerializationInput), typeof(T).MakeByRefType() });
            Deserialize = (Delegate_Deserialize)Delegate.CreateDelegate(typeof(Delegate_Deserialize), miDeserialize);
        }

        public static void CreateAssembly(ModuleBuilder moduleBuilder)
        {
            var typeBuilder = moduleBuilder.DefineType($"SerializationHelper_{typeof(T).Name}", // TODO: GetStringRep(type)
                TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract);

            var serializeMethodBuilder = typeBuilder.DefineMethod(
                "Serialize",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(SerializationOutput),
                new[] { typeof(SerializationOutput), typeof(T) });
            SerializeDelegateCreationHelper.CreateAssembly(serializeMethodBuilder);

            var deserializeMethodBuilder = typeBuilder.DefineMethod(
                "Deserialize",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(SerializationInput),
                new[] { typeof(SerializationInput), typeof(T).MakeByRefType() });
            DeserializeDelegateCreationHelper.CreateAssembly(deserializeMethodBuilder);

            typeBuilder.CreateType();
        }

        static class SerializeDelegateCreationHelper
        {
            static Expression GenerateSerializeValueExpression(Expression so, Expression value)
            {
                Expression serializeExpression = null;
                MethodInfo mi;
                if (TypeSerializationMethodMapping.TypeSerializeMethodMapping.TryGetValue(value.Type, out mi))
                {
                    serializeExpression = Expression.Call(so, mi, value);
                }
                else if (value.Type.IsArray)
                {
                    serializeExpression = GenereateSerializeArrayExpression(so, value);
                }
                else if (value.Type.IsGenericType && value.Type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    serializeExpression = GenerateSerializeListExpression(so, value);
                }
                else if (value.Type.IsEnum)
                {
                    serializeExpression = GenerateSerializeEnumExpression(so, value);
                }
                else
                {
                    throw new SerializationException();
                }
                return serializeExpression;
            }

            static Expression GenerateSerializeEnumExpression(Expression so, Expression e)
            {
                var mi = TypeSerializationMethodMapping.TypeSerializeMethodMapping[typeof(int)];
                return Expression.Call(so, mi, Expression.Convert(e, typeof(int)));
            }

            static Expression GenerateSerializeListExpression(Expression so, Expression l)
            {
                List<Expression> blockSerializeListExpressions = new List<Expression>();

                var countExpression = Expression.Property(l, "Count");

                // so.Serialize(l.Count)
                var mi = TypeSerializationMethodMapping.TypeSerializeMethodMapping[typeof(int)];
                var serializeListCountExpression = Expression.Call(so, mi, countExpression);
                Debug.Log(serializeListCountExpression.ToString());
                blockSerializeListExpressions.Add(serializeListCountExpression);

                // Loop and serialize each element
                ParameterExpression index = Expression.Variable(typeof(int), "i");
                LabelTarget label = Expression.Label();

                BlockExpression loopBlock = Expression.Block(
                    new[] { index },
                    Expression.Assign(index, Expression.Constant(0)),
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.LessThan(index, countExpression),
                            Expression.Block(
                                GenerateSerializeValueExpression(so, Expression.Property(l, "Item", index)),
                                Expression.PreIncrementAssign(index)
                            ),
                            Expression.Break(label)
                        ),
                        label
                    )
                );
                blockSerializeListExpressions.Add(loopBlock);

                return Expression.Block(blockSerializeListExpressions);
            }

            static Expression GenereateSerializeArrayExpression(Expression so, Expression a)
            {
                var blockSerializeArrayExpressions = new List<Expression>();

                var lengthExpression = Expression.ArrayLength(a);

                // so.Serialize(a.Length)
                var mi = TypeSerializationMethodMapping.TypeSerializeMethodMapping[typeof(int)];
                var serializeArrayLengthExpression = Expression.Call(so, mi, lengthExpression);
                Debug.Log(serializeArrayLengthExpression.ToString());
                blockSerializeArrayExpressions.Add(serializeArrayLengthExpression);

                // Loop and serialize each element
                ParameterExpression index = Expression.Parameter(typeof(int), "i");
                LabelTarget label = Expression.Label();

                BlockExpression loopBlock = Expression.Block(
                    new[] { index },
                    Expression.Assign(index, Expression.Constant(0)),
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.LessThan(index, lengthExpression),
                            Expression.Block(
                                GenerateSerializeValueExpression(so, Expression.ArrayIndex(a, index)),
                                Expression.PreIncrementAssign(index)
                            ),
                            Expression.Break(label)
                        ),
                        label
                    )
                );
                blockSerializeArrayExpressions.Add(loopBlock);

                return Expression.Block(blockSerializeArrayExpressions);
            }

            // (so, o) => {so.Serialize(o.i);so.Serialize(o.s);so.Serialize(o.a);return so;}
            public static void CreateAssembly(MethodBuilder methodBuilder)
            {
                var type = typeof(T);
                ParameterExpression so = Expression.Parameter(typeof(SerializationOutput), "so");
                ParameterExpression o = Expression.Parameter(type, "o");

                var blockVariables = new List<ParameterExpression>();
                var blockExpressions = new List<Expression>();

                LabelTarget labelTarget = Expression.Label(typeof(SerializationOutput));
                LabelExpression labelExpression = Expression.Label(labelTarget, so);

                if (!type.IsValueType)
                {
                    /*
                        int typeIndex;
                        typeIndex = SerializableTypesRegistry.GetTypeIndex(o.GetType());
                        if (o.GetType() != typeof(T))
                        {
                            // 'o' is of a derived type of T, dispatch the serialization to it
                            return SerializableTypesRegistry.GetSerializeDelegate(typeIndex)(so, o);
                        }
                        so.Serialize(typeIndex);
                        ... serialize fields of T ...
                        return so;
                     */
                    var exprGetType = Expression.Call(o, typeof(object).GetMethod("GetType"));
                    var exprGetTypeIndex =
                        Expression.Call(
                            typeof(SerializableTypesRegistry).GetMethod("GetTypeIndex", new[] { typeof(Type) }),
                            exprGetType);
                    var typeIndex = Expression.Variable(typeof(int), "typeIndex");
                    blockVariables.Add(typeIndex);
                    var assignTypeIndex = Expression.Assign(typeIndex, exprGetTypeIndex);
                    blockExpressions.Add(assignTypeIndex);
                    var miGetSerializeDelegate =
                        typeof(SerializableTypesRegistry).GetMethod("GetSerializeDelegate", new[]{typeof(int)});
                    var returnConcreteSerializeExpression =
                        Expression.Return(labelTarget,
                            Expression.Invoke(Expression.Call(miGetSerializeDelegate, typeIndex), so, o),
                            typeof(SerializationOutput));
                    var conditionalConcreteSerialize = Expression.IfThen(
                        Expression.NotEqual(exprGetType, Expression.Constant(type)),
                        returnConcreteSerializeExpression);
                    Debug.Log(conditionalConcreteSerialize);
                    blockExpressions.Add(conditionalConcreteSerialize);
                    var mi = TypeSerializationMethodMapping.TypeSerializeMethodMapping[typeof(int)];
                    var serializeTypeIndexExpression = Expression.Call(so, mi, typeIndex);
                    Debug.Log(serializeTypeIndexExpression.ToString());
                    blockExpressions.Add(serializeTypeIndexExpression);
                }

                foreach (var fi in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (Attribute.GetCustomAttributes(fi, typeof(NonSerializedAttribute)).Any())
                    {
                        continue;
                    }

                    if (!fi.IsPublic && !Attribute.GetCustomAttributes(fi, typeof(UnityEngine.SerializeField)).Any())
                    {
                        continue;
                    }

                    MemberExpression memberExpression = Expression.Field(o, fi);

                    var serializeExpression = GenerateSerializeValueExpression(so, memberExpression);
                    Debug.Log(serializeExpression.ToString());
                    blockExpressions.Add(serializeExpression);
                }

                GotoExpression returnExpression = Expression.Return(labelTarget, so, typeof(SerializationOutput));

                blockExpressions.Add(returnExpression);
                blockExpressions.Add(labelExpression);

                var lambda = Expression.Lambda<Delegate_Serialize>(
                    Expression.Block(typeof(SerializationOutput), blockVariables, blockExpressions), so, o);
                Debug.Log(lambda.ToString());

                lambda.CompileToMethod(methodBuilder);
            }
        }

        static class DeserializeDelegateCreationHelper
        {
            static Expression GenerateDeserializeValueExpression(Expression si, Expression value)
            {
                Expression deserializeExpression = null;
                MethodInfo mi;
                if (TypeSerializationMethodMapping.TypeDeserializeMethodMapping.TryGetValue(value.Type, out mi))
                {
                    deserializeExpression = Expression.Call(si, mi, value);
                }
                else if (value.Type.IsArray)
                {
                    deserializeExpression = GenerateDeserializeArrayExpression(si, value);
                }
                else if (value.Type.IsGenericType && value.Type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    deserializeExpression = GenerateDeserializeListExpression(si, value);
                }
                else if (value.Type.IsEnum)
                {
                    deserializeExpression = GenerateDeserializeEnumExpression(si, value);
                }
                else
                {
                    throw new SerializationException();
                }
                return deserializeExpression;
            }

            static Expression GenerateDeserializeEnumExpression(Expression si, Expression e)
            {
                ParameterExpression i = Expression.Variable(typeof(int), "i");

                var mi = TypeSerializationMethodMapping.TypeDeserializeMethodMapping[typeof(int)];
                MethodCallExpression deserializeExpression = Expression.Call(si, mi, i);

                var assignExpression = Expression.Assign(e, Expression.Convert(i, e.Type));

                return Expression.Block(
                    new[]{i},
                    deserializeExpression,
                    assignExpression
                );
            }

            static Expression GenerateDeserializeListExpression(Expression si, Expression l)
            {
                List<Expression> blockDeserializeListExpressions = new List<Expression>();

                ParameterExpression countExpression = Expression.Variable(typeof(int), "count");

                var mi = TypeSerializationMethodMapping.TypeDeserializeMethodMapping[typeof(int)];
                var deserializeListCountExpression = Expression.Call(si, mi, countExpression);
                Debug.Log(deserializeListCountExpression.ToString());
                blockDeserializeListExpressions.Add(deserializeListCountExpression);

                var instantiateExpression =
                    Expression.Assign(l, Expression.New(l.Type));
                Debug.Log(instantiateExpression.ToString());
                blockDeserializeListExpressions.Add(instantiateExpression);

                // Loop and serialize each element
                ParameterExpression index = Expression.Variable(typeof(int), "i");
                ParameterExpression x = Expression.Variable(l.Type.GetGenericArguments()[0], "x");
                LabelTarget label = Expression.Label();

                BlockExpression loopBlock = Expression.Block(
                    new[] { index },
                    Expression.Assign(index, Expression.Constant(0)),
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.LessThan(index, countExpression),
                            Expression.Block(
                                new[] {x},
                                GenerateDeserializeValueExpression(si, x),
                                Expression.Call(l, l.Type.GetMethod("Add"), x),
                                Expression.PreIncrementAssign(index)
                            ),
                            Expression.Break(label)
                        ),
                        label
                    )
                );
                blockDeserializeListExpressions.Add(loopBlock);

                return Expression.Block(new[] { countExpression }, blockDeserializeListExpressions);
            }

            static Expression GenerateDeserializeArrayExpression(Expression si, Expression a)
            {
                var blockDeserializeArrayExpressions = new List<Expression>();

                ParameterExpression lengthExpression = Expression.Variable(typeof(int), "length");

                var mi = TypeSerializationMethodMapping.TypeDeserializeMethodMapping[typeof(int)];
                var deserializeArrayLengthExpression = Expression.Call(si, mi, lengthExpression);
                Debug.Log(deserializeArrayLengthExpression.ToString());
                blockDeserializeArrayExpressions.Add(deserializeArrayLengthExpression);

                var instantiateExpression =
                    Expression.Assign(a, Expression.NewArrayBounds(a.Type.GetElementType(), lengthExpression));
                Debug.Log(instantiateExpression.ToString());
                blockDeserializeArrayExpressions.Add(instantiateExpression);

                // Loop and serialize each element
                ParameterExpression index = Expression.Parameter(typeof(int), "i");
                LabelTarget label = Expression.Label();

                BlockExpression loopBlock = Expression.Block(
                    new[] { index },
                    Expression.Assign(index, Expression.Constant(0)),
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.LessThan(index, lengthExpression),
                            Expression.Block(
                                GenerateDeserializeValueExpression(si, Expression.ArrayAccess(a, index)),
                                Expression.PreIncrementAssign(index)
                            ),
                            Expression.Break(label)
                        ),
                        label
                    )
                );
                blockDeserializeArrayExpressions.Add(loopBlock);

                return Expression.Block(new [] {lengthExpression}, blockDeserializeArrayExpressions);
            }

            // (si, out o) => {o = new Base();si.Deserialize(out o.i);si.Deserialize(out o.s);return si;}
            public static void CreateAssembly(MethodBuilder methodBuilder)
            {
                var type = typeof(T);
                ParameterExpression si = Expression.Parameter(typeof(SerializationInput), "si");
                ParameterExpression o = Expression.Parameter(type.MakeByRefType(), "o");

                var blockVariables = new List<ParameterExpression>();
                var blockExpressions = new List<Expression>();

                LabelTarget labelTarget = Expression.Label(typeof(SerializationInput));
                GotoExpression returnExpression = Expression.Return(labelTarget, si, typeof(SerializationInput));

                if (!type.IsValueType)
                {
                    /*
                        int typeIndex;
                        si.Deserialize(out typeIndex);
                        var concreteType;
                        concreteType = SerializableTypesRegistry.GetIndexedType(typeIndex);
                        if (concreteType != typeof(T))
                        {
                            si.Rewind(); // so the typeIndex can be read again
                            object x;
                            SerializableTypesRegistry.GetDeserializeDelegate(typeIndex)(si, out x);
                            o = (T)x;
                            return si;
                        }

                        o = new T();
                        ... deserialize fields of T ...
                        return si;
                     */
                    var typeIndex = Expression.Variable(typeof(int), "typeIndex");
                    blockVariables.Add(typeIndex);
                    var mi = TypeSerializationMethodMapping.TypeDeserializeMethodMapping[typeof(int)];
                    var deserializeTypeIndex = Expression.Call(si, mi, typeIndex);
                    blockExpressions.Add(deserializeTypeIndex);
                    var miGetIndexedType = typeof(SerializableTypesRegistry).GetMethod("GetIndexedType", new[] {typeof(int)});
                    var concreteType = Expression.Variable(typeof(Type), "concreteType");
                    blockVariables.Add(concreteType);
                    var assignConcreteTypeExpression = Expression.Assign(concreteType, Expression.Call(miGetIndexedType, typeIndex));
                    blockExpressions.Add(assignConcreteTypeExpression);
                    var miRewind = typeof(SerializationInput).GetMethod("Rewind");
                    var x = Expression.Variable(typeof(object), "x");
                    var miGetDeserializeDelegate =
                        typeof(SerializableTypesRegistry).GetMethod("GetDeserializeDelegate", new[] {typeof(int)});
                    var concreteDeserialize =
                        Expression.Invoke(Expression.Call(miGetDeserializeDelegate, typeIndex), si, x);
                    var assign = Expression.Assign(o, Expression.Convert(x, type));
                    var conditionalConcreteDeserialize =
                        Expression.IfThen(
                            Expression.NotEqual(concreteType, Expression.Constant(type)),
                            Expression.Block(
                                new[] {x},
                                Expression.Call(si, miRewind),
                                concreteDeserialize,
                                assign,
                                returnExpression // labelExpression is at the end of the entire block (below)
                            )
                        );
                    Debug.Log(conditionalConcreteDeserialize.ToString());
                    blockExpressions.Add(conditionalConcreteDeserialize);
                }

                BinaryExpression instantiateExpression = Expression.Assign(o, Expression.New(type));
                Debug.Log(instantiateExpression.ToString());
                blockExpressions.Add(instantiateExpression);

                foreach (var fi in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (Attribute.GetCustomAttributes(fi, typeof(NonSerializedAttribute)).Any())
                    {
                        continue;
                    }

                    if (!fi.IsPublic && !Attribute.GetCustomAttributes(fi, typeof(UnityEngine.SerializeField)).Any())
                    {
                        continue;
                    }

                    MemberExpression memberExpression = Expression.Field(o, fi);

                    var deserializeExpression = GenerateDeserializeValueExpression(si, memberExpression);
                    Debug.Log(deserializeExpression.ToString());
                    blockExpressions.Add(deserializeExpression);
                }

                LabelExpression labelExpression = Expression.Label(labelTarget, si);

                blockExpressions.Add(returnExpression);
                blockExpressions.Add(labelExpression);

                var lambda = Expression.Lambda<Delegate_Deserialize>(
                    Expression.Block(typeof(SerializationInput), blockVariables, blockExpressions), si, o);
                Debug.Log(lambda.ToString());

                lambda.CompileToMethod(methodBuilder);
            }
        }
    }

    public class SerializationException : Exception
    {
    }

    public partial class SerializationOutput
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

    public partial class SerializationInput
    {
        MemoryStream stream;
        long position;

        public SerializationInput(MemoryStream stream_)
        {
            stream = stream_;
            stream.Seek(0, SeekOrigin.Begin);
            position = stream.Position;
        }

        public MemoryStream GetStream()
        {
            return stream;
        }

        public void Rewind()
        {
            // stream.Seek(position, SeekOrigin.Begin);
            stream.Position = position;
        }

        public SerializationInput Deserialize(out Byte value)
        {
            position = stream.Position;
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
            position = stream.Position;
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
            position = stream.Position;
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
            position = stream.Position;
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
