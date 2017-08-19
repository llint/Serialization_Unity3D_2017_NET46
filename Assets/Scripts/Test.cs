﻿using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Serialization;

public enum MyEnum { A, B, C };

static class Helpers
{
    internal static string ToString<T>(T[] ta)
    {
        return $"[{string.Join(",", ta.Select(t => t.ToString()))}]";
    }

    internal static string ToString<T>(T[][] taa)
    {
        return $"[{string.Join(",", taa.Select(ta => ToString(ta)))}]";
    }
}

[Serializable]
public class Base
{
    public int i = 0;

	public Struct[][] taa;

    public string[][] saa;

    public override string ToString()
	{
		return $"Base: i={i}, taa={Helpers.ToString(taa)}, saa={Helpers.ToString(saa)}";
	}
}

[Serializable]
public struct Struct
{
    public MyEnum[] ea;

    public int i;
	public string s;

	public override string ToString()
	{
		return $"Struct: ea={Helpers.ToString(ea)}, i={i}, s={s}";
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
        Base o = new Base {
            i = 42,
            saa = new string[][] {
                new string[] {"X", "Y", "Z"},
                new string[] {"T", "U"},
            },
            taa = new Struct[][] {
                new Struct[] {
                    new Struct {
                        i = 888,
                        s = "xxx",
                        ea = new MyEnum[] {MyEnum.A, MyEnum.B, MyEnum.C},
                    }
                },
                new Struct[] {
                    new Struct {
                        i = 666,
                        s = "yyyy",
                        ea = new MyEnum[] {MyEnum.C, MyEnum.B, MyEnum.A},
                    }
                },
            }
        };
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
