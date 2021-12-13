using System;
using System.Collections.Generic;
using System.Text;

namespace Silmoon.Security
{
    /// <summary>
    /// ��ʾһ���û���ʾ����֤����
    /// </summary>
    public struct IdentityAuthInfo
    {
        /// <summary>
        /// �½�����һ����ʾ��Ϣ
        /// </summary>
        /// <param name="identityString"></param>
        /// <param name="passwordCode"></param>
        public IdentityAuthInfo(string identityString, string passwordCode)
        {
            IdentityString = identityString;
            PasswordCode = passwordCode;
        }
        /// <summary>
        /// �û���ʶ
        /// </summary>
        public string IdentityString;
        /// <summary>
        /// ������֤������
        /// </summary>
        public string PasswordCode;
    }
}
