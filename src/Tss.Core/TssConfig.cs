﻿namespace Tss.Core
{
	public class TssConfig
	{
		public string ClientId { get; set; }
		public string CredentialsPath { get; set; }
		public string MappingsPath { get; set; }
		public int CallbackPort { get; set; } = 8123;
	}
}