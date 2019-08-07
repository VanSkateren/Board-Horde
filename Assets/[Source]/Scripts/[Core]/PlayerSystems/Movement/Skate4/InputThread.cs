using Rewired;
using System;
using System.Threading;
using UnityEngine;
using XInputDotNetPure;

public class InputThread : MonoBehaviour
{
	public InputController inputController;

	private bool playerIndexSet;

	private PlayerIndex playerIndex;

	private GamePadState state;

	private GamePadState prevState;

	private bool _threadActive;

	public int refreshRate = 8;

	public int RegularUpdatesPerSecond;

	public int ThreadedUpdatesPerSecond;

	private Thread _updateThread;

	private int _updateCount;

	private float _time;

	private int _lastPos;

	private int _pos;

	private object _threadLock = new object();

	public int _maxLength = 100;

	private InputThread.InputStruct _lastFrameData;

	public InputThread.InputStruct[] inputsIn = new InputThread.InputStruct[100];

	public InputThread.InputStruct[] inputsOut = new InputThread.InputStruct[100];

	public InputFilter leftXFilter = new InputFilter();

	public InputFilter leftYFilter = new InputFilter();

	public InputFilter rightXFilter = new InputFilter();

	public InputFilter rightYFilter = new InputFilter();

	private int count;

	private AutoResetEvent reset;

	private bool directInput;

	private float inputTimer;

	public float inputInterval;

	private float _velxMax;

	private float _velxTemp;

	private float _velyMax;

	private float _velyTemp;

	private float _velxAvg;

	private float _velyAvg;

	private float _tempx;

	private float _tempy;

	private int _lastPosClamp
	{
		get
		{
			if (this._lastPos - 1 < this._maxLength)
			{
				int num = this._lastPos;
			}
			else
			{
				int num1 = this._maxLength;
			}
			if (this._lastPos - 1 < 0)
			{
				return 0;
			}
			return this._lastPos - 1;
		}
	}

	public Vector2 avgVelLastUpdate
	{
		get
		{
			TimeSpan timeSpan;
			this._tempx = 0f;
			this._tempy = 0f;
			if (this._lastPosClamp < 2)
			{
				return Vector2.zero;
			}
			for (int i = 1; i < this._lastPosClamp; i++)
			{
				float single = this._tempx;
				float single1 = this.inputsOut[i].rightX - this.inputsOut[i - 1].rightX;
				timeSpan = TimeSpan.FromTicks(this.inputsOut[i - 1].time - this.inputsOut[i].time);
				this._tempx = single + single1 / (float)timeSpan.Seconds;
			}
			this._velxAvg = this._tempx / (float)this._lastPosClamp;
			for (int j = 1; j < this._lastPosClamp; j++)
			{
				float single2 = this._tempx;
				float single3 = this.inputsOut[j].rightY - this.inputsOut[j - 1].rightY;
				timeSpan = TimeSpan.FromTicks(this.inputsOut[j - 1].time - this.inputsOut[j].time);
				this._tempx = single2 + single3 / (float)timeSpan.Seconds;
			}
			this._velyAvg = this._tempx / (float)this._lastPosClamp;
			return new Vector2(this._velxAvg, this._velyAvg);
		}
	}

	public Vector2 lastPosLeft
	{
		get
		{
			return new Vector2(this.inputsOut[this._lastPosClamp].leftX, this.inputsOut[this._lastPosClamp].leftY);
		}
	}

	public Vector2 lastPosRight
	{
		get
		{
			return new Vector2(this.inputsOut[this._lastPosClamp].rightX, this.inputsOut[this._lastPosClamp].rightY);
		}
	}

	public Vector2 maxVelLastUpdateLeft
	{
		get
		{
			return this.MaxVelLastUpdate(false);
		}
	}

	public Vector2 maxVelLastUpdateRight
	{
		get
		{
			return this.MaxVelLastUpdate(true);
		}
	}

	public float x
	{
		get
		{
			return this.inputsOut[this._lastPosClamp].rightX;
		}
	}

	public float xdel
	{
		get
		{
			return this.inputsOut[this._lastPosClamp].rightX - this.inputsOut[0].rightX;
		}
	}

	public InputThread()
	{
	}

