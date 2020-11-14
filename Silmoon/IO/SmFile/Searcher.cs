using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace Silmoon.IO.SmFile
{
    /// <summary>
    /// Ŀ¼�ļ�������
    /// </summary>
    public class Searcher
    {
        /// <summary>
        /// ��ȡ�ƶ�Ŀ¼�����е��ļ���
        /// </summary>
        /// <param name="path">ָ����Ŀ¼</param>
        /// <param name="processCallBack">����ί��</param>
        /// <returns></returns>
        public string[] GetFiles(string path, SearcherEventHandler processCallBack = null)
        {
            ArrayList arrayList = new ArrayList();
            try
            {
                string[] fs = Directory.GetFiles(path);
                foreach (string file in fs)
                {
                    if (processCallBack != null)
                        processCallBack(file, Path.GetFileName(file), 1);
                    arrayList.Add(file);
                }
            }
            catch { }

            try
            {
                string[] ds = Directory.GetDirectories(path);
                foreach (string dpath in ds)
                {
                    if (processCallBack != null)
                        processCallBack(dpath, "", 2);
                    InternalGetFiles(dpath, arrayList, processCallBack);
                }
            }
            catch { }
            return (string[])arrayList.ToArray(typeof(string));
        }
        void InternalGetFiles(string path, ArrayList array, SearcherEventHandler processCallBack)
        {
            try
            {
                string[] fs = Directory.GetFiles(path);
                foreach (string file in fs)
                {
                    if (processCallBack != null)
                        processCallBack(file, Path.GetFileName(file), 1);
                    array.Add(file);
                }
            }
            catch { }

            try
            {
                string[] ds = Directory.GetDirectories(path);
                foreach (string dpath in ds)
                {
                    if (processCallBack != null)
                        processCallBack(dpath, "", 2);
                    InternalGetFiles(dpath, array, processCallBack);
                }
            }
            catch { }
        }

        /// <summary>
        /// �����ļ�
        /// </summary>
        /// <param name="path">·��</param>
        /// <param name="searchPattern">�����ַ���</param>
        /// <param name="searchOption">����ѡ��</param>
        /// <returns>�ҵ����ļ���</returns>
        public string[] SearchFile(string path, string searchPattern, SearchOption searchOption, SearcherEventHandler processCallBack = null)
        {
            ArrayList arrayList = new ArrayList();
            try
            {
                string[] fs = Directory.GetFiles(path, searchPattern);
                foreach (string file in fs)
                {
                    if (processCallBack != null)
                        processCallBack(file, Path.GetFileName(file), 1);
                    arrayList.Add(file);
                }
            }
            catch { }
            if (searchOption == SearchOption.AllDirectories)
            {
                try
                {
                    string[] ds = Directory.GetDirectories(path);
                    foreach (string dpath in ds)
                    {
                        if (processCallBack != null)
                            processCallBack(dpath, "", 2);
                        InternalGetFiles(dpath, arrayList, processCallBack);
                    }
                }
                catch { }
            }
            return (string[])arrayList.ToArray(typeof(string));
        }
        void InternalSearchFile(string path, ArrayList array, string searchPattern, SearcherEventHandler processCallBack)
        {
            try
            {
                string[] fs = Directory.GetFiles(path, searchPattern);
                foreach (string file in fs)
                {
                    if (processCallBack != null)
                        processCallBack(file, Path.GetFileName(file), 1);
                    array.Add(file);
                }
            }
            catch { }

            try
            {
                string[] ds = Directory.GetDirectories(path);
                foreach (string dpath in ds)
                {
                    if (processCallBack != null)
                        processCallBack(dpath, "", 2);
                    InternalGetFiles(dpath, array, processCallBack);
                }
            }
            catch { }
        }
    }
    /// <summary>
    /// ��ʾ�������ļ�ʱ���¼��������
    /// </summary>
    /// <param name="fullPath">����·��</param>
    /// <param name="name">�ļ������ļ�����</param>
    /// <param name="type"></param>
    public delegate void SearcherEventHandler(string fullPath, string name, int type);
}
