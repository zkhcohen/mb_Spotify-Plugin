using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace SpotifyAPI.Web.Auth
{
    internal static class AuthUtil
    {
        public static void OpenBrowser(string url, bool AR)
        {
#if NETSTANDARD2_0
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
#else
            

            if (AR == true)
            {
                

                var proc = new Process();
                proc.StartInfo.FileName = "powershell.exe";
                proc.StartInfo.Arguments = $"$ie = new-object -com \"InternetExplorer.Application\"; $ie.navigate(\"\"\" {url} \"\"\"); $ie.visible = $true";
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.UseShellExecute = false;


                if (!proc.Start())
                {

                    MessageBox.Show("Powershell didn't start properly.");
                    return;

                }

                var reader = proc.StandardError;
                string line;
                while ((line = reader.ReadLine()) != null) System.Console.WriteLine(line);

                proc.Close();


            }
            else
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));

            }
#endif
        }

    }
}