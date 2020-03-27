using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Text;
using System.Web;

namespace Silmoon.Web
{
    public class HtmlHelper
    {
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
        public static string MakeNewQueryString(NameValueCollection collection, string additionQueryString = "")
        {
            collection = new NameValueCollection(collection);
            string s = "";
            var tp = StringHelper.AnalyzeNameValue(additionQueryString.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries), "=");

            for (int i = 0; i < tp.Count; i++)
            {
                string key = tp.GetKey(i);
                string value = tp[i];

                collection[key] = value;
            }

            for (int i = 0; i < collection.Count; i++)
            {
                string key = collection.GetKey(i);
                string value = collection[i];

                s += $"{key}={value}&";
            }
            if (s != "")
            {
                s = s.Remove(s.Length - 1);
            }
            //if (s[0] != '?')
            //    s = "?" + s;

            return s;
        }
        public static string MakeQueryString(NameValueCollection parameters)
        {
            string result = string.Empty;
            for (int i = 0; i < parameters.Count; i++)
            {
                result += "&" + HttpUtility.UrlEncode(parameters.GetKey(i)) + "=" + HttpUtility.UrlEncode(parameters[i]);
            }
            return result.Substring(1, result.Length - 1);
        }

    }
}
