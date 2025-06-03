using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace wow_addon_backuper;

class Util
{
    public static void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        Environment.Exit(0);
    }

    public static async Task WriteToFile(string filename, byte[] bytes)
    {
        var dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/wow-addon-backuper";
        Directory.CreateDirectory(dir);
        var file = $"{dir}/{filename}";
        using (var fs = File.Open(file, File.Exists(file) ? FileMode.Truncate : FileMode.CreateNew))
        {
            await fs.WriteAsync(bytes, 0, bytes.Length);
        }
    }

    public static async Task<byte[]?> ReadFromFile(string filename)
    {
        var dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/wow-addon-backuper";
        var file = $"{dir}/{filename}";
        if (!File.Exists(file)) return null;

        var finfo = new FileInfo(file);
        byte[] buffer = new byte[finfo.Length];
        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, buffer.Length, true))
        {
            while (true)
            {
                if ((await fs.ReadAsync(buffer)) <= 0)
                    break;
            }
        }
        return buffer;
    }

    public static void DeleteFile(string filename)
    {
        var dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/wow-addon-backuper";
        var file = $"{dir}/{filename}";
        if (!File.Exists(file)) return;

        File.Delete(file);
    }
}