using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.Collections;

namespace Silmoon.Windows
{
    /// <summary>
    /// �������û�����
    /// </summary>
    public class SAM
    {
        /// <summary>
        /// ʵ�����������û������һ��ʵ��
        /// </summary>
        public SAM()
        {

        }

        private DirectoryEntry getASMDirectoryEntryRoot()
        {
            return new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
        }
        private DirectoryEntry getUserAndGroupDirectoryEntryRoot(string identity)
        {
            return getASMDirectoryEntryRoot().Children.Find(identity);
        }

        /// <summary>
        /// ����һ���û�
        /// </summary>
        /// <param name="info">�û��˻���Ϣ�ṹ</param>
        public void CreateUser(NTUserInfo info)
        {
            DirectoryEntry rootad = getASMDirectoryEntryRoot();
            DirectoryEntry NewUser = rootad.Children.Add(info.Username, "User");
            NewUser.Invoke("SetPassword", new object[] { info.Password });
            NewUser.Invoke("Put", "UserFlags", info.UserFlags);

            NewUser.Properties["Description"].Value = info.Description;
            NewUser.Properties["Fullname"].Value = info.Fullname;

            NewUser.CommitChanges();
            NewUser.Dispose();
            rootad.Dispose();
        }
        /// <summary>
        /// ����һ���û���
        /// </summary>
        /// <param name="groupname">�û�������</param>
        /// <param name="description">�û�������</param>
        public void CreateGroup(string groupname, string description)
        {
            DirectoryEntry rootad = getASMDirectoryEntryRoot();
            DirectoryEntry NewUser = rootad.Children.Add(groupname, "Group");
            NewUser.Properties["Description"].Value = description;
            NewUser.CommitChanges();
            NewUser.Dispose();
            rootad.Dispose();
        }
        /// <summary>
        /// ɾ��һ���û�����
        /// </summary>
        /// <param name="identity">�û����������</param>
        public void DeleteUserOrGroup(string identity)
        {
            DirectoryEntry rootad = getASMDirectoryEntryRoot();
            DirectoryEntry UAD = rootad.Children.Find(identity);
            if (UAD == null) throw new NullReferenceException("ָ�����û������ڣ�");
            rootad.Children.Remove(UAD);
            rootad.Dispose();
        }

        /// <summary>
        /// ���һ���û�����
        /// </summary>
        /// <param name="username">�û��˻���</param>
        /// <param name="groupname">�û�����</param>
        public void AddUserToGroup(string username, string groupname)
        {
            if (GetIdentityType(username) == IdentityType.User)
            {
                DirectoryEntry identityRoot = getUserAndGroupDirectoryEntryRoot(groupname);
                identityRoot.Invoke("Add", getASMDirectoryEntryRoot().Children.Find(username).Path);
                identityRoot.Dispose();
            }
        }
        /// <summary>
        /// ��һ������ɾ��һ���û��˻�
        /// </summary>
        /// <param name="username">�û��˻���</param>
        /// <param name="groupname">�û�����</param>
        public void RemoveUserFromGroup(string username, string groupname)
        {
            if (GetIdentityType(username) == IdentityType.User)
            {

                DirectoryEntry identityRoot = getUserAndGroupDirectoryEntryRoot(groupname);
                identityRoot.Invoke("Remove", getASMDirectoryEntryRoot().Children.Find(username).Path);
                identityRoot.Dispose();
            }
        }

        /// <summary>
        /// ���һ���û��������Ƿ����
        /// </summary>
        /// <param name="identity">�û��ʺ�������������</param>
        /// <returns></returns>
        public bool ExistIdentity(string identity)
        {
            DirectoryEntry rootad = getASMDirectoryEntryRoot();
            foreach (DirectoryEntry ent in rootad.Children)
            {
                if (ent.SchemaClassName.ToLower() == "user" || ent.SchemaClassName.ToLower() == "group")
                {
                    if (ent.Name == identity)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ��ȡ�������е��û��˻�
        /// </summary>
        public NTUserInfo[] GetUsernames
        {
            get
            {
                DirectoryEntry rootad = getASMDirectoryEntryRoot();
                ArrayList _arr = new ArrayList();
                foreach (DirectoryEntry ent in rootad.Children)
                {
                    if (ent.SchemaClassName.ToLower() == "user")
                    {
                        NTUserInfo info = new NTUserInfo();
                        info.Username = ent.Name;
                        info.Description = (string)ent.Properties["Description"].Value;
                        info.Fullname = (string)ent.Properties["Fullname"].Value;
                        _arr.Add(info);
                    }
                }
                rootad.Dispose();
                return (NTUserInfo[])_arr.ToArray(typeof(NTUserInfo));
            }
        }
        /// <summary>
        /// ��ȡ��������������
        /// </summary>
        public string[] GetGroups
        {
            get
            {
                DirectoryEntry rootad = getASMDirectoryEntryRoot();
                ArrayList _arr = new ArrayList();
                foreach (DirectoryEntry ent in rootad.Children)
                {
                    if (ent.SchemaClassName.ToLower() == "group")
                    {
                        _arr.Add(ent.Name);
                    }
                }
                rootad.Dispose();
                return (string[])_arr.ToArray(typeof(string));
            }
        }

        /// <summary>
        /// ���һ����ʶ������
        /// </summary>
        /// <param name="identity">��ʶ</param>
        /// <returns></returns>
        public IdentityType GetIdentityType(string identity)
        {
            DirectoryEntry ent = getASMDirectoryEntryRoot().Children.Find(identity);
            if (ent.SchemaClassName.ToLower() == "user")
                return IdentityType.User;
            else if (ent.SchemaClassName.ToLower() == "group")
                return IdentityType.Group;
            return IdentityType.Unknown;
        }
    }
    /// <summary>
    /// ϵͳ�˻���Ϣ�ṹ
    /// </summary>
    public class NTUserInfo
    {
        public string Username;
        public string Password;
        public string Fullname = "";
        public string Description = "";
        public int UserFlags = 66049;
    }

    /// <summary>
    /// ��ʶ����
    /// </summary>
    public enum IdentityType
    {
        Unknown = 0,
        User = 1,
        Group = 2,
    }
}