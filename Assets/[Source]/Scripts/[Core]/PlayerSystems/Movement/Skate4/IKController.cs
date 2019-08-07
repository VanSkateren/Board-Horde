using FSMHelper;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System;
using UnityEngine;

public class IKController : MonoBehaviour
{
	[SerializeField]
	private FullBodyBipedIK _finalIk;

	[SerializeField]
	private Animator _anim;

	[SerializeField]
	private AnimationCurve _animCurve;

	[SerializeField]
	private Transform skaterLeftFoot;

	[SerializeField]
	private Transform skaterRightFoot;

	[SerializeField]
	private Transform skaterLeftFootTargetParent;

	[SerializeField]
	private Transform skaterRightFootTargetParent;

	[SerializeField]
	private Transform skaterLeftFootTarget;

	[SerializeField]
	private Transform skaterRightFootTarget;

	[SerializeField]
	private Transform steezeLeftFootTarget;

	[SerializeField]
	private Transform steezeRightFootTarget;

	[SerializeField]
	private Transform ikAnimLeftFootTarget;

	[SerializeField]
	private Transform ikAnimRightFootTarget;

	[SerializeField]
	private Transform ikLeftFootPosition;

	[SerializeField]
	private Transform ikRightFootPosition;

	[SerializeField]
	private Transform ikLeftFootPositionOffset;

	[SerializeField]
	private Transform ikRightFootPositionOffset;

	[SerializeField]
	private Transform ikAnimBoard;

	[SerializeField]
	private Rigidbody physicsBoard;

	[SerializeField]
	private Transform physicsBoardBackwards;

	[SerializeField]
	private Rigidbody _ikAnim;

	[SerializeField]
	private float _lerpSpeed = 10f;

	[SerializeField]
	private float _steezeLerpSpeed = 2.5f;

	[Range(0f, 1f)]
	[SerializeField]
	private float _ikLeftPosLerp;

	[Range(0f, 1f)]
	[SerializeField]
	private float _ikLeftRotLerp;

	[Range(0f, 1f)]
	[SerializeField]
	private float _ikLeftLerpPosTarget;

	[Range(0f, 1f)]
	[SerializeField]
	private float _ikLeftLerpRotTarget;

	[Range(0f, 1f)]
	[SerializeField]
	private float _ikRightPosLerp;

	[Range(0f, 1f)]
	[SerializeField]
	private float _ikRightRotLerp;

	[Range(0f, 1f)]
	[SerializeField]
	private float _ikRightLerpPosTarget;

	[Range(0f, 1f)]
	[SerializeField]
	private float _ikRightLerpRotTarget;

	private float _rightPositionWeight = 1f;

	private float _leftPositionWeight = 1f;

	private float _rightRotationWeight = 1f;

	private float _leftRotationWeight = 1f;

	[SerializeField]
	private float _leftSteezeMax;

	[SerializeField]
	private float _rightSteezeMax;

	private float _leftSteezeTarget;

	[SerializeField]
	private float _leftSteezeWeight;

	private float _rightSteezeTarget;

	[SerializeField]
	private float _rightSteezeWeight;

	private Vector3 _skaterLeftFootPos = Vector3.zero;

	private Quaternion _skaterLeftFootRot = Quaternion.identity;

	private Vector3 _skaterRightFootPos = Vector3.zero;

	private Quaternion _skaterRightFootRot = Quaternion.identity;

	public float offsetScaler = 0.05f;

	public float popOffsetScaler = 0.5f;

	private Vector3 _boardPrevPos = Vector3.zero;

	private Vector3 _boardLastPos = Vector3.zero;

	private bool _impactSet;

	public IKController()
	{
	}

	private void FixedUpdate()
	{
		this.MoveAndRotateIKGameObject();
		this.LerpToTarget();
		this.SetSteezeWeight();
		this.SetIK();
	}

	public void ForceLeftLerpValue(float p_value)
	{
		this._ikLeftPosLerp = p_value;
		this._ikLeftLerpPosTarget = p_value;
	}

