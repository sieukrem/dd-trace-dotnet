
REM Create logging directory for tracer version
mkdir D:\home\LogFiles\Datadog\Tracer\v1_10_32

REM Create home directory for tracer version
mkdir D:\home\site\wwwroot\datadog\tracer\v1_10_32

REM Copy tracer home directory to version specific directory
xcopy /e D:\home\SiteExtensions\Datadog.Trace.AzureAppServices\Tracer D:\home\site\wwwroot\datadog\tracer\v1_10_32

REM Create directory for trace proxy job to live
mkdir D:\home\site\wwwroot\app_data\jobs\continuous\tracer-proxy-job

REM Copy all tracer proxy files
xcopy /e D:\home\SiteExtensions\Datadog.Trace.AzureAppServices\TracerProxy D:\home\site\wwwroot\app_data\jobs\continuous\tracer-proxy-job

REM Create directory for agent to live
mkdir D:\home\site\wwwroot\app_data\jobs\continuous\datadog-agent

REM Copy all agent files
xcopy /e D:\home\SiteExtensions\Datadog.Trace.AzureAppServices\Agent D:\home\site\wwwroot\app_data\jobs\continuous\datadog-agent
