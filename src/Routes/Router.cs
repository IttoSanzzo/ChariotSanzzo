using System.Reflection;
using System.Runtime.CompilerServices;

namespace ChariotSanzzo.Routes {
	public interface IWithFilePath {
		public string FilePath { get; }
	}
	public abstract class WithFilePath([CallerFilePath] string path = "") : IWithFilePath {
		public string FilePath { get; } = path;
	}
	public interface IRouteGroup : IWithFilePath {
		RouteGroupBuilder Configure(RouteGroupBuilder builder);
	}
	public interface IRoute : IWithFilePath {
		Delegate Handle { get; }
		RouteHandlerBuilder Configure(RouteHandlerBuilder builder);
	}

	public static class Router {
		private static string RouterBaseFilePath = "";
		private static Dictionary<string, RouteGroupBuilder> RouteGroups { get; set; } = [];

		public static void LoadRouter(this WebApplication app) {
			LoadRouterBaseFilePath();
			app.LoadRouteGroups();
			app.LoadRoutes();
		}
		private static void LoadRouteGroups(this WebApplication app) {
			var routeGroupTypes = Assembly.GetExecutingAssembly().GetTypes()
			.Where((T) => typeof(IRouteGroup).IsAssignableFrom(T)
				&& !T.IsInterface
				&& !T.IsAbstract);

			foreach (var type in routeGroupTypes) {
				var group = (IRouteGroup)Activator.CreateInstance(type)!;
				var path = GetRouteGroupEndpoint(group.FilePath);
				var groupBuilder = app.MapGroup(path);
				group.Configure(groupBuilder.WithOpenApi());
				RouteGroups.Add(path, groupBuilder);
			}
			RouteGroups = RouteGroups.OrderByDescending(x => x.Key.Length).ToDictionary();
		}
		private static void LoadRoutes(this WebApplication app) {
			var routeTypes = Assembly.GetExecutingAssembly().GetTypes()
			.Where((T) => typeof(IRoute).IsAssignableFrom(T)
				&& !T.IsInterface
				&& !T.IsAbstract);

			foreach (var type in routeTypes)
				AddRoute(app, (IRoute)Activator.CreateInstance(type)!);
		}

		private static void AddRoute(WebApplication app, IRoute route) {
			var (path, method) = GetRouteEndpointAndMethod(route.FilePath);
			var routeGroupEndpoint = RouteGroups.Keys.FirstOrDefault(path.StartsWith);
			route.Configure((routeGroupEndpoint != null)
				? RouteGroups[routeGroupEndpoint].MapMethods(
				 path.Length != routeGroupEndpoint.Length
					 ? "/"
					 : path[routeGroupEndpoint.Length..],
				 [method],
				 route.Handle
				).WithTags(["Grouped Routes"])
				: route.Configure(app.MapMethods(
					path,
					[method],
					route.Handle
				)).WithTags(["Ungrouped Routes"])
			).WithOpenApi();
		}
		private static string GetRouteGroupEndpoint(string filePath) => GetEndpointFromFilePath(filePath);
		private static (string path, string method) GetRouteEndpointAndMethod(string filePath) {
			var endpoint = GetEndpointFromFilePath(filePath);
			var method = GetMethod(Path.GetFileNameWithoutExtension(filePath));
			return (endpoint, method);
		}
		private static string GetEndpointFromFilePath(string filePath) {
			var relative = Path.GetRelativePath(RouterBaseFilePath, filePath);
			var dir = Path.GetDirectoryName(relative)!;
			var segments = dir.Split(Path.DirectorySeparatorChar);
			var route = string.Join("", segments.Select(ParseSegment));
			return route == "" ? "/" : route;
		}
		private static string ParseSegment(string segment) => segment switch {
			var s when s.StartsWith('(') && s.EndsWith(')') => "",
			var s when s.StartsWith("[...") && s.EndsWith(']') => $"/{{*{s[4..^1]}}}",
			var s when s.StartsWith('[') && s.EndsWith(']') => $"/{{{s[1..^1]}}}",
			_ => $"/{segment}"
		};
		private static string GetMethod(string fileName) {
			return fileName.ToUpper() switch {
				"GET" => "GET",
				"POST" => "POST",
				"DELETE" => "DELETE",
				"PUT" => "PUT",
				"PATCH" => "PATCH",
				"UPDATE" => "UPDATE",
				_ => "ERROR"
			};
		}
		private static void LoadRouterBaseFilePath([CallerFilePath] string routerFilePath = "") => RouterBaseFilePath = routerFilePath[..routerFilePath.LastIndexOf("/")];
	}
}
