using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class Dbg : MonoBehaviour
{
	public string LogFile = "logyo.txt";

	public bool EchoToConsole = true;

	public bool AddTimeStamp = true;

	private StreamWriter OutputStream;

	private static Dbg Singleton;

	public static Dbg Instance
	{
		get
		{
			return Dbg.Singleton;
		}
	}

	static Dbg()
	{
	}

	public Dbg()
	{
	}

	private void Awake()
	{
		if (Dbg.Singleton != null)
		{
			UnityEngine.Debug.LogError("Multiple Dbg Singletons exist!");
			return;
		}
		Dbg.Singleton = this;
		this.OutputStream = new StreamWriter(this.LogFile, true);
	}

	private void OnApplicationQuit()
	{
	}

	private void OnDestroy()
	{
		if (this.OutputStream != null)
		{
			this.OutputStream.Close();
			this.OutputStream = null;
		}
		UnityEngine.Debug.Log("stopping");
	}

	[Conditional("DEBUG")]
	[Conditional("PROFILE")]
	public static void Trace(string Message)
	{
		if (Dbg.Instance == null)
		{
			UnityEngine.Debug.Log(Message);
			return;
		}
		Dbg.Instance.Write(Message);
	}

	private void Write(string message)
	{
		if (this.OutputStream != null)
		{
			this.OutputStream.WriteLine(message);
			this.OutputStream.Flush();
		}
		if (this.EchoToConsole)
		{
			UnityEngine.Debug.Log(message);
		}
	}
}