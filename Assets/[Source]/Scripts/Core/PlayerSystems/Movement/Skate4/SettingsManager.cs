using RootMotion.FinalIK;
using System;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
	[SerializeField]
	private Animator _skaterAnimator;

	[SerializeField]
	private Animator _steezeAnimator;

	[SerializeField]
	private Animator _ikAnimator;

	[SerializeField]
	private RuntimeAnimatorController _regularAnim;

	[SerializeField]
	private RuntimeAnimatorController _goofyAnim;

	[SerializeField]
	private RuntimeAnimatorController _regularSteezeAnim;

	[SerializeField]
	private RuntimeAnimatorController _goofySteezeAnim;

	[SerializeField]
	private RuntimeAnimatorController _regularIkAnim;

	[SerializeField]
	private RuntimeAnimatorController _goofyIkAnim;

	private static SettingsManager _instance;

	public SettingsManager.Stance stance;

	public SettingsManager.ControlType controlType;

	public static SettingsManager Instance
	{
		get
		{
			return SettingsManager._instance;
		}
	}

	public SettingsManager()
	{
	}

	private void Awake()
	{
		if (!(SettingsManager._instance != null) || !(SettingsManager._instance != this))
		{
			SettingsManager._instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		this.SetStance((PlayerPrefs.GetInt("stance", 0) == 0 ? SettingsManager.Stance.Regular : SettingsManager.Stance.Goofy));
	}

	public void SetStance(SettingsManager.Stance p_stance)
	{
		this.stance = p_stance;
		PlayerPrefs.SetInt("stance", (this.stance == SettingsManager.Stance.Regular ? 0 : 1));
		if (this.stance == SettingsManager.Stance.Goofy)
		{
			PlayerController.Instance.skaterController.finalIk.solver.leftLegChain.bendConstraint.bendGoal = PlayerController.Instance.skaterController.goofyLeftKneeGuide;
			PlayerController.Instance.skaterController.finalIk.solver.rightLegChain.bendConstraint.bendGoal = PlayerController.Instance.skaterController.goofyRightKneeGuide;
			this._skaterAnimator.runtimeAnimatorController = this._goofyAnim;
			this._steezeAnimator.runtimeAnimatorController = this._goofySteezeAnim;
			this._ikAnimator.runtimeAnimatorController = this._goofyIkAnim;
			return;
		}
		PlayerController.Instance.skaterController.finalIk.solver.leftLegChain.bendConstraint.bendGoal = PlayerController.Instance.skaterController.regsLeftKneeGuide;
		PlayerController.Instance.skaterController.finalIk.solver.rightLegChain.bendConstraint.bendGoal = PlayerController.Instance.skaterController.regsRightKneeGuide;
		this._skaterAnimator.runtimeAnimatorController = this._regularAnim;
		this._steezeAnimator.runtimeAnimatorController = this._regularSteezeAnim;
		this._ikAnimator.runtimeAnimatorController = this._regularIkAnim;
	}

	public enum ControlType
	{
		Same,
		Swap,
		Simple
	}

	public enum Stance
	{
		Regular,
		Goofy
	}
}