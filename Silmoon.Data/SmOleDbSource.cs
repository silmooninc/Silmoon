using System;
using System.Collections.Generic;
using System.Text;
using Silmoon.Data.SqlClient;

namespace Silmoon.Data
{
    /// <summary>
    /// �����Լ�������������ʹ��SmSqlClient����Դ�Ŀ��������á�
    /// </summary>
    public abstract class SmOleDbSource : IDisposable
    {
        SmOleDb _ssc;
        /// <summary>
        /// ���û��ȡ����Դ
        /// </summary>
        public SmOleDb DataSource
        {
            get { return _ssc; }
            set { _ssc = value; }
        }
        /// <summary>
        /// �ر���������
        /// </summary>
        public void Close()
        {
            _ssc.Close();
        }

        #region IDisposable ��Ա

        public void Dispose()
        {
            _ssc.Dispose();
        }

        #endregion

        /// <summary>
        /// ʵ�����������ͺ�����Դ
        /// </summary>
        /// <param name="open">�Ƿ���ʵ����ʱ������ݿ�</param>
        /// <param name="conStr">ָ���������ݿ�����ݿ������ַ���</param>
        public void InitData(bool open, string conStr)
        {
            _ssc = new SmOleDb(conStr);
            if (open) { _ssc.Open(); }
        }

    }
}
