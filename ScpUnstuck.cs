using ArithFeather.ArithSpawningKit.RandomPlayerSpawning;
using ArithFeather.ArithSpawningKit.SpawnPointTools;
using MEC;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Config;
using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArithFeather.ScpUnstuck
{
	[PluginDetails(
		author = "Arith",
		name = "SCP Unstuck",
		description = "",
		id = "ArithFeather.ScpUnstuck",
		configPrefix = "afsu",
		version = "1.0",
		SmodMajor = 3,
		SmodMinor = 4,
		SmodRevision = 0
		)]
	public class ScpUnstuck : Plugin, IEventHandlerWaitingForPlayers, IEventHandlerDoorAccess
	{
		[ConfigOption] private readonly bool disablePlugin = false;
		[ConfigOption] public readonly float timeBeforeDoorOpens = 15;

		public override void OnDisable() => Info("SCP Unstuck Disabled");
		public override void OnEnable() => Info("SCP Unstuck Enabled");

		public override void Register() => AddEventHandlers(this);

		private List<SpawnPoint> doorData;

		private Dictionary<string, PlayerSpawnPoint> loadedDoorData;
		private Dictionary<string, PlayerSpawnPoint> LoadedDoorData => loadedDoorData ?? (loadedDoorData = new Dictionary<string, PlayerSpawnPoint>());

		private Dictionary<string, ScpStuckInRoom> scpTryingToEscape;
		public Dictionary<string, ScpStuckInRoom> ScpTryingToEscape => scpTryingToEscape ?? (scpTryingToEscape = new Dictionary<string, ScpStuckInRoom>());

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			if (disablePlugin)
			{
				PluginManager.DisablePlugin(this);
			}

			doorData = SpawnDataIO.Open("sm_plugins/LockedDoorCheckPoints.txt");

			LoadedDoorData.Clear();
			ScpTryingToEscape.Clear();

			var playerPointCount = doorData.Count;
			var rooms = CustomRoomManager.Instance.Rooms;
			var roomCount = rooms.Count;

			// Create player spawn points on map
			for (var i = 0; i < roomCount; i++)
			{
				var r = rooms[i];

				for (var j = 0; j < playerPointCount; j++)
				{
					var p = doorData[j];

					if (p.RoomType == r.Name && p.ZoneType == r.Zone)
					{
						LoadedDoorData.Add(p.RoomType, new PlayerSpawnPoint(p.RoomType, p.ZoneType,
							Tools.Vec3ToVec(r.Transform.TransformPoint(Tools.VecToVec3(p.Position))) + new Vector(0, 0.3f, 0),
							Tools.Vec3ToVec(r.Transform.TransformDirection(Tools.VecToVec3(p.Rotation)))));
					}
				}
			}
		}

		public void OnDoorAccess(PlayerDoorAccessEvent ev)
		{
			// If door closed and access denied and they are an SCP and they aren't already trying to escape.
			if (!ev.Door.Open && !ev.Allow && ev.Player.TeamRole.Team == Smod2.API.Team.SCP && ev.Player.TeamRole.Role != Role.SCP_079 && !ScpTryingToEscape.ContainsKey(ev.Door.Name))
			{
				var scpPos = ev.Player.GetPosition().VecToVec3();
				var room = Tools.FindRoomAtPoint(scpPos);
				bool playerIsInRoom = false;

				if (LoadedDoorData.TryGetValue(room.Name, out PlayerSpawnPoint point))
				{
					var roomCheckPos = point.Position.VecToVec3();
					var playerDistanceToRoom = Vector3.Distance(roomCheckPos, scpPos);
					var doorDistanceToRoom = Vector3.Distance(roomCheckPos, ev.Door.Position.VecToVec3());

					playerIsInRoom = playerDistanceToRoom < doorDistanceToRoom;
				}
				else if (room.Name == "106" || room.Name == "079" || room.Name == "*&*Outside Cams")
				{
					playerIsInRoom = true;
				}

				if (playerIsInRoom)
				{
					ScpTryingToEscape.Add(ev.Door.Name, new ScpStuckInRoom(ev.Player, ev.Door.GetComponent() as Door, this));
				}
			}
		}
	}
}
