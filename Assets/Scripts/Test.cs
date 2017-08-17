using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Serialization;

[Serializable]
class Base
{
    public int i = 0;
    public string s = "";
}

public class Test : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        Debug.Log("Hello Unity 2017.1");

        SerializationCodeGenerator
            .GenerateCode(Path.Combine(Application.dataPath, "Scripts/Serialization_Generated.txt"));

        // NB: only the runtime code should invoke this
        Serialization.Serialization.Initialize();

        SerializationOutput so = new SerializationOutput();
        Base o = new Base { i = 42, s = "hello" };
        so.Serialize(o);

        SerializationInput si = new SerializationInput(so.GetStream());
        Base o2;
        si.Deserialize(out o2);
        Debug.Log($"o2.i={o2.i}, o2.s={o2.s}");
    }

    // Update is called once per frame
    void Update()
    {
    }
}