	public void ForceRightLerpValue(float p_value)
	{
		this._ikRightPosLerp = p_value;
		this._ikRightLerpPosTarget = p_value;
	}

	public float GetKneeBendWeight()
	{
		return this._finalIk.solver.leftLegChain.bendConstraint.weight;
	}

	public void LeftIKWeight(float p_value)
	{
		this._leftPositionWeight = p_value;
		this._leftRotationWeight = p_value;
		this._finalIk.solver.leftFootEffector.positionWeight = p_value;
		this._finalIk.solver.leftFootEffector.rotationWeight = p_value;
	}

	private void LerpToTarget()
	{
		this._ikLeftPosLerp = Mathf.MoveTowards(this._ikLeftPosLerp, this._ikLeftLerpPosTarget, Time.deltaTime * this._lerpSpeed);
		this._ikRightPosLerp = Mathf.MoveTowards(this._ikRightPosLerp, this._ikRightLerpPosTarget, Time.deltaTime * this._lerpSpeed);
		this._ikLeftRotLerp = Mathf.MoveTowards(this._ikLeftRotLerp, this._ikLeftLerpRotTarget, Time.deltaTime * this._lerpSpeed);
		this._ikRightRotLerp = Mathf.MoveTowards(this._ikRightRotLerp, this._ikRightLerpRotTarget, Time.deltaTime * this._lerpSpeed);
	}

	private void MoveAndRotateIKGameObject()
	{
		if (PlayerController.Instance.playerSM.IsInImpactStateSM())
		{
			if (!this._impactSet)
			{
				PlayerController.Instance.respawn.behaviourPuppet.BoostImmunity(1000f);
				this._impactSet = true;
			}
		}
		else if (this._impactSet)
		{
			this._impactSet = false;
		}
		this._ikAnim.velocity = this.physicsBoard.velocity;
		this._ikAnim.position = this.physicsBoard.position;
		Quaternion instance = PlayerController.Instance.boardController.boardMesh.rotation;
		if (PlayerController.Instance.GetBoardBackwards())
		{
			instance = Quaternion.AngleAxis(180f, PlayerController.Instance.boardController.boardMesh.up) * instance;
		}
		Vector3 vector3 = (!PlayerController.Instance.GetBoardBackwards() ? PlayerController.Instance.boardController.boardMesh.forward : -PlayerController.Instance.boardController.boardMesh.forward);
		Vector3 vector31 = Vector3.ProjectOnPlane(PlayerController.Instance.skaterController.skaterTransform.up, vector3);
		Vector3 vector32 = vector31.normalized;
		if (!PlayerController.Instance.IsGrounded() && !PlayerController.Instance.boardController.triggerManager.IsColliding)
		{
			instance = Quaternion.LookRotation(vector3, vector32);
		}
		Quaternion quaternion = Quaternion.Inverse(this.ikAnimBoard.rotation) * instance;
		Rigidbody rigidbody = this._ikAnim;
		rigidbody.rotation = rigidbody.rotation * quaternion;
		this._ikAnim.angularVelocity = this.physicsBoard.angularVelocity;
		this._boardLastPos = this.physicsBoard.position;
	}

	private void MoveAndRotateSkaterIKTargets()
	{
		this.skaterLeftFootTargetParent.position = this.skaterLeftFoot.position;
		this.skaterLeftFootTargetParent.rotation = this.skaterLeftFoot.rotation;
		this.skaterRightFootTargetParent.position = this.skaterRightFoot.position;
		this.skaterRightFootTargetParent.rotation = this.skaterRightFoot.rotation;
	}

	private void OnAnimatorIK(int layer)
	{
		this.MoveAndRotateSkaterIKTargets();
	}

	public void OnOffIK(float p_value)
	{
		this._leftPositionWeight = p_value;
		this._rightPositionWeight = p_value;
		this._rightRotationWeight = p_value;
		this._leftRotationWeight = p_value;
		this._finalIk.solver.leftFootEffector.positionWeight = p_value;
		this._finalIk.solver.rightFootEffector.positionWeight = p_value;
		this._finalIk.solver.rightFootEffector.rotationWeight = p_value;
		this._finalIk.solver.leftFootEffector.rotationWeight = p_value;
	}

