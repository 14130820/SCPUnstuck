using System;
using System.Collections.Generic;
using Exiled.API.Enums;
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
		public override Version Version => new Version("2.06");

		public Dictionary<string, StuckInRoom>
			ScpTryingToEscape = new Dictionary<string, StuckInRoom>(Config.CacheSize);

		private void Server_WaitingForPlayers() {
			ScpTryingToEscape.Clear();
			_fixedPoints.Clear();

			// Fix points
			var rooms = Map.Rooms;
			var roomCount = rooms.Count;

			foreach (var doorPoint in _rawPoints) {
				var point = doorPoint.Value;
				var roomType = point.RoomType;

				for (int i = 0; i < roomCount; i++) {
					var room = rooms[i];

					if (room.Type == roomType)
						_fixedPoints.Add(doorPoint.Key,
							new DoorPoint(roomType, room.Transform.TransformPoint(point.Position)));

				}
			}
		}

		private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev) {
			var door = ev.Door;
			var doorName = door.DoorName.ToLowerInvariant();
			var player = ev.Player;

			// If door closed and access denied and they are an SCP and they aren't already trying to escape.
			if (door.isOpen || ev.IsAllowed || (Config.SCPOnly && (!Config.SCPOnly || player.Team != Team.SCP)) ||
				player.Role == RoleType.Scp079 || player.Role == RoleType.Scp106 ||
				ScpTryingToEscape.ContainsKey(doorName)) return;

			if (_fixedPoints.TryGetValue(doorName, out var point)) {

				var roomCheckPos = point.Position;
				var playerDistanceToRoom = Vector3.Distance(roomCheckPos, player.Position);
				var doorDistanceToRoom = Vector3.Distance(roomCheckPos, door.transform.position);

				if (playerDistanceToRoom < doorDistanceToRoom)
					ScpTryingToEscape.Add(doorName, StuckInRoom.SetPlayerStuck(ev.Player, door, this));
			}
		}

		private class DoorPoint {
			public readonly RoomType RoomType;
			public readonly Vector3 Position;

			public DoorPoint(RoomType roomType, Vector3 position) {
				this.RoomType = roomType;
				Position = position;
			}
		}

		private readonly Dictionary<string, DoorPoint> _fixedPoints = new Dictionary<string, DoorPoint>();

		private readonly Dictionary<string, DoorPoint> _rawPoints = new Dictionary<string, DoorPoint>
		{
			{"lcz_armory", new DoorPoint(RoomType.LczArmory, new Vector3(2.468124f, 1.43f, -0.01200104f))},
			{"012", new DoorPoint(RoomType.Lcz012, new Vector3(5.32489f, 1.430002f, -6.420892f))},
			{"914", new DoorPoint(RoomType.Lcz914, new Vector3(1.833687f, 1.430001f, 0.1032865f))},
			{"hid", new DoorPoint(RoomType.HczHid, new Vector3(0.06840611f, 1.429993f, -9.714242f))},
			{"079_first", new DoorPoint(RoomType.Hcz079, new Vector3(15.22645f, -3.113281f, 0.02222576f))},
			{"079_second", new DoorPoint(RoomType.Hcz079, new Vector3(6.521089f, -3.144531f, -14.89046f))},
			{"106_bottom", new DoorPoint(RoomType.Hcz106, new Vector3(21.49849f, -18.66949f, -20.07209f))},
			{"106_primary", new DoorPoint(RoomType.Hcz106, new Vector3(28.74622f, 1.329468f, 0.02032089f))},
			{"106_secondary", new DoorPoint(RoomType.Hcz106, new Vector3(29.03088f, 1.329468f, -29.51972f))},
			{"049_armory", new DoorPoint(RoomType.Hcz049, new Vector3(6.269577f, 265.4324f, 6.756578f))},
			{"nuke_armory", new DoorPoint(RoomType.HczNuke, new Vector3(-3.545931f, 401.43f, 18.09708f))},
			{"hcz_armory", new DoorPoint(RoomType.HczArmory, new Vector3(2.363129f, 1.429993f, 0.1472318f))},
			{"096", new DoorPoint(RoomType.Hcz096, new Vector3(-1.850082f, 1.429993f, -0.1229479f))},

			{"intercom", new DoorPoint(RoomType.EzIntercom, new Vector3(7.382059f, -0.4788208f, 1.433885f))},

			{"nuke_surface", new DoorPoint(RoomType.Surface, new Vector3(40.67078f, -11.0451f, -36.2037f))}
		};
	}
}
