using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace DevCommandLauncherApp.Services;

public static class PortChecker
{
    public static async Task<bool> IsPortInUseAsync(
        int port,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var tcpClient = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(700);

            await tcpClient.ConnectAsync("127.0.0.1", port, cts.Token).ConfigureAwait(false);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}
