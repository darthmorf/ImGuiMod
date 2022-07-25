using log4net.Appender;
using log4net.Core;
using log4net.Layout;

namespace ImGUI;

internal class ImGuiAppender : AppenderSkeleton
{
	public ImGuiAppender()
	{
		Layout = new PatternLayout
		{
			ConversionPattern = "[%d{HH:mm:ss.fff}] [%t/%level] [%logger]: %m"
		};
	}

	public string Name { get; set; } = "ImGUI";

	protected override void Append(LoggingEvent loggingEvent)
	{
		AppLog.Logs.Add(RenderLoggingEvent(loggingEvent));
	}
}