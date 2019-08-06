using System;
using UnityEngine;

public class InputFilter
{
	public double cutoff = 10;

	public double dataCollection = 200;

	public double omega;

	public double a0;

	public double a1;

	public double a2;

	public double b1;

	public double b2;

	public double k1;

	public double k2;

	public double k3;

	public double twoPassCutoff;

	public double fsfc;

	public double[] raw = new double[3];

	public double[] filtered = new double[3];

	public InputFilter()
	{
		this.InitFilter();
	}

	public float Filter(double val)
	{
		Array.Copy(this.filtered, 1, this.filtered, 2, 1);
		Array.Copy(this.filtered, 0, this.filtered, 1, 1);
		Array.Copy(this.raw, 1, this.raw, 2, 1);
		Array.Copy(this.raw, 0, this.raw, 1, 1);
		this.raw[0] = val;
		this.filtered[0] = this.a0 * this.raw[0] + this.a1 * this.raw[1] + this.a2 * this.raw[2] + this.b1 * this.filtered[1] + this.b2 * this.filtered[2];
		return (float)this.filtered[0];
	}

	private void InitFilter()
	{
		this.omega = (double)Mathf.Tan((float)(3.14159274101257 * (this.cutoff / this.dataCollection))) / 0.802;
		this.k1 = (double)Mathf.Sqrt(2f) * this.omega;
		this.k2 = this.omega * this.omega;
		this.a0 = this.k2 / (1 + this.k1 + this.k2);
		this.a1 = 2 * this.a0;
		this.a2 = this.a0;
		this.k3 = this.a1 / this.k2;
		this.b1 = -2 * this.a0 + this.k3;
		this.b2 = 1 - 2 * this.a0 - this.k3;
		this.raw = new double[3];
		this.filtered = new double[3];
	}
}