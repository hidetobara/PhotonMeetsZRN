using UnityEngine;
using System.Collections;

public class PlayerController : Photon.MonoBehaviour
{
	private Camera _BackCamera;
	private CharacterController _Controller;
	private IdleChanger _Motion;

	private Vector3 _Velocity;
	private Vector3 _Position { get { return gameObject.transform.position + new Vector3(0, 0.75f, 0); } }

	public void SetCamera(Camera c)
	{
		_BackCamera = c;
		_Controller = gameObject.GetComponent<CharacterController>();
		_Motion = gameObject.GetComponent<IdleChanger>();
	}

	private void Update()
	{
		if (photonView.isMine)
		{
			if (_BackCamera != null)
			{
				float distance = Vector3.Distance(_Position, _BackCamera.transform.position);
				if (distance > 1.0f)
				{
					_BackCamera.transform.position = Vector3.Lerp(_BackCamera.transform.position, _Position, 0.01f);
				}
				_BackCamera.transform.LookAt(gameObject.transform);
			}
		}
		else
		{
			transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * 5);
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow)) ActLeft();
		if (Input.GetKeyDown(KeyCode.RightArrow)) ActRight();
		if (Input.GetKeyDown(KeyCode.UpArrow)) ActForward();
		if (Input.GetKeyDown(KeyCode.DownArrow)) ActBack();
		if (Input.GetKeyDown(KeyCode.Space)) ActAttack();
	}

	private Vector3 correctPlayerPos = Vector3.zero; // We lerp towards this
	private Quaternion correctPlayerRot = Quaternion.identity; // We lerp towards this

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			// We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(_Motion.CurrentAction);
		}
		else
		{
			// Network player, receive data
			this.correctPlayerPos = (Vector3)stream.ReceiveNext();
			this.correctPlayerRot = (Quaternion)stream.ReceiveNext();
			_Motion.Change((IdleChanger.Action)stream.ReceiveNext());
		}
	}

	private void OnGUI()
	{
		if (!photonView.isMine) return;
		GUI.Label(
			new Rect(Screen.width * 0.9f, Screen.height * 0.1f, Screen.width * 0.1f, Screen.height * 0.1f),
			photonView.viewID + "." + photonView.ownerId);

		if (_BackCamera == null) return;

		Quaternion q = gameObject.transform.rotation;

		float size = 50;
		if (GUI.Button(new Rect(size, 0, size, size), "FRWRD")) ActForward();
		if (GUI.Button(new Rect(0, size, size, size), "LEFT")) ActLeft();
		if (GUI.Button(new Rect(size * 2, size, size, size), "RIGHT")) ActRight();
		if (GUI.Button(new Rect(size, size * 2, size, size), "BACK")) ActBack();
		if (GUI.Button(new Rect(size, size, size, size), "JUMP")) ActJump();
		_Velocity *= Dump;

		if (Vector3.Dot(_Velocity, _Velocity) < 0.0001f) _Motion.Change(IdleChanger.Action.WAIT00);

		if (_Controller.isGrounded)
			_Controller.Move(_Velocity * Time.deltaTime);
		else
			_Controller.SimpleMove(_Velocity * Time.deltaTime);
	}

	void ActLeft()
	{
		_Motion.Change(IdleChanger.Action.WALK00_F);
		Vector3 e = _Rotation.eulerAngles;
		e += new Vector3(0, -15, 0);
		_Rotation = Quaternion.Euler(e);
	}

	void ActRight()
	{
		_Motion.Change(IdleChanger.Action.WALK00_F);
		Vector3 e = _Rotation.eulerAngles;
		e += new Vector3(0, 15, 0);
		_Rotation = Quaternion.Euler(e);
	}

	Quaternion _Rotation { get { return gameObject.transform.rotation; } set { gameObject.transform.rotation = value; } }
	const float Impluse = 3.0f;
	const float Dump = 0.95f;
	void ActJump()
	{
		_Motion.Change(IdleChanger.Action.JUMP00);
		Vector3 m = _Rotation * Vector3.up * Impluse;
		_Velocity += m;
	}

	void ActForward()
	{
		_Motion.Change(IdleChanger.Action.WALK00_F);
		Vector3 m = _Rotation * Vector3.forward * Impluse;
		_Velocity += m;
	}

	void ActBack()
	{
		_Motion.Change(IdleChanger.Action.WALK00_F);
		Vector3 m = _Rotation * Vector3.back * Impluse;
		_Velocity += m;
	}

	void ActAttack()
	{
		Vector3 p = gameObject.transform.position + new Vector3(0, 0.5f, 0);
		GameObject ball = Instantiate(Resources.Load("ball"), p, new Quaternion()) as GameObject;
		ball.rigidbody.velocity = _Rotation * Vector3.forward * 5f;

		PhotonNetwork.RaiseEvent(1, ball.transform.position, true, null);
	}
}
