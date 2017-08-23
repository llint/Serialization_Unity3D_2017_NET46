using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MyEnum { A, B, C };

static class Helpers
{
    internal static string ToString<T>(T t)
    {
        return t.ToString();
    }

    internal static string ToString<T>(T[] ta)
    {
        return $"[{string.Join(",", ta.Select(t => ToString(t)))}]";
    }

    internal static string ToString<T>(T[][] taa)
    {
        return $"[{string.Join(",", taa.Select(ta => ToString(ta)))}]";
    }

    internal static string ToString<T>(List<T>[] lta)
    {
        return $"({string.Join(",", lta.Select(lt => ToString(lt)))})";
    }

    internal static string ToString<T>(List<T> lt)
    {
        return $"({string.Join(",", lt.Select(t => ToString(t)))})";
    }

    internal static string ToString<T>(List<T[]> lt)
    {
        return $"({string.Join(",", lt.Select(ta => ToString(ta)))})";
    }
}

[Serializable]
public class Base
{
    // permutations (ps is null):
    // - [SerializeField] with 'private' - thows null reference exception
    // - [NonSerialized] with 'public' - not touched, no exception
    // - 'public' - throws null reference exception
    // [SerializeField]
    [NonSerialized]
    public string ps;

    public int i = 0;

    public List<string[]> ls = new List<string[]>();

    public byte[] buffer = new byte[0];

	public Struct[][] taa = new Struct[0][];

    public string[][] saa = new string[0][];

    public override string ToString()
	{
		return $"Base: i={i}, ls={Helpers.ToString(ls)}, buffer={Helpers.ToString(buffer)}, taa={Helpers.ToString(taa)}, saa={Helpers.ToString(saa)}";
	}
}

[Serializable]
public class Derived : Base
{
    public string s = "";

    public override string ToString()
    {
        return $"Derived: s={s} +++ " + base.ToString();
    }
}

[Serializable]
public struct Struct
{
    public MyEnum[] ea;

    public Base b;

    public int i;
	public string s;

	public override string ToString()
	{
		return $"Struct: b={b}, ea={Helpers.ToString(ea)}, i={i}, s={s}";
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
        Legacy.SerializableTypeRegistry.Initialize();

        Base o = new Base {
            i = 42,
            ls = new List<string[]> { new string[] { "hello", "world" } },
            buffer = new byte[] {42, 42, 42, 42},
            saa = new string[][] {
                new string[] {"X", "Y", "Z"},
                new string[] {"T", "U"},
            },
            taa = new Struct[][] {
                new Struct[] {
                    new Struct {
                        b = new Derived {s = "Derived"},
                        i = 888,
                        s = "xxx",
                        ea = new MyEnum[] {MyEnum.A, MyEnum.B, MyEnum.C},
                    }
                },
                new Struct[] {
                    new Struct {
                        b = new Base(),
                        i = 666,
                        s = "yyyy",
                        ea = new MyEnum[] {MyEnum.C, MyEnum.B, MyEnum.A},
                    }
                },
            }
        };

        const int COUNTER = 1000;

        var stopWatch = new System.Diagnostics.Stopwatch();

        {
            Serialization.SerializationOutput so = new Serialization.SerializationOutput();

            // stopWatch.Reset();
            stopWatch.Restart();
            for (int i = 0; i < COUNTER; ++i)
            {
                so.Serialize(o);
            }
            stopWatch.Stop();
            Debug.Log($"Serialize: {stopWatch.ElapsedMilliseconds}");

            Serialization.SerializationInput si = new Serialization.SerializationInput(so.GetStream());

            // stopWatch.Reset();
            stopWatch.Restart();
            for (int i = 0; i < COUNTER; ++i)
            {
                Base o2;
                si.Deserialize(out o2);
            }
            stopWatch.Stop();
            Debug.Log($"Deserialize: {stopWatch.ElapsedMilliseconds}");

            // Debug.Log($"o2: {o2}");
        }

        {
            Legacy.SerializationOutput so = new Legacy.SerializationOutput();

            // stopWatch.Reset();
            stopWatch.Restart();
            for (int i = 0; i < COUNTER; ++i)
            {
                so.Serialize(o);
            }
            stopWatch.Stop();
            Debug.Log($"Legacy.Serialize: {stopWatch.ElapsedMilliseconds}");

            Legacy.SerializationInput si = new Legacy.SerializationInput(so.GetStream());

            // stopWatch.Reset();
            stopWatch.Restart();
            for (int i = 0; i < COUNTER; ++i)
            {
                Base o2 = (Base)si.Serialize(typeof(Base));
            }
            stopWatch.Stop();
            Debug.Log($"Legacy.Deserialize: {stopWatch.ElapsedMilliseconds}");

            // Debug.Log($"o2: {o2}");
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
