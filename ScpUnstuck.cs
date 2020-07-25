using System;
using System.Collections.Generic;
using ArithFeather.AriToolKit.PointEditor;
using Exiled.API.Features;
using UnityEngine;

namespace ArithFeather.ScpUnstuck {

	public class ScpUnstuck : Plugin<Config> {

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

		public override string Author => "Arith";
		public override Version Version => new Version("2.0");

		private List<SpawnPoint> _doorData;

		private Dictionary<string, SpawnPoint> _loadedDoorData;
		private Dictionary<string, SpawnPoint> LoadedDoorData => _loadedDoorData ?? (_loadedDoorData = new Dictionary<string, SpawnPoint>());

		private Dictionary<string, ScpStuckInRoom> _scpTryingToEscape;
		public Dictionary<string, ScpStuckInRoom> ScpTryingToEscape => _scpTryingToEscape ?? (_scpTryingToEscape = new Dictionary<string, ScpStuckInRoom>());

		private void Server_WaitingForPlayers() {
			_doorData = PointAPI.GetPointList("LockedDoorCheckPoints");

			LoadedDoorData.Clear();
			ScpTryingToEscape.Clear();

			var playerPointCount = _doorData.Count;
			var rooms = Map.Rooms;
			var roomCount = rooms.Count;

			// Create player spawn points on map
			for (var i = 0; i < roomCount; i++) {
				var r = rooms[i];

				var check079 = r.Name == "079";
				var check106 = r.Name == "106";

				for (var j = 0; j < playerPointCount; j++) {
					var p = _doorData[j];
					var roomType = p.RoomType;
					var nameToCheck = p.RoomType;

					if (check079 && nameToCheck.StartsWith("079")) {
						nameToCheck = "079";
					} else if (check106 && nameToCheck.StartsWith("106")) {
						nameToCheck = "106";
					}

					if (nameToCheck == r.Name && p.ZoneType == r.Zone) {
						LoadedDoorData.Add(roomType, new SpawnPoint(p.RoomType, p.ZoneType,
							r.Transform.TransformPoint(p.Position) + new Vector3(0, 0.3f, 0),
							r.Transform.TransformDirection(p.Rotation)));
					}
				}
			}
		}

		private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev) {
			var door = ev.Door;
			var doorName = door.DoorName;
			var player = ev.Player;
			// If door closed and access denied and they are an SCP and they aren't already trying to escape.
			if (!door.isOpen && !ev.IsAllowed && player.Team == Team.SCP && player.Role != RoleType.Scp079 && player.Role != RoleType.Scp106 && !ScpTryingToEscape.ContainsKey(doorName)) {
				var roomName = string.Empty;
				if (doorName.StartsWith("079") || doorName.StartsWith("106")) {
					roomName = doorName;
				}

				var scpPos = ev.Player.Position;

				if (string.IsNullOrEmpty(roomName))
				{
					roomName = player.CurrentRoom.Name;
				}

				bool playerIsInRoom = false;

				if (LoadedDoorData.TryGetValue(roomName, out SpawnPoint point))
				{
					var roomCheckPos = point.Position;
					var playerDistanceToRoom = Vector3.Distance(roomCheckPos, scpPos);
					var doorDistanceToRoom = Vector3.Distance(roomCheckPos, door.transform.position);

					playerIsInRoom = playerDistanceToRoom < doorDistanceToRoom;
				}

				if (playerIsInRoom) {
					ScpTryingToEscape.Add(doorName, new ScpStuckInRoom(ev.Player, door, this));
				}
			}
		}
	}
}
