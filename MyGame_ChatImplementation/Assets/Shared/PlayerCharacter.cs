using System;
using System.Collections.Generic;
using Continental.Shared;

namespace Continental.Games
{
	/// <summary>
	/// Defines player belongings: position in 3D, rotation, color, current movement
	/// </summary>
	public class PlayerCharacter : Player
	{
		public enum RotationState : byte
		{
			Motionless = 1,
			Left = 2,
			Right = 3
		}

		public enum MovementState : byte
		{
			Motionless = 1,
			Forward = 2,
			Backward = 3
		}

		public Continental.Games.Color Color;
		public string Name { get; set; }
		public bool IsOnline { get; set; }
		public Continental.Games.Vector2 Position { get; set; }
		public float RotationAngle { get; set; }
		public RotationState Rotation { get; set; }
		public MovementState Movement { get; set; }
		public ulong connectionId;
		public bool isInGame;

        public ulong GetConnectionId
        {
            get
            {
                return this.connectionId;
            }
            set { }
        }

		public DateTime CommandTime;
		/// <summary>
		/// Degrees per second
		/// </summary>
		public const float ROTATION_SPEED = 120f;

		/// <summary>
		/// units per second
		/// </summary>
		public const float MOVEMENT_SPEED = 5f;
		private const float Deg2Rad = 0.0174532925f;

		public void UpdateState(PlayerCharacter pc, bool alterPositionAndRotation, bool alterState = true)
		{
			if (alterState)
			{
				Movement = pc.Movement;
				Rotation = pc.Rotation;
			}
			if (alterPositionAndRotation)
			{
				if (pc.Movement.Equals(MovementState.Motionless))
				{
					Position = pc.Position;
				}
				if (pc.Rotation.Equals(RotationState.Motionless))
				{
					RotationAngle = pc.RotationAngle;
				}
			}
		}

		public void Update(float deltaTime)
		{
			if (!IsOnline)
				return;
			if (Rotation != RotationState.Motionless)
			{
				RotationAngle += (ROTATION_SPEED * deltaTime) * (Rotation.Equals(RotationState.Left) ? -1f : 1f);
			}
			if (Movement != MovementState.Motionless)
			{
				Continental.Games.Vector2 pos = Position;
				// eval and update position..z
				if (Movement.Equals(MovementState.Forward))
				{
					pos.X += MOVEMENT_SPEED * deltaTime * (float)Math.Sin(RotationAngle * Deg2Rad);
					pos.Y += MOVEMENT_SPEED * deltaTime * (float)Math.Cos(RotationAngle * Deg2Rad);
				}
				else if (Movement.Equals(MovementState.Backward))
				{
					pos.X -= MOVEMENT_SPEED * deltaTime * (float)Math.Sin(RotationAngle * Deg2Rad);
					pos.Y -= MOVEMENT_SPEED * deltaTime * (float)Math.Cos(RotationAngle * Deg2Rad);
				}
				// keep in bounds
				if (pos.X < Globals.MIN_X)
					pos.X = Globals.MIN_X;
				if (pos.Y < Globals.MIN_Y)
					pos.Y = Globals.MIN_Y;
				if (pos.X > Globals.MAX_X)
					pos.X = Globals.MAX_X;
				if (pos.Y > Globals.MAX_Y)
					pos.Y = Globals.MAX_Y;
				this.Position = pos;
			}
		}

		public new void Deserialize(System.IO.BinaryReader br, short command)
		{
			Continental.Games.Vector2 pos;
			switch (command)
			{
				case NetworkCommands.INIT_AREA:
				case NetworkCommands.PLAYER_ADDED:
					playerId = br.ReadUInt64();
					Name = br.ReadString();
					IsOnline = br.ReadBoolean();
					pos = new Continental.Games.Vector2();
					pos.Deserialize(br, command);
					Position = pos;
					RotationAngle = br.ReadSingle();
					Rotation = (RotationState)br.ReadByte();
					Movement = (MovementState)br.ReadByte();
					Continental.Games.Color color = new Continental.Games.Color();
					color.Deserialize(br, command);
					Color = color;
					break;

				case NetworkCommands.PLAYER_CHARACTER_STATE_CHANGE:
					playerId = br.ReadUInt64();
					Movement = (MovementState)br.ReadByte();
					if (Movement.Equals(MovementState.Motionless)) // character stopped, update position
					{
						pos = new Continental.Games.Vector2();
						pos.Deserialize(br, command);
						Position = pos;
					}
					Rotation = (RotationState)br.ReadByte();
					if (Rotation.Equals(RotationState.Motionless)) // rotation stopped, update angle
					{
						RotationAngle = br.ReadSingle();
					}
					CommandTime = new DateTime(br.ReadInt64());
					break;

				case NetworkCommands.ONLINE_STATE_CHANGE:
					playerId = br.ReadUInt64();
					IsOnline = br.ReadBoolean();
					break;
			}
		}

		public new void Serialize(System.IO.BinaryWriter bw, short command)
		{
			switch (command)
			{
				case NetworkCommands.INIT_AREA:
				case NetworkCommands.PLAYER_ADDED:
					bw.Write(playerId);
					bw.Write(Name);
					bw.Write(IsOnline);
					Position.Serialize(bw, command);
					bw.Write(RotationAngle);
					bw.Write((byte)Rotation);
					bw.Write((byte)Movement);
					Color.Serialize(bw, command);
					break;
				case NetworkCommands.PLAYER_CHARACTER_STATE_CHANGE:
					bw.Write(playerId);
					bw.Write((byte)Movement);
					if (Movement.Equals(MovementState.Motionless))
					{
						Position.Serialize(bw, command);
					}
					bw.Write((byte)Rotation);
					if (Rotation.Equals(RotationState.Motionless))
					{
						bw.Write(RotationAngle);
					}
					bw.Write(DateTime.Now.Ticks);
					break;
				case NetworkCommands.ONLINE_STATE_CHANGE:
					bw.Write(playerId);
					bw.Write(IsOnline);
					break;

			}
		}

		public new string ToString()
		{
			return string.Format("Movement:{0:g}, Rotation:{1:g}, position:{2}, angle:{3}, commandTime:{4:HH.mm.ss ffff}, TimeDiff:{5:#.###}, Name:{6}, internal Player data:({7})",
				this.Movement, Rotation, Position, RotationAngle, CommandTime, 
				DateTime.Now.Subtract(CommandTime).TotalMilliseconds, Name, base.ToString());
		}
	}
}
