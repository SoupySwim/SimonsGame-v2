using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SimonsGame
{
	public class PlayerManager
	{
		private Dictionary<Guid, UsableInputMap> _playerInputMap = new Dictionary<Guid, UsableInputMap>();
		public Dictionary<Guid, UsableInputMap> PlayerInputMap { get { return _playerInputMap; } }
		private Dictionary<Guid, PlayerInfo> _playerInfoMap = new Dictionary<Guid, PlayerInfo>();
		public Dictionary<Guid, PlayerInfo> PlayerInfoMap { get { return _playerInfoMap; } }
		public Guid AddPlayer(UsableInputMap inputMap, string name = null)
		{
			Guid guid = Guid.NewGuid();
			_playerInputMap.Add(guid, inputMap);
			_playerInfoMap.Add(guid, new PlayerInfo()
			{
				Name = name == null ? "Player " + (_playerInfoMap.Count() + 1) : name,
				Id = guid,
				playerIndex = _playerInfoMap.Count()
			});
			return guid;
		}
	}

	public class UsableInputMap : Dictionary<AvailableButtons, int>
	{
		public bool IsAi { get; set; }
	}
	public class ControllerUsableInputMap : UsableInputMap
	{
		public PlayerIndex PlayerIndex { get; set; }
	}
	public class KeyboardUsableInputMap : UsableInputMap
	{
		public Keys Up { get; set; }
		public Keys Down { get; set; }
		public Keys Left { get; set; }
		public Keys Right { get; set; }
	}
	public class PlayerInfo
	{
		public String Name { get; set; }
		public int playerIndex { get; set; }
		public Guid Id { get; set; }
	}
}
