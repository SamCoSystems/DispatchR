using Microsoft.Extensions.Logging;

namespace DispatchR;

delegate void LogAction(string formatString, params object?[] parameters);

internal class DispatcherLog
{
	private readonly ILogger? _logger;

	public DispatcherErrors Errors { get; }
	public DispatcherDebug Debug { get; internal set; }

	public DispatcherLog(ILogger? logger)
	{
		_logger = logger;
		Errors = new DispatcherErrors(LogError);
		Debug = new DispatcherDebug(LogDebug);
	}

	private void LogError(string formatString, params object?[] parameters)
		=> Log(LogLevel.Error, formatString, parameters);

	private void LogDebug(string formatString, params object?[] parameters)
		=> Log(LogLevel.Debug, formatString, parameters);

	internal void Log(LogLevel level, string formatString, params object?[] parameters)
		=> _logger?.Log(level, formatString, parameters);
	
}
