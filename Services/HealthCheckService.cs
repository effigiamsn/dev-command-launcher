using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace DevCommandLauncherApp.Services;

public static class HealthCheckService
{
    private const int PollSeconds = 2;
    private const int MaxAttempts = 15;

    public static async Task<bool> WaitUntilHealthyAsync(
        string healthUrl,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();

        for (var i = 0; i < MaxAttempts; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var response = await httpClient.GetAsync(
                    healthUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // ignore retry
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(PollSeconds), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        return false;
    }
}
