using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Odbc;
using System.Data;

namespace Silmoon.Data.Odbc
{
    public class SmOdbcClient : SqlCommonTemplate,IDisposable,ISMSQL
    {
        OdbcConnection con = new OdbcConnection();
        string conStr;
        bool isConnect;
        int selectCommandTimeout = 30;

        /// <summary>
        /// ��ȡ�����Ƿ��Ѿ��������ݿ����
        /// </summary>
        public ConnectionState State
        {
            get { return con.State; }
        }

        /// <summary>
        /// ����ODBC����Դ��ʵ��
        /// </summary>
        /// <param name="constr">�����ַ���</param>
        public SmOdbcClient(string constr)
        {
            isConnect = false;
            conStr = constr;
            InitClass();
        }

        private void InitClass()
        {

        }

        #region IDisposable ��Ա

        public void Dispose()
        {
            Close();
            con.Dispose();
            con = null;
        }

        #endregion

        #region ISMSQL ��Ա

        /// <summary>
        /// �ر����ݿ����Ӳ����ͷ����Ӷ���
        /// </summary>
        public void Close()
        {
            if (State != ConnectionState.Closed)
            {
                con.Close();
                isConnect = false;
            }
        }
        /// <summary>
        /// ʹ��Ĭ�����Ӳ��Ҵ�һ�����ݿ�
        /// </summary>
        public void Open()
        {
            if (State == ConnectionState.Closed)
            {
                con.ConnectionString = conStr;
                con.Open();
                isConnect = true;
            }
        }
        /// <summary>
        /// ��ʹ��������������ʱ��ִ��SELECT��ѯ�ĳ�ʱʱ�䡣
        /// </summary>
        public int SelectCommandTimeout
        {
            get { return selectCommandTimeout; }
            set { selectCommandTimeout = value; }
        }
        public string Connectionstring
        {
            get { return conStr; }
            set { conStr = value; }
        }
        /// <summary>
        /// ִ��һ��û�з��ػ���Ҫ���ص�SQL�����ҷ�����Ӧ����
        /// </summary>
        /// <returns></returns>
        public int ExecNonQuery(string sqlcommand)
        {
            int reint = 0;
            if (isConnect)
            {
                OdbcCommand myCmd = new OdbcCommand(__chkSqlstr(sqlcommand), con);
                reint = myCmd.ExecuteNonQuery();
                myCmd.Dispose();
            }
            else
            { throw new Exception("���ݿ�û������"); }
            return reint;
        }
        /// <summary>
        /// �������ݽ������
        /// </summary>
        /// <param name="sqlcommand">��ѯ���</param>
        /// <returns></returns>
        public int GetRecordCount(string sqlcommand)
        {
            DataTable dt = GetDataTable(sqlcommand);
            int i = dt.Rows.Count;
            dt.Dispose();
            return i;
        }
        /// <summary>
        /// ����һ��SqlDataReader����
        /// </summary>
        /// <param name="sqlcommand">SQL����</param>
        /// <returns></returns>
        public object GetDataReader(string sqlcommand)
        {
            return new OdbcCommand(__chkSqlstr(sqlcommand), con).ExecuteReader();
        }
        /// <summary>
        /// ����һ��SqlCommand����
        /// </summary>
        /// <param name="sqlcommand">SQL����</param>
        /// <returns></returns>
        public object GetCommand(string sqlcommand)
        {
            return new OdbcCommand(__chkSqlstr(sqlcommand), con);
        }
        /// <summary>
        /// ��ȡһ��������������
        /// </summary>
        /// <param name="sqlcommand">SQL���</param>
        /// <returns></returns>
        public object GetDataAdapter(string sqlcommand)
        {
            return new OdbcDataAdapter(__chkSqlstr(sqlcommand), con);
        }
        /// <summary>
        /// ��ȡһ���ڴ����ݱ�
        /// </summary>
        /// <param name="sqlcommand">SQL����</param>
        /// <returns></returns>
        public DataTable GetDataTable(string sqlcommand)
        {
            DataTable dt = new DataTable();
            OdbcDataAdapter da = (OdbcDataAdapter)GetDataAdapter(sqlcommand);
            da.SelectCommand.CommandTimeout = selectCommandTimeout;
            da.Fill(dt);
            da.Dispose();
            return dt;
        }

        /// <summary>
        /// ����һ�������ݿ������ѯ�������ֶ�ֵ
        /// </summary>
        /// <param name="tablename">��</param>
        /// <param name="resulefield">�ֶ�</param>
        /// <param name="fieldname">�����ֶ�</param>
        /// <param name="fieldvalue">����ֵ</param>
        /// <returns></returns>
        public object GetFieldObjectForSingleQuery(string tablename, string resulefield, string fieldname, string fieldvalue)
        {
            object reobj;
            OdbcDataReader dr = (OdbcDataReader)GetDataReader("select " + resulefield + " from [" + tablename + "] where " + fieldname + " = " + fieldvalue);
            if (dr.Read())
            { reobj = dr[0]; }
            else
            {
                Close();
                reobj = null;
            }
            dr.Close();
            dr.Dispose();
            return reobj;
        }
        public object GetFieldObjectForSingleQuery(string sqlcommand, bool isUseReader)
        {
            if (isUseReader) return GetFieldObjectForSingleQuery(sqlcommand);
            else
            {
                DataTable dt = GetDataTable(sqlcommand);
                object returnObj = null;
                if (dt.Rows.Count != 0)
                    returnObj = dt.Rows[0][0];
                dt.Clear();
                dt.Dispose();
                return returnObj;
            }
        }
        /// <summary>
        /// ����һ�������ݿ������ѯ�������ֶ�ֵ
        /// </summary>
        /// <param name="sqlcommand">SQL�������ָ��һ�������ֶ�</param>
        /// <returns></returns>
        public object GetFieldObjectForSingleQuery(string sqlcommand)
        {
            object reobj;
            OdbcDataReader dr = (OdbcDataReader)GetDataReader(sqlcommand);
            if (dr.Read())
            { reobj = dr[0]; }
            else
            { reobj = null; }
            dr.Close();
            dr.Dispose();
            return reobj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="updatefield"></param>
        /// <param name="updatevalue"></param>
        /// <param name="fieldname"></param>
        /// <param name="fieldvalue"></param>
        public int UpdateFieldForSingleQuery(string tablename, string updatefield, string updatevalue, string fieldname, string fieldvalue)
        {
            return ExecNonQuery("Update [" + tablename + "] set " + updatefield + " = " + updatevalue + " where " + fieldname + " = " + fieldvalue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlcommand"></param>
        /// <returns></returns>
        public bool ExistRecord(string sqlcommand)
        {
            bool rebool = false;
            OdbcDataReader dr = (OdbcDataReader)GetDataReader(sqlcommand);
            if (dr.Read())
            { rebool = true; }
            else { rebool = false; }
            dr.Close();
            return rebool;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlcommand"></param>
        /// <param name="fieldname"></param>
        /// <returns></returns>
        public string ExistRecord(string sqlcommand, string fieldname)
        {
            string restring = "";
            OdbcDataReader dr = (OdbcDataReader)GetDataReader(sqlcommand);
            if (dr.Read())
            { restring = dr[fieldname].ToString(); }
            else { restring = null; }
            dr.Close();
            return restring;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object GetConnection()
        {
            return con;
        }

        public string __chkSqlstr(string sqlcommand)
        {
            //return SqlCommonTemplate.GetSecuritySqlString(sqlcommand);
            return sqlcommand;
        }
        #endregion

    }
}
