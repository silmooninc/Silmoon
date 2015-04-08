using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;

namespace Silmoon.Data.SqlClient
{
    public class SmMSSQLClient : SqlCommonTemplate,IDisposable,ISMSQL
    {
        SqlConnection con = null;

        string conStr;
        int selectCommandTimeout = 30;

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
        public SqlConnection Connection
        {
            get { return con; }
            set { con = value; }
        }

        /// <summary>
        /// ��ȡSQL������״̬
        /// </summary>
        public ConnectionState State
        {
            get { return con.State; }
        }

        /// <summary>
        /// ����MS SQL����Դ��ʵ��
        /// </summary>
        /// <param name="constr">�����ַ���</param>
        public SmMSSQLClient()
        {
            con = new SqlConnection();
        }
        /// <summary>
        /// ����MS SQL����Դ��ʵ��
        /// </summary>
        /// <param name="constr">�����ַ���</param>
        public SmMSSQLClient(string constr)
        {
            con = new SqlConnection();
            conStr = constr;
        }
        /// <summary>
        /// ����MS SQL����Դ��ʵ��
        /// </summary>
        /// <param name="constr">�����ַ���</param>
        public SmMSSQLClient(SqlConnection conn)
        {
            con = conn;
        }


        /// <summary>
        /// �ر����ݿ����Ӳ����ͷ����Ӷ���
        /// </summary>
        public void Close()
        {
            if (State != ConnectionState.Closed)
            {
                con.Close();
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
            }
        }

        /// <summary>
        /// ִ��һ��û�з��ػ���Ҫ���ص�SQL�����ҷ�����Ӧ����
        /// </summary>
        /// <returns></returns>
        public int ExecNonQuery(string sqlcommand)
        {
            int reint = 0;
            SqlCommand myCmd = new SqlCommand(__chkSqlstr(sqlcommand), con);
            reint = myCmd.ExecuteNonQuery();
            myCmd.Dispose();
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
            return new SqlCommand(__chkSqlstr(sqlcommand), con).ExecuteReader();
        }
        /// <summary>
        /// ����һ��SqlCommand����
        /// </summary>
        /// <param name="sqlcommand">SQL����</param>
        /// <returns></returns>
        public object GetCommand(string sqlcommand)
        {
            return new SqlCommand(__chkSqlstr(sqlcommand), con);
        }
        /// <summary>
        /// ��ȡһ��������������
        /// </summary>
        /// <param name="sqlcommand">SQL���</param>
        /// <returns></returns>
        public object GetDataAdapter(string sqlcommand)
        {
            return new SqlDataAdapter(__chkSqlstr(sqlcommand), con);
        }
        /// <summary>
        /// ��ȡһ���ڴ����ݱ�
        /// </summary>
        /// <param name="sqlcommand">SQL����</param>
        /// <returns></returns>
        public DataTable GetDataTable(string sqlcommand)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da = (SqlDataAdapter)GetDataAdapter(sqlcommand);
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
            SqlDataReader dr = (SqlDataReader)GetDataReader("select " + resulefield + " from [" + tablename + "] where " + fieldname + " = " + fieldvalue);
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
        /// <summary>
        /// ����һ�������ݿ������ѯ�������ֶ�ֵ
        /// </summary>
        /// <param name="sqlcommand">SQL��ѯ����</param>
        /// <param name="isUseReader">�Ƿ�ʹ��DataReader���й���</param>
        /// <returns></returns>
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
            SqlDataReader dr = (SqlDataReader)GetDataReader(sqlcommand);
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
            SqlDataReader dr = (SqlDataReader)GetDataReader(sqlcommand);
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
            SqlDataReader dr = (SqlDataReader)GetDataReader(sqlcommand);
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


        public void Dispose()
        {
            Close();
            con.Close();
            con.Dispose();
            con = null;
        }

        public string __chkSqlstr(string sqlcommand)
        {
            //HttpContext.Current.Response.Write(sqlcommand);
            return sqlcommand;
        }
    }
    public sealed class SmOleDb : IDisposable
    {
        string sqlCommand;
        OleDbConnection con;
        string conStr;
        bool isConnect;

        /// <summary>
        /// ��ȡ�����Ƿ��Ѿ��������ݿ����
        /// </summary>
        public bool Isconnect
        {
            get
            {
                return isConnect;
            }
        }

