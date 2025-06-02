using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace wow_addon_backuper;

class Util
{
    public static void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        Environment.Exit(0);
    }
}