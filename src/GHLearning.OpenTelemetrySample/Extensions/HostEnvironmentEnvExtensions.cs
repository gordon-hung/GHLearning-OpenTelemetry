namespace GHLearning.OpenTelemetrySample.Extensions;

public static class HostEnvironmentEnvExtensions
{
	/// <summary>
	/// Checks if the current host environment name is <see cref="Environments.Development"/>.
	/// </summary>
	/// <param name="hostEnvironment">An instance of <see cref="IHostEnvironment"/>.</param>
	/// <returns><see langword="true"/> if the environment name is <see cref="Environments.Development"/>, otherwise <see langword="false"/>.</returns>
	public static bool IsDev(this IHostEnvironment hostEnvironment)
	{

		return hostEnvironment.IsEnvironment("dev");
	}
}
