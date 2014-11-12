using UnityEngine;
using System.Collections;
using Continental.Games;
using Continental;
using System;
using System.IO;
using System.Text;

/// <summary>
/// A Unity script for managing keyboard control on the player character.
/// </summary>
public class CharacterControl : MonoBehaviour
{
	public PlayerCharacter PlayerCharacter;

	void Start()
	{
		Camera.main.transform.position = gameObject.transform.position + transform.forward * (-6) + transform.up * 2;
		Camera.main.transform.rotation = gameObject.transform.rotation;
		Camera.main.transform.LookAt(gameObject.transform);
		Camera.main.transform.parent = gameObject.transform;
	}

	void Update()
	{
		if (PlayerCharacter == null)
			return;
		if (!PlayerCharacter.IsOnline)
		{
			Destroy(gameObject);
			PlayerCharacter = null;
			return;
		}
		PlayerCharacter.RotationState previousRotationState = PlayerCharacter.Rotation;
		PlayerCharacter.MovementState previousMovementState = PlayerCharacter.Movement;
		
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
		{
			PlayerCharacter.Rotation = PlayerCharacter.RotationState.Left;
		}
		else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
		{
			PlayerCharacter.Rotation = PlayerCharacter.RotationState.Right;
		}
		else
		{
			PlayerCharacter.Rotation = PlayerCharacter.RotationState.Motionless;
		}

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
		{
			PlayerCharacter.Movement = PlayerCharacter.MovementState.Forward;
		}
		else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
		{
			PlayerCharacter.Movement = PlayerCharacter.MovementState.Backward;
		}
		else
		{
			PlayerCharacter.Movement = PlayerCharacter.MovementState.Motionless;
		}
		if (previousRotationState != PlayerCharacter.Rotation || previousMovementState != PlayerCharacter.Movement)
		{
			//send message to server if a movement is started of finished
			PlayerCharacter.CommandTime = DateTime.Now;
			Login.connection.Send(NetworkCommands.PLAYER_CHARACTER_STATE_CHANGE, PlayerCharacter);
		}
	}
}
