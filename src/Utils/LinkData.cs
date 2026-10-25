using ChariotSanzzo.Infrastructure.Config;

namespace ChariotSanzzo.Utils {
	public static class LinkData {
		public static string GetChariotApiFullAddress(string? endpoint = null) {
			if (endpoint != null)
				return AddressesConfig.ChariotApiAddress + endpoint;
			return AddressesConfig.ChariotApiAddress;
		}
		public static string GetAlbinaApiFullAddress(string? endpoint = null) {
			if (endpoint != null)
				return AddressesConfig.AlbinaApiAddress + endpoint;
			return AddressesConfig.AlbinaApiAddress;
		}
		public static string GetAlbinaSiteFullAddress(string? endpoint = null) {
			if (endpoint != null)
				return AddressesConfig.AlbinaSiteAddress + endpoint;
			return AddressesConfig.AlbinaSiteAddress;
		}
		public static string GetGjallarhornControlFullAddress(string? endpoint = null) {
			if (endpoint != null)
				return AddressesConfig.GjallarhornControlAdress + endpoint;
			return AddressesConfig.GjallarhornControlAdress;
		}
	}
}