	public void ResetIKOffsets()
	{
		Vector3 vector3 = this.ikRightFootPositionOffset.localPosition;
		vector3.x = 0f;
		vector3.z = 0f;
		this.ikRightFootPositionOffset.localPosition = vector3;
		Vector3 vector31 = this.ikLeftFootPositionOffset.localPosition;
		vector31.x = 0f;
		vector31.z = 0f;
		this.ikLeftFootPositionOffset.localPosition = vector31;
	}

	public void RightIKWeight(float p_value)
	{
		this._rightPositionWeight = p_value;
		this._rightRotationWeight = p_value;
		this._finalIk.solver.rightFootEffector.positionWeight = p_value;
		this._finalIk.solver.rightFootEffector.rotationWeight = p_value;
	}

	private void SetIK()
	{
		this.ikLeftFootPosition.position = this.ikAnimLeftFootTarget.position;
		this.ikRightFootPosition.position = this.ikAnimRightFootTarget.position;
		this._finalIk.solver.leftFootEffector.position = Vector3.Lerp(this.ikLeftFootPositionOffset.position, this._skaterLeftFootPos, this._ikLeftPosLerp);
		this._finalIk.solver.rightFootEffector.position = Vector3.Lerp(this.ikRightFootPositionOffset.position, this._skaterRightFootPos, this._ikRightPosLerp);
		this._finalIk.solver.leftFootEffector.rotation = Quaternion.Slerp(this.ikAnimLeftFootTarget.rotation, this._skaterLeftFootRot, this._ikLeftRotLerp);
		this._finalIk.solver.rightFootEffector.rotation = Quaternion.Slerp(this.ikAnimRightFootTarget.rotation, this._skaterRightFootRot, this._ikRightRotLerp);
		this._finalIk.solver.leftFootEffector.positionWeight = Mathf.MoveTowards(this._finalIk.solver.leftFootEffector.positionWeight, this._leftPositionWeight, Time.deltaTime * 5f);
		this._finalIk.solver.rightFootEffector.positionWeight = Mathf.MoveTowards(this._finalIk.solver.rightFootEffector.positionWeight, this._rightPositionWeight, Time.deltaTime * 5f);
		this._finalIk.solver.rightFootEffector.rotationWeight = Mathf.MoveTowards(this._finalIk.solver.rightFootEffector.rotationWeight, this._rightRotationWeight, Time.deltaTime * 5f);
		this._finalIk.solver.leftFootEffector.rotationWeight = Mathf.MoveTowards(this._finalIk.solver.leftFootEffector.rotationWeight, this._leftRotationWeight, Time.deltaTime * 5f);
	}

	public void SetIKRigidbodyKinematic(bool p_value)
	{
		this._ikAnim.isKinematic = p_value;
	}

	public void SetKneeBendWeight(float p_value)
	{
		this._finalIk.solver.leftLegChain.bendConstraint.weight = p_value;
		this._finalIk.solver.rightLegChain.bendConstraint.weight = p_value;
	}

	public void SetLeftIKOffset(float p_toeAxis, float p_forwardDir, float p_popDir, bool p_isPopStick, bool p_lockHorizontal, bool p_popping)
	{
		Vector3 pForwardDir = this.ikLeftFootPositionOffset.localPosition;
		if (p_lockHorizontal)
		{
			pForwardDir.y = 0f;
			pForwardDir.x = 0f;
			pForwardDir.z = p_forwardDir * this.offsetScaler;
		}
		else
		{
			pForwardDir.x = p_toeAxis * (p_popping ? this.popOffsetScaler : this.offsetScaler);
			if (!p_isPopStick)
			{
				pForwardDir.z = p_forwardDir * this.offsetScaler;
			}
			pForwardDir.y = 0f;
		}
		if (SettingsManager.Instance.stance == SettingsManager.Stance.Goofy)
		{
			pForwardDir.x = -pForwardDir.x;
		}
		pForwardDir.y = -0.01f;
		this.ikLeftFootPositionOffset.localPosition = Vector3.Lerp(this.ikLeftFootPositionOffset.localPosition, pForwardDir, Time.deltaTime * 10f);
	}

