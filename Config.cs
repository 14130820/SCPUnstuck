using Exiled.API.Interfaces;

namespace ArithFeather.ScpUnstuck {
	public class Config : IConfig
	{
		public bool IsEnabled { get; set; } = true;

		public float TimeBeforeDoorOpens { get; set; } = 15;
	}
}
