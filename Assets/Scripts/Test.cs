using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Serialization;

class Base
{
    public int i = 0;
    public string s = "";
}

public class Test : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		Debug.Log("Hello Unity 2017.1");

		TypeSerializationMethodMapping.Init();

		// the initialization code needs to be generated
		SerializationHelper<Base>.CreateDelegate_Serialize();
		SerializationHelper<Base>.CreateDelegate_Deserialize();

		SerializationOutput so = new SerializationOutput();
		Base o = new Base {i = 42, s = "hello"};
		so.Serialize(o);

		SerializationInput si = new SerializationInput(so.GetStream());
		Base o2;
		si.Deserialize(out o2);
		Debug.Log($"o2.i={o2.i}, o2.s={o2.s}");
	}

	// Update is called once per frame
	void Update ()
	{
	}
}
