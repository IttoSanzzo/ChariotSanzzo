using System.Text.Json;
using System.Text.Json.Serialization;
using ChariotSanzzo.Routes;
using ChariotSanzzo.Services;
using FluentValidation;
using Lavalink4NET.Extensions;

namespace ChariotSanzzo.Infrastructure {
	public static class AppFactory {
		public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new() {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			ReferenceHandler = ReferenceHandler.IgnoreCycles,
			Converters = {
					new JsonStringEnumConverter(null)
				}
		};

		private static WebApplication BuildApp(string[] args) {
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddHttpClient();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			builder.Services.AddControllers().AddJsonOptions(opts => {
				opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				opts.JsonSerializerOptions.IncludeFields = true;
			});
			builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
			builder.Configuration
				.AddJsonFile("configs/appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"configs/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
			builder.Services.AddCors((options) => {
				options.AddDefaultPolicy((policy) => {
					policy
						.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader();
				});
			});

			builder.LoadLocalServices();
			builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

			DiscordBotService.AddDiscordBotServiceToBuilder(builder);
			LavalinkService.AddLavalinkServiceToBuilder(builder);

			return builder.Build();
		}
		private static WebApplication ConfigureApp(this WebApplication app) {
			if (app.Environment.IsDevelopment()) {
				app.UseSwagger();
				app.UseSwaggerUI();
			}
			return app;
		}
		private static void LoadLocalServices(this WebApplicationBuilder builder) {
			// builder.Services.AddScoped<I, noI>();
			// builder.Services.AddScoped<IDtoValidationService, DtoValidationService>();
		}
		public static WebApplication CreateApp(string[] args) {
			var app = AppFactory.BuildApp(args).ConfigureApp();
			app.UseCors();
			app.UseWebSockets();
			app.LoadRouter();
			return app;
		}
	}
}
