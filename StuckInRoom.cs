using MEC;
using System.Collections.Generic;
using Exiled.API.Features;

namespace ArithFeather.PlayerUnstuck
{
	public class StuckInRoom
	{
		private Player _player;
		private Door _door;
		private PlayerUnstuck _plugin;

		public CoroutineHandle Coroutine;

		public void Initialize(Player player, Door door, PlayerUnstuck plugin)
		{
			this._player = player;
			this._plugin = plugin;
			this._door = door;
			Coroutine = Timing.RunCoroutine(_ScpStuck());
		}

		private IEnumerator<float> _ScpStuck()
		{
			var go = _player.GameObject;

			_player.Broadcast(5, $"You got locked in! Door will open in {_plugin.Config.TimeBeforeDoorOpens} seconds");

			var timer = _plugin.Config.TimeBeforeDoorOpens;

			while (timer > 0)
			{
				var comp = go.GetComponent<NicknameSync>();
				if (_door.NetworkisOpen || comp == null)
				{
					_plugin.ScpTryingToEscape.Remove(_door.name);
					yield break;
				}
				else if (timer <= 5)
				{
					_player.Broadcast(1, $"Door will open in {timer}");
				}

				yield return Timing.WaitForSeconds(1);
				timer--;
			}

			_plugin.ScpTryingToEscape.Remove(_door.name);
			_door.NetworkisOpen = true;
		}

		#region LazyPool

		private const int CacheSize = 50;
		private static readonly StuckInRoom[] CachedClasses = new StuckInRoom[CacheSize];

		public static StuckInRoom SetPlayerStuck(Player player, Door door, PlayerUnstuck plugin)
		{
			for (int i = 0; i < CacheSize; i++)
			{
				var c = CachedClasses[i];

				if (c.Coroutine.IsRunning) continue;

				c.Initialize(player, door, plugin);
				return c;
			}

			var createNew = new StuckInRoom();
			createNew.Initialize(player, door, plugin);
			return createNew;
		}

		#endregion
	}
}
