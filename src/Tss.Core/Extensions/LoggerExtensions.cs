using System;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace Tss.Core.Extensions
{
	/// <summary>
	/// Extension to <see cref="ILogger{T}"/> which returns <see cref="Unit"/> instead of <see cref="Void"/>.
	/// </summary>
	public static class LoggerExtensions
	{
		//
		// Summary:
		//     Formats and writes a critical log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Critical<T>(this ILogger<T> logger, string message, params object[] args)
		{
			logger.LogCritical(message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a critical log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Critical<T>(this ILogger<T> logger, Exception exception, string message,
			params object[] args)
		{
			logger.LogCritical(exception, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a critical log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Critical<T>(this ILogger<T> logger, EventId eventId, string message,
			params object[] args)
		{
			logger.LogCritical(eventId, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a critical log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Critical<T>(this ILogger<T> logger, EventId eventId, Exception exception,
			string message, params object[] args)
		{
			logger.LogCritical(eventId, exception, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a debug log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Debug<T>(this ILogger<T> logger, EventId eventId, Exception exception,
			string message, params object[] args)
		{
			logger.LogDebug(eventId, exception, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a debug log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Debug<T>(this ILogger<T> logger, EventId eventId, string message,
			params object[] args)
		{
			logger.LogDebug(eventId, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a debug log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Debug<T>(this ILogger<T> logger, Exception exception, string message,
			params object[] args)
		{
			logger.LogDebug(exception, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a debug log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Debug<T>(this ILogger<T> logger, string message, params object[] args)
		{
			logger.LogDebug(message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes an error log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Error<T>(this ILogger<T> logger, string message, params object[] args)
		{
			logger.LogError(message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes an error log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Error<T>(this ILogger<T> logger, Exception exception, string message,
			params object[] args)
		{
			logger.LogError(exception, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes an error log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Error<T>(this ILogger<T> logger, EventId eventId, Exception exception,
			string message, params object[] args)
		{
			logger.LogError(eventId, exception, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes an error log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Error<T>(this ILogger<T> logger, EventId eventId, string message,
			params object[] args)
		{
			logger.LogError(eventId, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes an informational log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Information<T>(this ILogger<T> logger, EventId eventId, string message,
			params object[] args)
		{
			logger.LogInformation(eventId, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes an informational log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Information<T>(this ILogger<T> logger, Exception exception, string message,
			params object[] args)
		{
			logger.LogInformation(exception, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes an informational log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Information<T>(this ILogger<T> logger, EventId eventId, Exception exception,
			string message, params object[] args)
		{
			logger.LogInformation(eventId, exception, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes an informational log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Information<T>(this ILogger<T> logger, string message, params object[] args)
		{
			logger.LogInformation(message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a trace log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Trace<T>(this ILogger<T> logger, string message, params object[] args)
		{
			logger.LogTrace(message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a trace log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Trace<T>(this ILogger<T> logger, Exception exception, string message,
			params object[] args)
		{
			logger.LogTrace(exception, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a trace log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Trace<T>(this ILogger<T> logger, EventId eventId, string message,
			params object[] args)
		{
			logger.LogTrace(eventId, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a trace log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Trace<T>(this ILogger<T> logger, EventId eventId, Exception exception,
			string message, params object[] args)
		{
			logger.LogTrace(eventId, exception, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a warning log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Warning<T>(this ILogger<T> logger, EventId eventId, string message,
			params object[] args)
		{
			logger.LogWarning(eventId, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a warning log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   eventId:
		//     The event id associated with the log.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Warning<T>(this ILogger<T> logger, EventId eventId, Exception exception,
			string message, params object[] args)
		{
			logger.LogWarning(eventId, exception, message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a warning log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Warning<T>(this ILogger<T> logger, string message, params object[] args)
		{
			logger.LogWarning(message, args);
			return Unit.Default;
		}

		//
		// Summary:
		//     Formats and writes a warning log message.
		//
		// Parameters:
		//   logger:
		//     The Microsoft.Extensions.Logging.ILogger to write to.
		//
		//   exception:
		//     The exception to log.
		//
		//   message:
		//     Format string of the log message in message template format. Example:
		//     "User {User} logged in from {Address}"
		//
		//   args:
		//     An object array that contains zero or more objects to format.
		public static Unit Warning<T>(this ILogger<T> logger, Exception exception, string message,
			params object[] args)
		{
			logger.LogWarning(exception, message, args);
			return Unit.Default;
		}
	}
}