using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace Silmoon.Data.SqlClient
{
    public class SmMSSQLClientSource : IDisposable
    {
        SqlConnection conn = new SqlConnection();
        SmMSSQLClient source = new SmMSSQLClient();

        public SmMSSQLClient DataSource
        {
            get { return source; }
            set { source = value; }
        }
        public SqlConnection Connection
        {
            get { return conn; }
            set { conn = value; }
        }

        public SmMSSQLClientSource()
        {
            source.Connection = conn;
        }

        #region IDisposable ��Ա

        public void Dispose()
        {
            Close();
            conn = null;
        }

        #endregion

        /// <summary>
        /// ʵ�����������ͺ�����Դ
        /// </summary>
        /// <param name="open">�Ƿ���ʵ����ʱ������ݿ�</param>
        /// <param name="conStr">ָ���������ݿ�����ݿ������ַ���</param>
        public void InitData(bool open, string conStr)
        {
            conn.ConnectionString = conStr;
            if (open) Open();
        }
        public bool Open()
        {
            if (conn.State == System.Data.ConnectionState.Open) return false;
            conn.Open();
            return true;
        }
        public bool Close()
        {
            if (conn.State == System.Data.ConnectionState.Closed) return false;
            conn.Close();
            return true;
        }
    }
}