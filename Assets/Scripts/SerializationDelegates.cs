using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

static class TypeSerializationMethodMapping
{
    internal static Dictionary<Type, MethodInfo> TypeSerializeMethodMapping = new Dictionary<Type, MethodInfo>();
	internal static Dictionary<Type, MethodInfo> TypeDeserializeMethodMapping = new Dictionary<Type, MethodInfo>();

    internal static void Init()
    {
        TypeSerializeMethodMapping = new Dictionary<Type, MethodInfo> {
            {typeof(int), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(int)})},
            {typeof(string), typeof(SerializationOutput).GetMethod("Serialize", new[]{typeof(string)})},
        };
        TypeDeserializeMethodMapping = new Dictionary<Type, MethodInfo> {
            {typeof(int), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(int).MakeByRefType()})},
            {typeof(string), typeof(SerializationInput).GetMethod("Deserialize", new[]{typeof(string).MakeByRefType()})},
        };
    }
}

class Base
{
    public int i = 0;
    public string s = "";
}

///////////////////////////////////////////////////////////////////////////////
// Generated

partial class SerializationOutput
{
	// Could this method be a generics?
	public SerializationOutput Serialize(Base o)
	{
		SerializationHelper<Base>.Serialize(this, o);
		return this;
	}
}

partial class SerializationInput
{
    // Could this method be a generics?
    public SerializationInput Deserialize(out Base o)
	{
		SerializationHelper<Base>.Deserialize(this, out o);
		return this;
	}
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

/*
	pattern: for each serializable type, and when a concrete object is in place, there are serialize and deserialize
	for deserialize, the assumption is that the object is created

	then there are the two master Serialize and Deserialize methods, which handle the object type serialization, and
	on the receiving side, the Deserialize method would read the type, and instantiate the concrete object
	(if not a struct) - for simplicity's sake, we can start with all types are either value types, or final class

	So we could always instantiate the objects based on the concrete type! let's start our experiment with this!

	However, string is a special example, but if we explicitly define the way to serialize string, it would be fine
 */

 // create a dotnet core project
 // now that only the higher version of the dotnet expressions work with the "out" parameter,