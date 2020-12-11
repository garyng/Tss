using System.Threading;
using LanguageExt;
using MediatR;

namespace Tss.Core.Requests
{
	public static class MediatorExtensions
	{
		public static TryAsync<TResponse> TrySend<TResponse>(this IMediator @this, IRequest<TResponse> request,
			CancellationToken cancellationToken = default) => async () => await @this.Send(request, cancellationToken);
	}
}