// Punto de entrada mínimo. Sin BackgroundService ni jobs: solo la estructura.
var builder = Host.CreateApplicationBuilder(args);

var host = builder.Build();

host.Run();
