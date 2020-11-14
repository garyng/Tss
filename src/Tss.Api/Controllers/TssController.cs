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
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<string>> Callback([FromQuery] string? code, [FromQuery] string? error)
		{
			if (!string.IsNullOrEmpty(error))
			{
				return Unauthorized($"Error: {error}");
			}

			await _service.CompleteLogin(code);
			return Ok("Spotify Authorization was successful. You can close this tab now.");
		}

		[HttpPost]
		public async Task MoveCurrentToGood()
		{
			await _service.MoveCurrentToGood();
		}
	}
}