        /// <summary>
        /// ��Ĺ��캯��
        /// </summary>
        public SmOleDb(string constr)
        {
            isConnect = false;
            conStr = constr;
            InitClass();
        }

        private void InitClass()
        {

        }

        /// <summary>
        /// �ر����ݿ����Ӳ����ͷ����Ӷ���
        /// </summary>
        public void Close()
        {
            con.Close();
            con = null;
            isConnect = false;
        }
        /// <summary>
        /// ʹ��Ĭ�����Ӳ��Ҵ�һ�����ݿ�
        /// </summary>
        public void Open()
        {
            con = new OleDbConnection(conStr);
            con.Open();
            isConnect = true;
        }


        /// <summary>
        /// ���û��߻�ȡSQLִ�����
        /// </summary>
        public string SqlCommand
        {
            get { return sqlCommand; }
            set { sqlCommand = value; }
        }
        /// <summary>
        /// ִ��һ��û�з��ػ���Ҫ���ص�SQL�����ҷ�����Ӧ����
        /// </summary>
        /// <returns></returns>
        public int ExecNonQuery()
        {
            int reint = 0;
            if (isConnect)
            {
                OleDbCommand myCmd = new OleDbCommand(sqlCommand, con);
                reint = myCmd.ExecuteNonQuery();
                myCmd.Dispose();
            }
            else
            {
                throw new Exception("���ݿ�û������");
            }
            return reint;
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
                OleDbCommand myCmd = new OleDbCommand(sqlcommand, con);
                reint = myCmd.ExecuteNonQuery();
                myCmd.Dispose();
            }
            else
            {
                throw new Exception("���ݿ�û������");
            }
            return reint;
        }

        /// <summary>
        /// ����SQL��ѯSqlDataReader���ͽ����
        /// </summary>
        /// <returns></returns>
        public OleDbDataReader GetOleDbDataReader()
        {
            return new OleDbCommand(sqlCommand, con).ExecuteReader();
        }
        public OleDbDataReader GetOleDbDataReader(string sqlcommand)
        {
            return new OleDbCommand(sqlcommand, con).ExecuteReader();
        }
        public OleDbCommand GetOleDbCommand(string sqlcommand)
        {
            return new OleDbCommand(sqlcommand, con);
        }
        public object GetFieldObjectForSingleQuery(string tablename, string resulefield, string fieldname, string fieldvalue)
        {
            object reobj;
            OleDbDataReader dr = GetOleDbDataReader("select " + resulefield + " from [" + tablename + "] where " + fieldname + " = " + fieldvalue);
            if (dr.Read())
            { reobj = dr[0]; }
            else
            {
                Close();
                throw new Exception("�޷�������");
            }
            dr.Close();
            dr.Dispose();
            return reobj;
        }
        public void UpdateFieldForSingleQuery(string tablename, string updatefield, string updatevalue, string fieldname, string fieldvalue)
        {
            ExecNonQuery("Update [" + tablename + "] set " + updatefield + " = " + updatevalue + " where " + fieldname + " = " + fieldvalue);
        }
        public bool ExistRecord(string sqlcommand)
        {
            bool rebool = false;
            OleDbDataReader dr = GetOleDbDataReader(sqlcommand);
            if (dr.Read())
            { rebool = true; }
            else { rebool = false; }
            dr.Close();
            return rebool;
        }
        public string ExistRecord(string sqlcommand, string fieldname)
        {
            string restring = "";
            OleDbDataReader dr = GetOleDbDataReader(sqlcommand);
            if (dr.Read())
            { restring = dr[fieldname].ToString(); }
            else { restring = null; }
            dr.Close();
            return restring;
        }
        public OleDbDataAdapter GetOleDbAdapter(string sqlcommand)
        {
            return new OleDbDataAdapter(sqlcommand, con);
        }
        /// <summary>
        /// ��ȡһ���ڴ����ݱ�
        /// </summary>
        /// <param name="sqlcommand">SQL����</param>
        /// <returns></returns>
        public DataTable GetOleDbDataTable(string sqlcommand)
        {
            DataTable dt = new DataTable();
            OleDbDataAdapter da = GetOleDbAdapter(sqlcommand);
            da.Fill(dt);
            da.Dispose();
            return dt;
        }
        public OleDbConnection GetOleDbConnection()
        {
            return con;
        }