	public void SetLeftIKRotationWeight(float p_value)
	{
	}

	public void SetLeftLerpTarget(float pos, float rot)
	{
		this._ikLeftLerpPosTarget = pos;
		this._ikLeftLerpRotTarget = rot;
	}

	public void SetLeftSteezeWeight(float p_value)
	{
		this._leftSteezeTarget = p_value;
	}

	public void SetMaxSteeze(float p_value)
	{
		this._leftSteezeMax = p_value;
		this._rightSteezeMax = p_value;
	}

	public void SetMaxSteezeLeft(float p_value)
	{
		this._leftSteezeMax = p_value;
	}

	public void SetMaxSteezeRight(float p_value)
	{
		this._rightSteezeMax = p_value;
	}

	public void SetRightIKOffset(float p_toeAxis, float p_forwardDir, float p_popDir, bool p_isPopStick, bool p_lockHorizontal, bool p_popping)
	{
		Vector3 pForwardDir = this.ikRightFootPositionOffset.localPosition;
		if (p_lockHorizontal)
		{
			pForwardDir.y = 0f;
			pForwardDir.x = 0f;
			pForwardDir.z = p_forwardDir * this.offsetScaler;
		}
		else
		{
			pForwardDir.x = p_toeAxis * (p_popping ? this.popOffsetScaler : this.offsetScaler);
			if (!p_isPopStick)
			{
				pForwardDir.z = p_forwardDir * this.offsetScaler;
			}
			pForwardDir.y = 0f;
		}
		if (SettingsManager.Instance.stance == SettingsManager.Stance.Goofy)
		{
			pForwardDir.x = -pForwardDir.x;
		}
		pForwardDir.y = 0.005f;
		this.ikRightFootPositionOffset.localPosition = Vector3.Lerp(this.ikRightFootPositionOffset.localPosition, pForwardDir, Time.deltaTime * 10f);
	}

	public void SetRightIKRotationWeight(float p_value)
	{
	}

	public void SetRightLerpTarget(float pos, float rot)
	{
		this._ikRightLerpPosTarget = pos;
		this._ikRightLerpRotTarget = rot;
	}

	public void SetRightSteezeWeight(float p_value)
	{
		this._rightSteezeTarget = p_value;
	}

	private void SetSteezeWeight()
	{
		float single = Mathf.Clamp(this._leftSteezeTarget, 0f, this._leftSteezeMax);
		float single1 = Mathf.Clamp(this._rightSteezeTarget, 0f, this._rightSteezeMax);
		if (PlayerController.Instance.playerSM.LeftFootOffSM())
		{
			single = 1f;
		}
		if (PlayerController.Instance.playerSM.RightFootOffSM())
		{
			single1 = 1f;
		}
		this._leftSteezeWeight = Mathf.MoveTowards(this._leftSteezeWeight, single, Time.deltaTime * this._steezeLerpSpeed);
		this._rightSteezeWeight = Mathf.MoveTowards(this._rightSteezeWeight, single1, Time.deltaTime * this._steezeLerpSpeed);
		this._skaterLeftFootPos = Vector3.Lerp(this.skaterLeftFootTarget.position, this.steezeLeftFootTarget.position, this._leftSteezeWeight);
		this._skaterLeftFootRot = Quaternion.Slerp(this.skaterLeftFootTarget.rotation, this.steezeLeftFootTarget.rotation, this._leftSteezeWeight);
		this._skaterRightFootPos = Vector3.Lerp(this.skaterRightFootTarget.position, this.steezeRightFootTarget.position, this._rightSteezeWeight);
		this._skaterRightFootRot = Quaternion.Slerp(this.skaterRightFootTarget.rotation, this.steezeRightFootTarget.rotation, this._rightSteezeWeight);
	}

	private void Start()
	{
	}
}