using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Serialization
{
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
}
