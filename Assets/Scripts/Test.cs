using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Serialization;

[Serializable]
public class Base
{
    public int i = 0;
    public string s = "";

	public Struct[][] v;

	public override string ToString()
	{
		return $"Base: i={i}, s={s}, v={v}";
	}
}

[Serializable]
public struct Struct
{
	public int i;
	public string s;

	public override string ToString()
	{
		return $"Struct: i={i}, s={s}";
	}
}

public class Test : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        Debug.Log("Hello Unity 2017.1");

        // NB: only the runtime code should invoke this
        Serialization.Serialization.Initialize();

        SerializationOutput so = new SerializationOutput();
        Base o = new Base { i = 42, s = "hello", v = new Struct[][] { new Struct[] { new Struct { i = 888, s = "xxx" } } } };
        so.Serialize(o);

        SerializationInput si = new SerializationInput(so.GetStream());
        Base o2;
        si.Deserialize(out o2);

        Debug.Log($"o2: {o2}");
    }

    // Update is called once per frame
    void Update()
    {
    }
}
