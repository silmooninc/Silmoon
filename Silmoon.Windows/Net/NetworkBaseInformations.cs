using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;
using System.Management;
using Silmoon.Net.Sockets;

namespace Silmoon.Windows.Net.NetworkInformation
{
    public class NetworkBaseInformations
    {
        [DllImport("Iphlpapi.dll")]
        static extern int SendARP(int DestIP, int SrcIP, ref MacAddress MacAddr, ref int PhyAddrLen);

        [DllImport("Ws2_32.dll")]
        static extern Int32 inet_addr(string ipaddr);

        /// <summary>
        /// 获取一个IP地址的MAC地址
        /// </summary>
        /// <param name="ip">本地网的IP地址</param>
        /// <returns></returns>
        public static MacAddress GetMacAddress(IPAddress ip)
        {
            MacAddress result = new MacAddress();
            int length = 6;
            int remote = inet_addr(ip.ToString());
            SendARP(remote, 0, ref result, ref length);
            return result;

        }
        ///// <summary>
        ///// 获取一个ip地址的mac地址
        ///// </summary>
        ///// <param name="macip">目标ip地址</param>
        ///// <param name="formatstr">mac地址格式连接字符串</param>
        ///// <returns></returns>
        //public static string GetMacAddressStr(IPAddress ip, string formatstr)
        //{
        //    StringBuilder strReturn = new StringBuilder();
        //    try
        //    {
        //        Int32 remote = inet_addr(ip.ToString());

        //        MacAddress macinfo = new MacAddress();
        //        Int32 length = 6;
        //        SendARP(remote, 0, ref macinfo, ref length);

        //        string temp = System.Convert.ToString(macinfo, 16).PadLeft(12, '0').ToUpper();

        //        int x = 12;
        //        for (int i = 0; i < 6; i++)
        //        {
        //            if (i == 5) { strReturn.Append(temp.Substring(x - 2, 2)); }
        //            else { strReturn.Append(temp.Substring(x - 2, 2) + formatstr); }
        //            x -= 2;
        //        }
        //        return strReturn.ToString();
        //    }
        //    catch
        //    { return strReturn.ToString(); }
        //}
        /// <summary>
        /// 返回本地默认网关
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetDefaultGateWay()
        {
            string ip = string.Empty;
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection nics = mc.GetInstances();

                foreach (ManagementObject nic in nics)
                {
                    if (Convert.ToBoolean(nic["ipEnabled"]) == true)
                    {
                        //Console.WriteLine((nic["IPAddress"] as String[])[0]);
                        //Console.WriteLine((nic["IPSubnet"] as String[])[0]);
                        ip = (nic["DefaultIPGateway"] as String[])[0];
                    }

                }
                nics.Dispose();
                mc.Dispose();
            }
            catch { ip = "0.0.0.0"; }
            return IPAddress.Parse(ip);
        }

    }
}
