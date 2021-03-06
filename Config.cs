﻿using Exiled.API.Interfaces;

namespace ArithFeather.PlayerUnstuck {
	public class Config : IConfig {
		public const int CacheSize = 50;

		public bool IsEnabled { get; set; } = true;

		public int TimeBeforeDoorOpens { get; set; } = 15;

		public int WarnDoorOpeningIn { get; set; } = 5;

		public bool SCPOnly { get; set; } = false;

		public string LanguageCultureInfo { get; set; } = "en-US";

		public string WarnBroadcast { get; set; } =
			"You got locked in! Door will open in {0} seconds";

		public string TimerBroadcast { get; set; } =
			"Door will open in {0}";
	}
}
