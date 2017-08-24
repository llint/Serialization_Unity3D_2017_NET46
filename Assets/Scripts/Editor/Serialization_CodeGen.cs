using System.IO;

using UnityEngine;
using UnityEditor;

using Serialization;

public class MenuItems
{
    [MenuItem("Tools/Serialization CodeGen")]
    private static void GenerateCode()
    {
        SerializationCodeGenerator
            .GenerateCode(Path.Combine(Application.dataPath, "Scripts/Serialization_Generated.cs"));
    }

    [MenuItem("Tools/Serialization CreateAssembly")]
    private static void CreateAssembly()
    {
        Serialization.Serialization.CreateAssembly();
    }
}
