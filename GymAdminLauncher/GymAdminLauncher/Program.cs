using System.Diagnostics;
using System.Net.Http;

namespace GymAdminLauncher;

class Program
{
    private const string ApiUrl = "http://localhost:5000";
    private const string BackendPath = "backend/GymAdmin.Api.exe";
    private const string ApiProcessName = "GymAdmin.Api";

    static async Task Main(string[] args)
    {
        // 1. Matar procesos previos si existen
        KillProcesses(ApiProcessName);

        // 2. Iniciar el Backend
        if (File.Exists(BackendPath))
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = BackendPath,
                UseShellExecute = false,
                CreateNoWindow = true, // Ocultar consola del backend
                WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(BackendPath))
            };
            Process.Start(startInfo);
        }

        // 3. Esperar a que el puerto esté abierto
        // Como el backend sirve el front casi de inmediato (antes del seed),
        // el usuario verá el spinner que pusimos en index.html rápidamente.
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(2);
        
        bool isReady = false;
        int attempts = 0;
        const int maxAttempts = 30;

        while (!isReady && attempts < maxAttempts)
        {
            try
            {
                // Solo nos importa si el servidor responde algo (200, 404, etc)
                var response = await client.GetAsync(ApiUrl);
                isReady = true;
            }
            catch
            {
                attempts++;
                await Task.Delay(500);
            }
        }

        // 4. Abrir el navegador
        // Usamos forceLogout=true para asegurar que siempre inicie en el login
        OpenBrowser($"{ApiUrl}?forceLogout=true");
    }

    private static void KillProcesses(string name)
    {
        var processes = Process.GetProcessesByName(name);
        foreach (var p in processes)
        {
            try { p.Kill(); p.WaitForExit(); } catch { }
        }
    }

    private static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Fallback para abrir con 'start' en Windows
            Process.Start("cmd", $"/c start {url}");
        }
    }
}
