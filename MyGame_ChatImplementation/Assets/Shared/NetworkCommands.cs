using System;
using Continental.Shared;
namespace Continental.Games
{
	/// <summary>
	/// A list of network commands
	/// </summary>
	public class NetworkCommands : CommandCodes
	{
		/// <summary>
		/// Sent to single player upon logging to the game
		/// </summary>
		public const short INIT_AREA = 1001;
		/// <summary>
		/// Client to server message. A player requests the game state from server, including all players location, rotation and unfinished moves.
		/// </summary>
		public const short GET_AREA = 1002;
		/// <summary>
		/// Informs that a player had changed its state (movement, rotation)
		/// </summary>
		public const short PLAYER_CHARACTER_STATE_CHANGE = 1003;
		/// <summary>
		/// Server informs client that another player has logged in/out
		/// </summary>
		public const short ONLINE_STATE_CHANGE = 1004;
		/// <summary>
		/// Server informs client that a newly registered player has joined the game
		/// </summary>
		public const short PLAYER_ADDED = 1005;
		/// <summary>
		/// Client to server message. A player requests login.
		/// </summary>
		public const short AUTHORIZE_REQUEST = 1006;
		/// <summary>
		/// Login successfull.
		/// </summary>
		public const short AUTHORIZE_RESPONSE_OK = 1007;
		/// <summary>
		/// Login unsuccessfull.
		/// </summary>
		public const short AUTHORIZE_RESPONSE_FAIL = 1008;
		/// <summary>
		/// Client to server message. A player requests registration.
		/// </summary>
		public const short REGISTER_REQUEST = 1009;
		/// <summary>
		/// Registration successfull.
		/// </summary>
		public const short REGISTER_RESPONSE_OK = 1010;
		/// <summary>
		/// Registration unsuccessfull.
        /// </summary>
        public const short REGISTER_RESPONSE_FAIL = 1011;

        public const short MESSAGE_SEND = 1012;
        public const short MESSAGE_RECIEVE = 1013;
        public const short AUTHORIZE_GUEST = 1014;
        public const short AUTHORIZE_GUEST_OK = 1014;
        public const short AUTHORIZE_GUEST_SUCESSFUL = 1015;
	}
}
