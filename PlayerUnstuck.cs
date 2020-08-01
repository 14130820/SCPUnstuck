using System;
using System.Collections.Generic;
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

		public override string Author => "Arith";
		public override Version Version => new Version("2.01");

		private List<SpawnPoint> _doorData;

		private Dictionary<string, SpawnPoint> _loadedDoorData;
		private Dictionary<string, SpawnPoint> LoadedDoorData => _loadedDoorData ?? (_loadedDoorData = new Dictionary<string, SpawnPoint>());

		public Dictionary<string, StuckInRoom> ScpTryingToEscape = new Dictionary<string, StuckInRoom>(Config.CacheSize);

		private void Server_WaitingForPlayers() {
			_doorData = PointAPI.GetPointList("LockedDoorCheckPoints");

			LoadedDoorData.Clear();
			ScpTryingToEscape.Clear();

			var playerPointCount = _doorData.Count;
			var serverRooms = Map.Rooms;
			var serveRoomsCount = serverRooms.Count;

			// Surface not included in Map.Rooms
			var surface = GameObject.Find("Root_*&*Outside Cams");
			FindDataMatchingServerRoom(playerPointCount, new Room(surface.name, surface.transform, surface.transform.position));

			// Create player spawn points on map
			for (var i = 0; i < serveRoomsCount; i++) {
				var serverRoom = serverRooms[i];
				FindDataMatchingServerRoom(playerPointCount, serverRoom);
			}
		}

		private void FindDataMatchingServerRoom(int playerPointCount, Room serverRoom) {
			var serverRoomName = serverRoom.Name;
			AriToolKit.Tools.TryFormatRoomName(ref serverRoomName);

			var check079 = serverRoomName == "HCZ_079";
			var check106 = serverRoomName == "HCZ_106";

			for (var j = 0; j < playerPointCount; j++) {
				var doorPoint = _doorData[j];
				var nameToCheck = doorPoint.RoomType; // Using doors for room check.

				if (check079 && nameToCheck.StartsWith("079")) {
					//079_FIRST etc
					nameToCheck = "HCZ_079";
				} else if (check106 && nameToCheck.StartsWith("106")) {
					//106_PRIMARY etc
					nameToCheck = "HCZ_106";
				}

				if (nameToCheck == serverRoomName) {
					LoadedDoorData.Add(doorPoint.RoomType, new SpawnPoint(doorPoint.RoomType, doorPoint.ZoneType,
						serverRoom.Transform.TransformPoint(doorPoint.Position) + new Vector3(0, 0.3f, 0),
						serverRoom.Transform.TransformDirection(doorPoint.Rotation)));
				}
			}
		}

		private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev) {
			var door = ev.Door;
			var doorName = door.DoorName;
			var player = ev.Player;

			// If door closed and access denied and they are an SCP and they aren't already trying to escape.
			if (door.isOpen || ev.IsAllowed || (Config.SCPOnly && (!Config.SCPOnly || player.Team != Team.SCP)) ||
				player.Role == RoleType.Scp079 || player.Role == RoleType.Scp106 ||
				ScpTryingToEscape.ContainsKey(doorName)) return;

			string roomName;
			if (doorName.StartsWith("079") || doorName.StartsWith("106")) {
				roomName = doorName;
			} else {
				roomName = player.CurrentRoom.Name;
				AriToolKit.Tools.TryFormatRoomName(ref roomName);
			}

			bool playerIsInRoom = false;

			if (LoadedDoorData.TryGetValue(roomName, out SpawnPoint point)) {
				var roomCheckPos = point.Position;
				var playerDistanceToRoom = Vector3.Distance(roomCheckPos, player.Position);
				var doorDistanceToRoom = Vector3.Distance(roomCheckPos, door.transform.position);

				playerIsInRoom = playerDistanceToRoom < doorDistanceToRoom;
			}

			if (playerIsInRoom) {
				ScpTryingToEscape.Add(doorName, StuckInRoom.SetPlayerStuck(ev.Player, door, this));
			}
		}
	}
}
