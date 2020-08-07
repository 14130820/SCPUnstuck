using System;
using System.Collections.Generic;
using ArithFeather.AriToolKit;
using ArithFeather.AriToolKit.PointEditor;
using Exiled.API.Features;
using UnityEngine;

namespace ArithFeather.PlayerUnstuck {

	public class PlayerUnstuck : Plugin<Config> {

		public PlayerUnstuck() {
			StuckInRoom.InitializePool();
		}

		public override void OnEnabled() {
			base.OnEnabled();
			Exiled.Events.Handlers.Player.InteractingDoor += Player_InteractingDoor;
			Exiled.Events.Handlers.Server.WaitingForPlayers += Server_WaitingForPlayers;
		}

		public override void OnDisabled() {
			Exiled.Events.Handlers.Player.InteractingDoor -= Player_InteractingDoor;
			Exiled.Events.Handlers.Server.WaitingForPlayers -= Server_WaitingForPlayers;
			base.OnDisabled();
		}

		public override void OnReloaded() {
			_doorData = PointAPI.GetPointList("LockedDoorCheckPoints");
			base.OnReloaded();
		}

		public override string Author => "Arith";
		public override Version Version => new Version("2.03");

		private PointList _doorData = PointAPI.GetPointList("LockedDoorCheckPoints");

		public Dictionary<string, StuckInRoom> ScpTryingToEscape = new Dictionary<string, StuckInRoom>(Config.CacheSize);

		private void Server_WaitingForPlayers() => ScpTryingToEscape.Clear();

		private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev) {
			var door = ev.Door;
			var doorName = door.DoorName.ToLowerInvariant();
			var player = ev.Player;

			if (Exiled.Loader.Loader.ShouldDebugBeShown) Debug.Log(doorName);

			// If door closed and access denied and they are an SCP and they aren't already trying to escape.
			if (door.isOpen || ev.IsAllowed || (Config.SCPOnly && (!Config.SCPOnly || player.Team != Team.SCP)) ||
				player.Role == RoleType.Scp079 || player.Role == RoleType.Scp106 ||
				ScpTryingToEscape.ContainsKey(doorName)) return;

			var room = player.CurrentRoom.GetCustomRoom();

			var points = _doorData.RoomGroupedFixedPoints[room.Id];
			var pointCount = points.Count;

			if (pointCount > 1) points = _doorData.IdGroupedFixedPoints[doorName];
			pointCount = points.Count;

			if (pointCount == 1) {
				var point = points[0];

				var roomCheckPos = point.Position;
				var playerDistanceToRoom = Vector3.Distance(roomCheckPos, player.Position);
				var doorDistanceToRoom = Vector3.Distance(roomCheckPos, door.transform.position);

				if (playerDistanceToRoom < doorDistanceToRoom)
					ScpTryingToEscape.Add(doorName, StuckInRoom.SetPlayerStuck(ev.Player, door, this));
			}
		}
	}
}
