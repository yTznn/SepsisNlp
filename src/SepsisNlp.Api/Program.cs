using Microsoft.AspNetCore.Http.Features;
using SepsisNlp.Application;
using SepsisNlp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// BLINDAGEM CONTRA ARQUIVOS GIGANTES (1GB+)
// ==========================================================
// 1. Tira o limite de tamanho do servidor Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = null;
});

// 2. Tira o limite de tamanho do formulário (Multipart)
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = long.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});
// ==========================================================

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ==========================================================
// 3. CORS: LIBERA O ACESSO DO FRONT-END EM REACT
// ==========================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Porta onde o seu React está rodando
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// ==========================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. ATIVA O CORS NA PIPELINE (Obrigatório vir antes da Autorização)
app.UseCors("AllowReact");

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();