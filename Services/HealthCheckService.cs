using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace DevCommandLauncherApp.Services;

public static class HealthCheckService
{
    private const int PollSeconds = 2;
    private const int MaxAttempts = 15;
    private static readonly HttpClient HttpClient = new();

    public static async Task<bool> IsReachableAsync(
        string url,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            using var response = await HttpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                timeoutCts.Token).ConfigureAwait(false);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> WaitUntilHealthyAsync(
        string healthUrl,
        CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < MaxAttempts; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var response = await HttpClient.GetAsync(
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
