using System.IO;

using UnityEngine;
using UnityEditor;

using Serialization;

public class MenuItems
{
    [MenuItem("Tools/Serialization CodeGen")]
    private static void NewMenuOption()
    {
        SerializationCodeGenerator
            .GenerateCode(Path.Combine(Application.dataPath, "Scripts/Serialization_Generated.cs"));
    }
}
