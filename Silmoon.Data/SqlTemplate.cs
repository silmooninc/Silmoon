using System;
using System.Collections.Generic;
using System.Text;

namespace Silmoon.Data
{
    /// <summary>
    /// ���õ�SQL����Դģ��
    /// </summary>
    public class SqlCommonTemplate
    {
        /// <summary>
        /// ���SQL�ֶ��ַ����Ƿ��ǰ�ȫ�ġ�
        /// </summary>
        /// <param name="sqlString">SQL�ֶ��ַ���</param>
        /// <returns></returns>
        public static bool CheckSqlStringSecurity(string sqlString)
        {
            sqlString = sqlString.Replace("''", "");
            return !(sqlString.IndexOf("'") != -1);
        }
        /// <summary>
        /// ���SQL�ֶ��ַ����Ƿ��ǰ�ȫ�ġ�
        /// </summary>
        /// <param name="sqlString">SQL�ֶ��ַ���</param>
        /// <returns></returns>
        public static string GetSecuritySqlString(string sqlString)
        {
            if (CheckSqlStringSecurity(sqlString))
                throw new System.Security.SecurityException("��⵽����Σ�յ�SQL��ѯ��䡣");
            else return sqlString;
        }
        /// <summary>
        /// ���˺͸���SQLΣ���ַ���������ǰ�����ԭ���Ƿ�ȫ��ȫ��ǿ�й��ˡ�
        /// </summary>
        /// <param name="sqlString">SQL�ֶ��ַ���</param>
        /// <returns></returns>
        public string InjectFieldReplace(string sqlString)
        {
            if (sqlString == null) return sqlString;
            return sqlString.Replace("'", "''");
        }
    }
}