	private void Awake()
	{
		if (!this.playerIndexSet || !this.prevState.IsConnected)
		{
			for (int i = 0; i < 4; i++)
			{
				PlayerIndex playerIndex = (PlayerIndex)i;
				if (!GamePad.GetState(playerIndex, GamePadDeadZone.None).IsConnected)
				{
					this.directInput = true;
				}
				else
				{
					this.directInput = false;
					this.playerIndex = playerIndex;
					this.playerIndexSet = true;
				}
			}
		}
		for (int j = 0; j < (int)this.inputsIn.Length; j++)
		{
			this.inputsIn[j] = new InputThread.InputStruct();
		}
		for (int k = 0; k < (int)this.inputsOut.Length; k++)
		{
			this.inputsOut[k] = new InputThread.InputStruct();
		}
		this._lastFrameData = new InputThread.InputStruct();
		this._threadActive = true;
		this._updateThread = new Thread(new ThreadStart(this.SuperFastLoop));
		this._updateThread.Start();
		this._time = Time.time;
		this.reset = new AutoResetEvent(false);
	}

	private void EmptyQueue()
	{
	}

	private float GetVel(float thisVal, float lastVal, long thisTime, long lastTime)
	{
		return (thisVal - lastVal) / ((float)(thisTime - lastTime) * 1E-07f);
	}

	private void InputUpdate()
	{
		this.prevState = this.state;
		this.state = GamePad.GetState(this.playerIndex);
		if (this._pos < this._maxLength)
		{
			if (this.state.IsConnected)
			{
				InputThread.InputStruct inputStruct = this.inputsIn[this._pos];
				InputFilter inputFilter = this.leftXFilter;
				GamePadThumbSticks thumbSticks = this.state.ThumbSticks;
				GamePadThumbSticks.StickValue left = thumbSticks.Left;
				inputStruct.leftX = inputFilter.Filter((double)left.X);
				InputThread.InputStruct inputStruct1 = this.inputsIn[this._pos];
				InputFilter inputFilter1 = this.leftYFilter;
				thumbSticks = this.state.ThumbSticks;
				left = thumbSticks.Left;
				inputStruct1.leftY = inputFilter1.Filter((double)left.Y);
				InputThread.InputStruct inputStruct2 = this.inputsIn[this._pos];
				InputFilter inputFilter2 = this.rightXFilter;
				thumbSticks = this.state.ThumbSticks;
				left = thumbSticks.Right;
				inputStruct2.rightX = inputFilter2.Filter((double)left.X);
				InputThread.InputStruct inputStruct3 = this.inputsIn[this._pos];
				InputFilter inputFilter3 = this.rightYFilter;
				thumbSticks = this.state.ThumbSticks;
				left = thumbSticks.Right;
				inputStruct3.rightY = inputFilter3.Filter((double)left.Y);
				this.inputsIn[this._pos].time = DateTime.UtcNow.Ticks;
				this.inputsIn[this._pos].leftXVel = this.GetVel(this.inputsIn[this._pos].leftX, this._lastFrameData.leftX, this.inputsIn[this._pos].time, this._lastFrameData.time);
				this.inputsIn[this._pos].leftYVel = this.GetVel(this.inputsIn[this._pos].leftY, this._lastFrameData.leftY, this.inputsIn[this._pos].time, this._lastFrameData.time);
				this.inputsIn[this._pos].rightXVel = this.GetVel(this.inputsIn[this._pos].rightX, this._lastFrameData.rightX, this.inputsIn[this._pos].time, this._lastFrameData.time);
				this.inputsIn[this._pos].rightYVel = this.GetVel(this.inputsIn[this._pos].rightY, this._lastFrameData.rightY, this.inputsIn[this._pos].time, this._lastFrameData.time);
				this._lastFrameData.leftX = this.inputsIn[this._pos].leftX;
				this._lastFrameData.leftY = this.inputsIn[this._pos].leftY;
				this._lastFrameData.rightX = this.inputsIn[this._pos].rightX;
				this._lastFrameData.rightY = this.inputsIn[this._pos].rightY;
				this._lastFrameData.time = this.inputsIn[this._pos].time;
				this._pos++;
				return;
			}
			this.inputsIn[this._pos].leftX = this.leftXFilter.Filter((double)((Mathf.Abs(this.inputController.player.GetAxis("LeftStickX")) < 0.1f ? 0f : this.inputController.player.GetAxis("LeftStickX"))));
			this.inputsIn[this._pos].leftY = this.leftYFilter.Filter((double)this.inputController.player.GetAxis("LeftStickY"));
			this.inputsIn[this._pos].rightX = this.rightXFilter.Filter((double)((Mathf.Abs(this.inputController.player.GetAxis("RightStickX")) < 0.1f ? 0f : this.inputController.player.GetAxis("RightStickX"))));
			this.inputsIn[this._pos].rightY = this.rightYFilter.Filter((double)this.inputController.player.GetAxis("RightStickY"));
			this.inputsIn[this._pos].time = DateTime.UtcNow.Ticks;
			this.inputsIn[this._pos].leftXVel = this.GetVel(this.inputsIn[this._pos].leftX, this._lastFrameData.leftX, this.inputsIn[this._pos].time, this._lastFrameData.time);
			this.inputsIn[this._pos].leftYVel = this.GetVel(this.inputsIn[this._pos].leftY, this._lastFrameData.leftY, this.inputsIn[this._pos].time, this._lastFrameData.time);
			this.inputsIn[this._pos].rightXVel = this.GetVel(this.inputsIn[this._pos].rightX, this._lastFrameData.rightX, this.inputsIn[this._pos].time, this._lastFrameData.time);
			this.inputsIn[this._pos].rightYVel = this.GetVel(this.inputsIn[this._pos].rightY, this._lastFrameData.rightY, this.inputsIn[this._pos].time, this._lastFrameData.time);
			this._lastFrameData.leftX = this.inputsIn[this._pos].leftX;
			this._lastFrameData.leftY = this.inputsIn[this._pos].leftY;
			this._lastFrameData.rightX = this.inputsIn[this._pos].rightX;
			this._lastFrameData.rightY = this.inputsIn[this._pos].rightY;
			this._lastFrameData.time = this.inputsIn[this._pos].time;
			this._pos++;
		}
	}

