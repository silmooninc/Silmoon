using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.AccessControl;

namespace Silmoon.IO.SmFile
{
    /// <summary>
    /// ���Ʒ��ʿ��Ʊ�
    /// </summary>
    public sealed class ACL
    {
        /// <summary>
        /// ɾ�����е�ϵͳ����Ȩ��
        /// </summary>
        /// <param name="filePath">�ļ�·��</param>
        public static void RemoveAllSystemAccessRule(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileSecurity _fs = File.GetAccessControl(filePath);
                try
                {
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("SYSTEM", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("Administrators", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("NETWORK", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("NETWORK SERVICE", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("LOCAL SERVICE", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("CREATOR OWNER", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("Users", FileSystemRights.FullControl, AccessControlType.Allow));
                }
                catch { }
                try { _fs.RemoveAccessRuleAll(new FileSystemAccessRule("Power Users", FileSystemRights.FullControl, AccessControlType.Allow)); }
                catch { }
                try { _fs.RemoveAccessRuleAll(new FileSystemAccessRule("IIS_WPG", FileSystemRights.FullControl, AccessControlType.Allow)); }
                catch { }
                try { _fs.RemoveAccessRuleAll(new FileSystemAccessRule("Guests", FileSystemRights.FullControl, AccessControlType.Allow)); }
                catch { }
                File.SetAccessControl(filePath, _fs);
            }
            else if (Directory.Exists(filePath))
            {
                DirectorySecurity _fs = Directory.GetAccessControl(filePath);
                try
                {
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("SYSTEM", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("Administrators", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("NETWORK", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("NETWORK SERVICE", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("LOCAL SERVICE", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("CREATOR OWNER", FileSystemRights.FullControl, AccessControlType.Allow));
                    _fs.RemoveAccessRuleAll(new FileSystemAccessRule("Users", FileSystemRights.FullControl, AccessControlType.Allow));
                }
                catch { }
                try { _fs.RemoveAccessRuleAll(new FileSystemAccessRule("Power Users", FileSystemRights.FullControl, AccessControlType.Allow)); }
                catch { }
                try { _fs.RemoveAccessRuleAll(new FileSystemAccessRule("IIS_WPG", FileSystemRights.FullControl, AccessControlType.Allow)); }
                catch { }
                try { _fs.RemoveAccessRuleAll(new FileSystemAccessRule("Guests", FileSystemRights.FullControl, AccessControlType.Allow)); }
                catch { }
                Directory.SetAccessControl(filePath, _fs);
            }
            else
                throw new FileNotFoundException("Ҫ�������ļ�û���ҵ�", filePath);
        }
        public static FileSecurity RemoveAllSystemAccessRule(FileSecurity fs)
        {
            try
            {
                fs.RemoveAccessRuleAll(new FileSystemAccessRule("SYSTEM", FileSystemRights.FullControl, AccessControlType.Allow));
                fs.RemoveAccessRuleAll(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
                fs.RemoveAccessRuleAll(new FileSystemAccessRule("Administrators", FileSystemRights.FullControl, AccessControlType.Allow));
                fs.RemoveAccessRuleAll(new FileSystemAccessRule("NETWORK", FileSystemRights.FullControl, AccessControlType.Allow));
                fs.RemoveAccessRuleAll(new FileSystemAccessRule("NETWORK SERVICE", FileSystemRights.FullControl, AccessControlType.Allow));
                fs.RemoveAccessRuleAll(new FileSystemAccessRule("LOCAL SERVICE", FileSystemRights.FullControl, AccessControlType.Allow));
                fs.RemoveAccessRuleAll(new FileSystemAccessRule("CREATOR OWNER", FileSystemRights.FullControl, AccessControlType.Allow));
                fs.RemoveAccessRuleAll(new FileSystemAccessRule("Users", FileSystemRights.FullControl, AccessControlType.Allow));
            }
            catch { }
            try { fs.RemoveAccessRuleAll(new FileSystemAccessRule("Power Users", FileSystemRights.FullControl, AccessControlType.Allow)); }
            catch { }
            try { fs.RemoveAccessRuleAll(new FileSystemAccessRule("IIS_WPG", FileSystemRights.FullControl, AccessControlType.Allow)); }
            catch { }
            try { fs.RemoveAccessRuleAll(new FileSystemAccessRule("Guests", FileSystemRights.FullControl, AccessControlType.Allow)); }
            catch { }
            return fs;
        }
        public static DirectorySecurity RemoveAllSystemAccessRule(DirectorySecurity ds)
        {
            try
            {
                ds.RemoveAccessRuleAll(new FileSystemAccessRule("SYSTEM", FileSystemRights.FullControl, AccessControlType.Allow));
                ds.RemoveAccessRuleAll(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
                ds.RemoveAccessRuleAll(new FileSystemAccessRule("Administrators", FileSystemRights.FullControl, AccessControlType.Allow));
                ds.RemoveAccessRuleAll(new FileSystemAccessRule("Administrator", FileSystemRights.FullControl, AccessControlType.Allow));
                ds.RemoveAccessRuleAll(new FileSystemAccessRule("NETWORK", FileSystemRights.FullControl, AccessControlType.Allow));
                ds.RemoveAccessRuleAll(new FileSystemAccessRule("NETWORK SERVICE", FileSystemRights.FullControl, AccessControlType.Allow));
                ds.RemoveAccessRuleAll(new FileSystemAccessRule("LOCAL SERVICE", FileSystemRights.FullControl, AccessControlType.Allow));
                ds.RemoveAccessRuleAll(new FileSystemAccessRule("CREATOR OWNER", FileSystemRights.FullControl, AccessControlType.Allow));
                ds.RemoveAccessRuleAll(new FileSystemAccessRule("Users", FileSystemRights.FullControl, AccessControlType.Allow));
            }
            catch { }
            try { ds.RemoveAccessRuleAll(new FileSystemAccessRule("Power Users", FileSystemRights.FullControl, AccessControlType.Allow)); }
            catch { }
            try { ds.RemoveAccessRuleAll(new FileSystemAccessRule("IIS_WPG", FileSystemRights.FullControl, AccessControlType.Allow)); }
            catch { }
            try { ds.RemoveAccessRuleAll(new FileSystemAccessRule("Guests", FileSystemRights.FullControl, AccessControlType.Allow)); }
            catch { }
            return ds;
        }

        /// <summary>
        /// �����ļ��̳�Ȩ�ޱ���
        /// </summary>
        /// <param name="filePath">Ŀ���ļ�����Ŀ¼</param>
        /// <param name="isProtected">�Ƿ��ܱ�����</param>
        /// <param name="preserveInheritance">�Ƿ�������</param>
        public static void SetProtectionRule(string filePath,bool isProtected, bool preserveInheritance)
        {
            if (File.Exists(filePath))
            {
                FileSecurity fs = File.GetAccessControl(filePath);
                fs.SetAccessRuleProtection(isProtected, preserveInheritance);
                File.SetAccessControl(filePath, fs);
            }
            else if (Directory.Exists(filePath))
            {
                DirectorySecurity ds = Directory.GetAccessControl(filePath);
                ds.SetAccessRuleProtection(isProtected, preserveInheritance);
                Directory.SetAccessControl(filePath, ds);
            }
            else
                throw new FileNotFoundException("Ҫ�������ļ�û���ҵ�", filePath);
        }

        /// <summary>
        /// ��ӱ��µ�·����ȫ����
        /// </summary>
        /// <param name="filePath">�ļ�·��</param>
        /// <param name="identity">����</param>
        public static void AddAccessRule(string filePath, string identity)
        {
            if (File.Exists(filePath))
            {
                FileSecurity _fs = File.GetAccessControl(filePath);
                _fs.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(filePath, _fs);
            }
            else if (Directory.Exists(filePath))
            {
                DirectorySecurity _fs = Directory.GetAccessControl(filePath);
                _fs.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                _fs.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                Directory.SetAccessControl(filePath, _fs);
            }
            else throw new FileNotFoundException("Ҫ�������ļ�û���ҵ�", filePath);
        }
        /// <summary>
        /// ���һ���û�����Ȩ�޵�Ŀ¼
        /// </summary>
        /// <param name="ds">Ŀ¼��ȫ����</param>
        /// <param name="identity">�û���ʶ</param>
        /// <param name="rights">Ȩ��</param>
        /// <param name="Inhert">�Ƿ�Ӧ�õ��Ӷ���</param>
        /// <returns></returns>
        public static DirectorySecurity AddAccessRule(DirectorySecurity ds, string identity, FileSystemRights rights, bool Inhert)
        {
            ds.AddAccessRule(new FileSystemAccessRule(identity, rights, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
            if (Inhert) ds.AddAccessRule(new FileSystemAccessRule(identity, rights, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            return ds;
        }

        /// <summary>
        /// ɾ��ָ���û���ACL
        /// </summary>
        /// <param name="identity">Windows�ʻ�</param>
        /// <param name="filePath">�ļ�·��</param>
        public static void RemoveAccessRule(string filePath, string identity)
        {
            if (File.Exists(filePath))
            {
                FileSecurity _fs = File.GetAccessControl(filePath);
                _fs.RemoveAccessRuleAll(new FileSystemAccessRule(identity, FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(filePath, _fs);
            }
            else if (Directory.Exists(filePath))
            {
                DirectorySecurity _fs = Directory.GetAccessControl(filePath);
                _fs.RemoveAccessRuleAll(new FileSystemAccessRule(identity, FileSystemRights.FullControl, AccessControlType.Allow));
                Directory.SetAccessControl(filePath, _fs);
            }
            else throw new FileNotFoundException("Ҫ�������ļ�û���ҵ�", filePath);
        }
        /// <summary>
        /// ɾ��ָ�����µ�Ŀ¼��ȫ
        /// </summary>
        /// <param name="ds">Ŀ¼��ȫʵ��</param>
        /// <param name="identity">����</param>
        /// <returns></returns>
        public static DirectorySecurity RemoveAccessRule(DirectorySecurity ds, string identity)
        {
            ds.RemoveAccessRuleAll(new FileSystemAccessRule(identity, FileSystemRights.FullControl, AccessControlType.Allow));
            return ds;
        }
    }
}