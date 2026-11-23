// Punto de entrada mínimo. Sin endpoints, controladores ni lógica: solo la estructura.
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Run();
