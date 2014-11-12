using UnityEngine;
using System.Collections;
using Continental.Games;

/// <summary>
/// A Unity script for managing a player game object
/// </summary>
public class PlayerCharacterScript : MonoBehaviour
{
	public PlayerCharacter PlayerCharacter;
	private TextMesh PlayerName;

	// Use this for initialization
	void Start()
	{
		PlayerName = gameObject.transform.FindChild("PlayerName").GetComponent<TextMesh>();
		PlayerName.text = PlayerCharacter.Name;
		gameObject.GetComponent<MeshRenderer>().material.color = new UnityEngine.Color(PlayerCharacter.Color.R / 255f,
			  PlayerCharacter.Color.G / 255f, PlayerCharacter.Color.B / 255f, 1f);
		if (PlayerCharacter.playerId == Login.myPlayer.playerId)
		{
			gameObject.AddComponent<CharacterControl>().PlayerCharacter = PlayerCharacter;
		}
	}


	// Update is called once per frame
	void Update()
	{
		PlayerCharacter.Update(Time.deltaTime);
		if (PlayerCharacter.IsOnline)
		{
			PlayerName.color = new UnityEngine.Color(0, 1, 0);
		}
		else
		{
			Destroy(gameObject);
			return;
		}
		PlayerName.transform.rotation = Camera.main.transform.rotation;
		gameObject.transform.position = new Vector3(PlayerCharacter.Position.X, gameObject.transform.position.y,
			PlayerCharacter.Position.Y);
		gameObject.transform.rotation = Quaternion.Euler(0, PlayerCharacter.RotationAngle, 0);
	}
}
