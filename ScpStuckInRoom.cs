using MEC;
using System.Collections.Generic;
using Exiled.API.Features;

namespace ArithFeather.ScpUnstuck
{
	public class ScpStuckInRoom
	{
		private readonly Player _player;
		private readonly Door _door;
		private readonly ScpUnstuck _plugin;

		public ScpStuckInRoom(Player player, Door door, ScpUnstuck plugin)
		{
			this._player = player;
			this._plugin = plugin;
			this._door = door;
			Timing.RunCoroutine(_ScpStuck());
		}

		private IEnumerator<float> _ScpStuck()
		{
			var go = _player.GameObject;

			Map.Broadcast(5, $"You got locked in! Door will open in {_plugin.Config.TimeBeforeDoorOpens} seconds");

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
					Map.Broadcast(1, $"Door will open in {timer}");
				}

				yield return Timing.WaitForSeconds(1);
				timer--;
			}

			_plugin.ScpTryingToEscape.Remove(_door.name);
			_door.NetworkisOpen = true;
		}
	}
}