	private Vector2 MaxVelLastUpdate(bool right)
	{
		this._velxMax = 0f;
		this._velyMax = 0f;
		for (int i = 1; i < this._lastPosClamp; i++)
		{
			this._velxTemp = (right ? this.inputsOut[i].rightXVel : this.inputsOut[i].leftXVel);
			if (Mathf.Abs(this._velxTemp) > Mathf.Abs(this._velxMax))
			{
				this._velxMax = this._velxTemp;
			}
		}
		for (int j = 1; j < this._lastPosClamp; j++)
		{
			this._velyTemp = (right ? this.inputsOut[j].rightYVel : this.inputsOut[j].leftYVel);
			if (Mathf.Abs(this._velyTemp) > Mathf.Abs(this._velyMax))
			{
				this._velyMax = this._velyTemp;
			}
		}
		return new Vector2(this._velxMax, this._velyMax);
	}

	private void OnApplicationQuit()
	{
		this._threadActive = false;
		this._updateThread.Abort();
		this._updateThread.Join();
	}

	private void OnDestroy()
	{
		this._threadActive = false;
		this._updateThread.Abort();
		this._updateThread.Join();
	}

	private void OnDisable()
	{
		this._threadActive = false;
		this._updateThread.Abort();
		this._updateThread.Join();
	}

	private void SuperFastLoop()
	{
		long ticks = DateTime.UtcNow.Ticks;
		this.count = 0;
		while (this._threadActive)
		{
			if (DateTime.UtcNow.Ticks - ticks >= (long)10000000)
			{
				this.ThreadedUpdatesPerSecond = this.count;
				this.count = 0;
				ticks = DateTime.UtcNow.Ticks;
			}
			this.count++;
			this.InputUpdate();
			this.reset.WaitOne(this.refreshRate);
		}
	}

	public void Update()
	{
		if (!this.playerIndexSet || !this.prevState.IsConnected)
		{
			for (int i = 0; i < 4; i++)
			{
				PlayerIndex playerIndex = (PlayerIndex)i;
				if (!GamePad.GetState(playerIndex, GamePadDeadZone.None).IsConnected)
				{
					this.directInput = true;
				}
				else
				{
					this.directInput = false;
					this.playerIndex = playerIndex;
					this.playerIndexSet = true;
				}
			}
		}
		PlayerController.Instance.inputController.debugUI.SetThreadActive(this._threadActive);
		if (Time.time - this._time >= 1f)
		{
			this.RegularUpdatesPerSecond = this._updateCount;
			this._updateCount = 0;
			this._time = Time.time;
		}
		this._updateCount++;
		this.inputTimer += Time.deltaTime;
		if (this.inputTimer > this.inputInterval)
		{
			this.inputTimer = 0f;
			lock (this._threadLock)
			{
				this._lastPos = this._pos;
				Array.Copy(this.inputsIn, this.inputsOut, (int)this.inputsIn.Length);
				this._pos = 0;
			}
		}
	}

	public class InputStruct
	{
		public float leftX;

		public float leftY;

		public float rightX;

		public float rightY;

		public float leftXVel;

		public float leftYVel;

		public float rightXVel;

		public float rightYVel;

		public long time;

		public InputStruct()
		{
		}
	}
}