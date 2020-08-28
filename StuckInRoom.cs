using MEC;
using System.Collections.Generic;
using Exiled.API.Features;
using System;

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
			Coroutine = Timing.RunCoroutine(_PlayerStuck());
		}

		private IEnumerator<float> _PlayerStuck()
		{
			var go = _player.GameObject;
			var doorName = _door.DoorName.ToLowerInvariant();

			_player.Broadcast(5, PlayerUnstuck.Configs.WarnBroadcast);

			var timer = _plugin.Config.TimeBeforeDoorOpens;

			while (timer > 0)
			{
				var comp = go.GetComponent<NicknameSync>();
				if (_door.NetworkisOpen || comp == null)
				{
					_plugin.ScpTryingToEscape.Remove(doorName);
					yield break;
				}
				else if (timer <= PlayerUnstuck.Configs.WarnDoorOpeningIn)
				{
					_player.Broadcast(1, string.Format(PlayerUnstuck.Configs.TimerBroadcast, timer.ToString(PlayerUnstuck.CachedCultureInfo.NumberFormat)));
				}


				yield return Timing.WaitForSeconds(1);
				timer--;
			}

			_plugin.ScpTryingToEscape.Remove(doorName);
			_door.NetworkisOpen = true;
		}

		#region LazyPool

		private static readonly StuckInRoom[] CachedClasses = new StuckInRoom[Config.CacheSize];

		public static void InitializePool()
		{
			for (int i = 0; i < Config.CacheSize; i++)
			{
				CachedClasses[i] = new StuckInRoom();
			}
		}

		public static StuckInRoom SetPlayerStuck(Player player, Door door, PlayerUnstuck plugin)
		{
			for (int i = 0; i < Config.CacheSize; i++)
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
