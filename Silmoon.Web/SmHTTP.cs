using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Net;

/// <summary>
/// web ��ժҪ˵��
/// </summary>
namespace Silmoon.Web
{
    /// <summary>
    /// ��UI������Ѿ���ƺõ�HTML
    /// </summary>
    public class SmHTTP
    {
        public static string ReturnShortURL()
        {
            return HttpContext.Current.Request.FilePath.ToString() + "?" + (string)HttpContext.Current.Request.ServerVariables["QUERY_STRING"];
        }
    }
}