using System.Threading.Tasks;
using GaryNg.Utils.Void;
using LanguageExt;

namespace Tss.Core.Requests
{
	public static class TaskExtensions
	{
		public static TryAsync<Void> ToTry(this Task @this)
			=> async () =>
			{
				await @this;
				return Void.Default;
			};

		public static TryAsync<T> ToTry<T>(this Task<T> @this)
			=> async () => await @this;
	}
}