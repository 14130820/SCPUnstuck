using System;
using MEC;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Extensions;
using Interactables.Interobjects.DoorUtils;

namespace ArithFeather.PlayerUnstuck
{
	public class StuckInRoom
	{
		private Player _player;
		private DoorVariant _door;
		private PlayerUnstuck _plugin;

		public CoroutineHandle Coroutine;

		public void Initialize(Player player, DoorVariant door, PlayerUnstuck plugin)
		{
			this._player = player;
			this._plugin = plugin;
			this._door = door;
			Coroutine = Timing.RunCoroutine(_PlayerStuck());
		}

		private IEnumerator<float> _PlayerStuck()
		{
			var go = _player.GameObject;

			_player.Broadcast(5, PlayerUnstuck.Configs.WarnBroadcast);

			var timer = _plugin.Config.TimeBeforeDoorOpens;

			while (timer > 0)
			{
				var comp = go.GetComponent<NicknameSync>();
				if (_door.IsConsideredOpen() || comp == null)
				{
					_plugin.ScpTryingToEscape.Remove(_door.Type());
					yield break;
				}
				else if (timer <= PlayerUnstuck.Configs.WarnDoorOpeningIn)
				{
					_player.Broadcast(1, string.Format(PlayerUnstuck.Configs.TimerBroadcast, timer.ToString(PlayerUnstuck.CachedCultureInfo.NumberFormat)));
				}


				yield return Timing.WaitForSeconds(1);
				timer--;
			}

			_plugin.ScpTryingToEscape.Remove(_door.Type());
			_door.NetworkTargetState = true;
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

		public static StuckInRoom SetPlayerStuck(Player player, DoorVariant door, PlayerUnstuck plugin)
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