        public static string Constr(string mdbPath)
        {
            return @"Provider=Microsoft.Jet.OLEDB.4.0;Data source= " + mdbPath;
        }

        public void Dispose()
        {
            GC.Collect();
        }
    }
    public sealed class SmOracleClient : IDisposable
    {
        string sqlCommand;
        OracleConnection con;
        string conStr;
        bool isConnect;


        /// <summary>
        /// ��ȡ�����Ƿ��Ѿ��������ݿ����
        /// </summary>
        public bool Isconnect
        {
            get
            {
                return isConnect;
            }
        }

        /// <summary>
        /// ��Ĺ��캯��
        /// </summary>
        public SmOracleClient(string constr)
        {
            isConnect = false;
            conStr = constr;
            InitClass();
        }

        private void InitClass()
        {

        }

        /// <summary>
        /// �ر����ݿ����Ӳ����ͷ����Ӷ���
        /// </summary>
        public void Close()
        {
            con.Close();
            con = null;
            isConnect = false;
        }
        /// <summary>
        /// ʹ��Ĭ�����Ӳ��Ҵ�һ�����ݿ�
        /// </summary>
        public void Open()
        {
            con = new OracleConnection(conStr);
            con.Open();
            isConnect = true;
        }


        /// <summary>
        /// ���û��߻�ȡSQLִ�����
        /// </summary>
        public string SqlCommand
        {
            get { return sqlCommand; }
            set { sqlCommand = value; }
        }
        /// <summary>
        /// ִ��һ��û�з��ػ���Ҫ���ص�SQL�����ҷ�����Ӧ����
        /// </summary>
        /// <returns></returns>
        public int ExecNonQuery()
        {
            int reint = 0;
            if (isConnect)
            {
                OracleCommand myCmd = new OracleCommand(sqlCommand, con);
                reint = myCmd.ExecuteNonQuery();
                myCmd.Dispose();
            }
            else
            {
                throw new Exception("���ݿ�û������");
            }
            return reint;
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
                OracleCommand myCmd = new OracleCommand(sqlcommand, con);
                reint = myCmd.ExecuteNonQuery();
                myCmd.Dispose();
            }
            else
            {
                throw new Exception("���ݿ�û������");
            }
            return reint;
        }
        /// <summary>
        /// ����SQL��ѯSqlDataReader���ͽ����
        /// </summary>
        /// <returns></returns>
        public OracleDataReader SqlDataRead()
        {
            return new OracleCommand(sqlCommand, con).ExecuteReader();
        }
        public OracleDataReader SqlDataRead(string sqlcommand)
        {
            return new OracleCommand(sqlcommand, con).ExecuteReader();
        }
        public OracleCommand SqlCommander(string sqlcommand)
        {
            return new OracleCommand(sqlcommand, con);
        }
        public object GetFieldObjectForSingleQuery(string tablename, string resulefield, string fieldname, string fieldvalue)
        {
            object reobj;
            OracleDataReader dr = SqlDataRead("select " + resulefield + " from [" + tablename + "] where " + fieldname + " = " + fieldvalue);
            if (dr.Read())
            { reobj = dr[0]; }
            else
            {
                Close();
                throw new Exception("�޷�������");
            }
            dr.Close();
            dr.Dispose();
            return reobj;
        }
        public void UpdateFieldForSingleQuery(string tablename, string updatefield, string updatevalue, string fieldname, string fieldvalue)
        {
            ExecNonQuery("Update [" + tablename + "] set " + updatefield + " = " + updatevalue + " where " + fieldname + " = " + fieldvalue);
        }
        public bool ExSqlRecordSet(string sqlcommand)
        {
            bool rebool = false;
            OracleDataReader dr = SqlDataRead(sqlcommand);
            if (dr.Read())
            { rebool = true; }
            else { rebool = false; }
            dr.Close();
            return rebool;
        }
        public OracleDataAdapter DataAdapter(string sqlString)
        {
            return new OracleDataAdapter(sqlString, con);
        }


        public static string Constr(string mdbPath)
        {
            return @"Provider=Microsoft.Jet.OLEDB.4.0;Data source= " + mdbPath;
        }


        public void Dispose()
        {
            GC.Collect();
        }
    }
}
