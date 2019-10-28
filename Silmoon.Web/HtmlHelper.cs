using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.Mvc;

namespace Silmoon.Web
{
    public class HtmlHelper
    {
        /// <summary>
        /// �����Ѿ���ƺõ�asp.net������Ϣ��HTML��
        /// </summary>
        /// <param name="s">��Ϣ</param>
        /// <returns></returns>
        public static string ErrorMsgHtml(Exception exception)
        {
            string msg = "<style type=\"text/css\"><!--";
            msg += "body,td,th {font-family: Verdana, Arial, Helvetica, sans-serif;	font-size: 12px;}";
            msg += "--></style>";
            msg += "<p style=\"background:#CCCCCC\"><strong>" + exception.Message + "��</strong></p>";
            msg += "<pre>" + exception.ToString() + "</pre>";
            return msg;
        }
        public static string ErrorMsgHtml(string ErrTitle, string ErrText)
        {
            string msg = "<style type=\"text/css\"><!--";
            msg += "body,td,th {font-family: Verdana, Arial, Helvetica, sans-serif;	font-size: 12px;}";
            msg += "--></style>";
            msg += "<p style=\"background:#CCCCCC\"><strong>" + ErrTitle + "��</strong></p>";
            msg += "<p>" + ErrText + "</p>";
            return msg;
        }
        public static string ErrorMsgHtml(string ErrTitle, string ErrText, string ErrTrace)
        {
            string msg = "<style type=\"text/css\"><!--";
            msg += "body,td,th {font-family: Verdana, Arial, Helvetica, sans-serif;	font-size: 12px;}";
            msg += "--></style>";
            msg += "<p style=\"background:#CCCCCC\"><strong>" + ErrTitle + "��</strong></p>";
            msg += "<p>" + ErrText + "</p>";
            msg += "<table border=\"1\" cellpadding=\"0\" cellspacing=\"0\" bordercolor=\"#000000\" bgcolor=\"#CCCCCC\">";
            msg += "<tr><td>���٣�<br /><pre>" + ErrTrace + "</pre></td></tr></table>";
            return msg;
        }
        public static string ErrorMsgHtml(string ErrTitle, string ErrText, string ErrSrc, string ErrTrace)
        {
            string msg = "<style type=\"text/css\"><!--";
            msg += "body,td,th {font-family: Verdana, Arial, Helvetica, sans-serif;	font-size: 12px;}";
            msg += "--></style>";
            msg += "<p style=\"background:#CCCCCC\"><strong>" + ErrTitle + "��</strong></p>";
            msg += "<p>" + ErrText + "</p>";
            msg += "<table border=\"1\" cellpadding=\"0\" cellspacing=\"0\" bordercolor=\"#000000\" bgcolor=\"#CCCCCC\">";
            msg += "<tr><td>Դ��<br />" + ErrSrc + "</td></tr></table>";
            msg += "<br /><table border=\"1\" cellpadding=\"0\" cellspacing=\"0\" bordercolor=\"#000000\" bgcolor=\"#CCCCCC\">";
            msg += "<tr><td>���٣�<br /><pre>" + ErrTrace + "</pre></td></tr></table>";
            return msg;
        }
        /// <summary>
        /// �����Ѿ���ƺõ�A��ǩ��
        /// </summary>
        /// <param name="text">��ʾ������</param>
        /// <param name="url">������URL</param>
        /// <param name="target">Ŀ��</param>
        /// <returns></returns>
        public static string MakeAMark(string text, string url, string target)
        {
            return "<a href=\"" + url + "\" target=\"" + target + "\">" + text + "</a>";
        }
        /// <summary>
        /// �����Ѿ���ƺõ�A��ǩ��
        /// </summary>
        /// <param name="text">��ʾ������</param>
        /// <param name="url">������URL</param>
        /// <param name="target">Ŀ��</param>
        /// <param name="alt">Tip����</param>
        /// <returns></returns>
        public static string MakeAMark(string text, string url, string target, string alt)
        {
            return "<a href=\"" + url + "\" target=\"" + target + "\" alt=\"" + alt + "\">" + text + "</a>";
        }
        /// <summary>
        /// �����Ѿ���ƺõ�A��ǩ��
        /// </summary>
        /// <param name="text">��ʾ������</param>
        /// <param name="url">������URL</param>
        /// <returns></returns>
        public static string MakeAMark(string text, string url)
        {
            return "<a href=\"" + url + "\">" + text + "</a>";
        }
        public static string MakeImgMark(string imgUrl, string linkTo, string target)
        {
            return MakeAMark("<img border=\"0\" src=\"" + imgUrl + "\"></img>", linkTo, target);
        }

        /// <summary>
        /// ������ܵ�HTML����
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string DecodeHTMLFilter(string s)
        {
            s = s.Replace("{//LeftMark}", "<");
            s = s.Replace("{//RightMark}", ">");
            s = s.Replace("''", "'");
            s = s.Replace("{//_}", " ");
            return s;
        }
        /// <summary>
        /// ����HTML����
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string EncodeHTMLFilter(string s)
        {
            s = s.Replace("<", "{//LeftMark}");
            s = s.Replace(">", "{//RightMark}");
            s = s.Replace("'", "''");
            s = s.Replace(" ", "{//_}");
            return s;
        }
        /// <summary>
        /// ��Textarea�е�HTMLд�����ݿ��ǰ�ô���
        /// </summary>
        /// <param name="html">HTML</param>
        /// <returns></returns>
        public static string TextareaInHTML(string html)
        {
            html = html.Replace(" ", "&nbsp;");
            html = html.Replace("\r\n", "<br />");
            return html;
        }
        /// <summary>
        /// ��HTML��д��Textarea��ǰ�ô���
        /// </summary>
        /// <param name="html">HTML</param>
        /// <returns></returns>
        public static string TextareaOutHTML(string html)
        {
            html = html.Replace("&nbsp;", " ");
            html = html.Replace("<br />", "\r\n");
            return html;
        }
        public static string MakePostFormHtml(NameValueCollection values, string postTo, string name, bool submit = false)
        {
            string result = "<form method=\"post\" action=\"" + postTo + "\" name=\"" + name + "\" id=\"" + name + "\">\r\n";
            for (int i = 0; i < values.Count; i++)
            {
                result += "<input type='hidden' name='" + values.GetKey(i) + "' value='" + values[i] + "'/>\r\n";
            }
            result += "</form>\r\n";
            if (submit) result += "<script>document.forms['" + name + "'].submit();</script>";
            return result;
        }
        public static List<SelectListItem> MakeMvcSelectListItems(Type type, object enumObj)
        {
            if (type.IsEnum)
            {
                var names = type.GetEnumNames();
                var values = type.GetEnumValues();
                var result = new List<SelectListItem>();
                for (int i = 0; i < names.Length; i++)
                {
                    result.Add(new SelectListItem() { Text = names[i], Value = values.GetValue(i).ToString() });
                }
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
