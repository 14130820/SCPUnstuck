using Exiled.API.Interfaces;

namespace ArithFeather.PlayerUnstuck {
	public class Config : IConfig {
		public const int CacheSize = 50;

		public bool IsEnabled { get; set; } = true;

		public float TimeBeforeDoorOpens { get; set; } = 15;

		public bool SCPOnly { get; set; } = false;
	}
}
