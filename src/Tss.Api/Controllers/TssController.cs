using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tss.Core;

namespace Tss.Api.Controllers
{
	[ApiController]
	[Route("[action]")]
	public class TssController : ControllerBase
	{
		private readonly TssService _service;

		public TssController(TssService service)
		{
			_service = service;
		}

		/// <summary>
		/// Try logging in. A login url is returned if the login failed.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public async Task<TryLoginResult> TryLogin()
		{
			return await _service.TryLogin();
		}

		/// <summary>
		/// Complete the login flow.
		/// </summary>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<string>> Callback([FromQuery] string? code, [FromQuery] string? error)
		{
			if (!string.IsNullOrEmpty(error))
			{
				return Unauthorized($"Error: {error}");
			}

			await _service.CompleteLogin(code!);
			return Ok("Spotify Authorization was successful. You can close this tab now.");
		}

		/// <summary>
		/// Move the currently playing song to the "good" list.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public async Task MoveCurrentToGood()
		{
			await _service.MoveCurrentToGood();
		}

		/// <summary>
		/// Move the currently playing song to the "not good" list.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public async Task MoveCurrentToNotGood()
		{
			await _service.MoveCurrentToNotGood();
		}
	}
}