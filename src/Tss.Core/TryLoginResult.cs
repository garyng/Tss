namespace Tss.Core
{
	public class TryLoginResult
	{
		public bool Success { get; set; }
		public string? LoginUrl { get; set; }

		public TryLoginResult(bool success, string? loginUrl)
		{
			Success = success;
			LoginUrl = loginUrl;
		}
	}
}