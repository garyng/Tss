using System.Text.Json.Serialization;

namespace Tss.Core
{
	public class TssConfig
	{
		public string ClientId { get; set; }
		public string CredentialsPath { get; set; }
		public string MappingsPath { get; set; }
		public int CallbackPort { get; set; } = 8123;
		
		[JsonIgnore]
		public string CallbackUrl => $"http://localhost:{CallbackPort}/callback";
	}
}