using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tss.Core;

namespace Tss.Api.Controllers
{
	public class TryLoginResponse
	{
		public bool Success { get; set; }
		public string? LoginUrl { get; set; }
	}


	[ApiController]
	[Route("[action]")]
	public class TssController : ControllerBase
	{
		private readonly TssService _service;

		public TssController(TssService service)
		{
			_service = service;
		}

		[HttpPost]
		public async Task<TryLoginResponse> TryLogin()
		{
			var (success, loginUrl) = await _service.TryLogin();

			return new TryLoginResponse
			{
				Success = success,
				LoginUrl = loginUrl
			};
		}

		[HttpGet]
		public async Task<string> Callback([FromQuery] string? code, [FromQuery] string? error)
		{
			// todo: on error return different code
			await _service.CompleteLogin(code, error);
			return "Spotify Authorization was successful. You can close this tab now.";
		}

		[HttpPost]
		public async Task MoveCurrentToGood()
		{
			await _service.MoveCurrentToGood();
		}
	}
}