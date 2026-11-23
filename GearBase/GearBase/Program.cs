using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/* Login com cookie (não vimos em sala). Depois que o funcionário entra, o navegador guarda um cookie
   e manda ele junto em todo pedido. O login vale 8 horas e vai sendo renovado enquanto a pessoa usa.
   O "opcoes => { ... }" é uma lambda, um método escrito ali mesmo, que o AddCookie usa para se configurar.
   O OnRedirectToLogin faz a API responder 401 quando não tem login. Sem ele, o ASP.NET tentaria abrir
   uma página de login própria, que não existe aqui. */
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.Cookie.Name = "GearBase";
        opcoes.Cookie.HttpOnly = true;
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(8);
        opcoes.SlidingExpiration = true;
        opcoes.Events.OnRedirectToLogin = contexto =>
        {
            contexto.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

/* Tratamento de erro único (não vimos em sala). O app.Use roda em volta de todo pedido: o await next()
   chama o controller e, se der exceção em qualquer lugar, ela cai no catch e volta como 400 com a mensagem.
   Precisa ficar aqui e não num try/catch do controller porque as validações dos set (preço negativo,
   quantidade zero...) rodam quando o ASP.NET monta o objeto, antes de o método do controller começar.
   O new { erro = ... } é um objeto sem classe que vira o JSON {"erro": "mensagem"} que a tela lê. */
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { erro = ex.Message });
    }
});

app.UseSwagger();
app.UseSwaggerUI();

/* O front-end (pasta wwwroot) é aberto pela própria API (não vimos em sala).
   UseDefaultFiles abre o index.html e UseStaticFiles entrega os arquivos HTML, CSS e JS. */
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Todos os endpoints exigem login, menos os marcados com [AllowAnonymous] (entrar e sair)
app.MapControllers().RequireAuthorization();

app.Run();
