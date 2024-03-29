﻿using System.Net;
using System.Net.NetworkInformation;

namespace Conectify.Shared.Services;

public static class WebFunctions
{
    public static string GetIPAdress()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
       .AddressList
       .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
       .ToString();
    }

    public static string GetMacAdress()
    {
        return NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault(string.Empty);
    }
}
