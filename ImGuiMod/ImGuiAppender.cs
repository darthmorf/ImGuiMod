using log4net.Appender;
using log4net.Core;
using log4net.Layout;

namespace ImGuiMod;

internal class ImGuiAppender : AppenderSkeleton
{
	public ImGuiAppender()
	{
		Layout = new PatternLayout
		{
			ConversionPattern = "[%d{HH:mm:ss.fff}] [%t/%level] [%logger]: %m"
		};
		Name = "ImGUI";
	}

	protected override void Append(LoggingEvent loggingEvent)
	{
		AppLog.Logs.Add(RenderLoggingEvent(loggingEvent));
	}
}