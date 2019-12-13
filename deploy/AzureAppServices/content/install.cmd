
rmdir /s /q D:\home\site\siteextensions\Datadog.Trace.AzureAppServices\v1_10_12\Tracer\
mkdir D:\home\site\siteextensions\Datadog.Trace.AzureAppServices\v1_10_12\Tracer\
xcopy /e D:\home\SiteExtensions\Datadog.Trace.AzureAppServices\Tracer D:\home\site\siteextensions\Datadog.Trace.AzureAppServices\v1_10_12\Tracer

mkdir %WEBROOT_PATH%\app_data\jobs\continuous\datadog-trace-agent
copy /y trace-agent.exe %WEBROOT_PATH%\app_data\jobs\continuous\datadog-trace-agent

