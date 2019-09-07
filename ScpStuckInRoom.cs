using MEC;
using Smod2.API;
using System.Collections.Generic;
using UnityEngine;

namespace ArithFeather.ScpUnstuck
{
	public class ScpStuckInRoom
	{
		private readonly Player player;
		private readonly Door door;
		private readonly ScpUnstuck plugin;

		public ScpStuckInRoom(Player player, Door door, ScpUnstuck plugin)
		{
			this.player = player;
			this.plugin = plugin;
			this.door = door;
			Timing.RunCoroutine(_ScpStuck());
		}

		private IEnumerator<float> _ScpStuck()
		{
			var go = player.GetGameObject() as GameObject;
			var broadcast = GameObject.Find("Host").GetComponent<Broadcast>();

			broadcast.CallTargetAddElement(go.GetComponent<NicknameSync>().connectionToClient, $"You got locked in! Door will open in {plugin.timeBeforeDoorOpens} seconds", 5, false);

			var timer = plugin.timeBeforeDoorOpens;

			while (timer > 0)
			{
				var comp = go.GetComponent<NicknameSync>();
				if (door.NetworkisOpen || comp == null)
				{
					plugin.ScpTryingToEscape.Remove(door.name);
					yield break;
				}
				else if (timer <= 5)
				{
					broadcast.CallTargetAddElement(comp.connectionToClient, $"Door will open in {timer}", 1, false);
				}

				yield return Timing.WaitForSeconds(1);
				timer--;
			}

			plugin.ScpTryingToEscape.Remove(door.name);
			door.NetworkisOpen = true;
		}
	}
}
