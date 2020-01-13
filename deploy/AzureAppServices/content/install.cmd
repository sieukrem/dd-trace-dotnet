
REM Create logging directory for tracer version
mkdir D:\home\LogFiles\Datadog\v1_10_39

REM Copy starting log files
xcopy /e /d D:\home\SiteExtensions\Datadog.Trace.AzureAppServices\LogFiles D:\home\LogFiles\Datadog\v1_10_39

REM Create home directory for tracer version
mkdir D:\home\site\wwwroot\datadog\tracer\v1_10_39

REM Copy tracer home directory to version specific directory
xcopy /e D:\home\SiteExtensions\Datadog.Trace.AzureAppServices\Tracer D:\home\site\wwwroot\datadog\tracer\v1_10_39

REM Create directory for agent to live
mkdir  D:\home\site\wwwroot\datadog\tracer\v1_10_39\agent

REM Copy all agent files
xcopy /e D:\home\SiteExtensions\Datadog.Trace.AzureAppServices\Agent D:\home\site\wwwroot\datadog\tracer\v1_10_39\agent
