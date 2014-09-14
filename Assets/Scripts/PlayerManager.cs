using UnityEngine;
using System.Collections;

public class PlayerManager : Photon.MonoBehaviour
{
	private PhotonView myPhotonView;
	public Camera MainCamera;

	// Use this for initialization
	void Start()
	{
		PhotonNetwork.ConnectUsingSettings("0.1");

		PhotonNetwork.sendRate = 10;
		PhotonNetwork.sendRateOnSerialize = 10;

		PhotonNetwork.OnEventCall += OnEventRaised;
	}

	void OnJoinedLobby()
	{
		Debug.Log("JoinRandom");
		PhotonNetwork.JoinRandomRoom();
	}

	void OnPhotonRandomJoinFailed()
	{
		PhotonNetwork.CreateRoom(null);
	}

	void OnJoinedRoom()
	{
		// 相手が入ってきてもここを通る？
		GameObject go = PhotonNetwork.Instantiate("unitychan", new Vector3(-3, 1, -3), Quaternion.identity, 0);
		go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
		myPhotonView = go.GetComponent<PhotonView>();

		if (myPhotonView.isMine)
		{
			PlayerController h = go.GetComponent<PlayerController>();
			h.SetCamera(MainCamera);

			myPhotonView.observed = go.transform as Component;
			myPhotonView.synchronization = ViewSynchronization.Unreliable;
			myPhotonView.onSerializeTransformOption = OnSerializeTransform.All;
		}
	}

	void OnEventRaised(byte eventCode, object content, int senderId)
	{
		if (eventCode == 1)
		{
			Vector3 p = (Vector3)content;
			print(senderId + "." + p);
		}
	}
}
