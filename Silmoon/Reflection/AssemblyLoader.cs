using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Windows.Forms;

namespace Silmoon.Reflection
{
    /// <summary>
    /// ������غ�ж�س���
    /// </summary>
    public class AssemblyLoader
    {
        ProxyAssembly _object;
        AppDomain _domain;
        string _dllPath;
        bool _fileLoaded = false;
        bool _assemblyCreated = false;

        /// <summary>
        /// ��ȡ����ʵ������
        /// </summary>
        public object AssemblyObject
        {
            get { return _object.AssemblyObject; }
        }


        /// <summary>
        /// ��ȡ�ļ��Ƿ��Ѿ�������
        /// </summary>
        public bool FileLoaded
        {
            get { return _fileLoaded; }
            set { _fileLoaded = value; }
        }
        /// <summary>
        /// ��ȡ�����Ƿ��Ѿ���ʵ����
        /// </summary>
        public bool AssemblyCreated
        {
            get { return _assemblyCreated; }
            set { _assemblyCreated = value; }
        }

        /// <summary>
        /// ʵ�������򼯹���
        /// </summary>
        /// <param name="assemblyPath">����DLL�ļ�·��</param>
        /// <param name="newDomainName">�½�������������</param>
        /// <param name="newDomain">�½�������������</param>
        public AssemblyLoader(string assemblyPath, string newDomainName, AppDomainSetup newDomain)
        {
            _domain = AppDomain.CreateDomain(newDomainName, null, newDomain);
            _object = (ProxyAssembly)_domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().CodeBase, "Silmoon.Reflection.ProxyAssembly");
            _dllPath = assemblyPath;
        }
        /// <summary>
        /// װ�س���DLL�ļ�
        /// </summary>
        /// <returns>�ļ��Ƿ���سɹ������ʧ�ܿ������Ѿ����أ�</returns>
        public bool Load()
        {
            if (_fileLoaded) return false;
            else
            {
                _object.LoadAssembly(_dllPath);
                _fileLoaded = true;
                return true;
            }
        }
        /// <summary>
        /// װ�س���DLL�ļ�����ʵ����������
        /// </summary>
        /// <param name="typeName">��������</param>
        /// <returns>�����ļ�������ɹ�����ʵ����ָ�������ͣ�����FALSE�����ļ����������Ѿ�����</returns>
        public bool Load(string typeName)
        {
            if (Load()) return CreateInstance(typeName);
            else return false;
        }
        /// <summary>
        /// ʵ����Ŀ������
        /// </summary>
        /// <param name="typeName">��������</param>
        /// <returns>ʵ�������ͣ��������FALSE���������Ѿ�ʵ��������û�м����ļ���</returns>
        public bool CreateInstance(string typeName)
        {
            if (_assemblyCreated) return false;
            bool result = _object.CreateInstance(typeName);
            _assemblyCreated = true;
            return result;
        }
        /// <summary>
        /// ʵ�������ͣ����ҵ���һ���ⲿ����
        /// </summary>
        /// <param name="typeName">��������</param>
        /// <param name="methodName">��������</param>
        /// <param name="parameters">����</param>
        /// <returns>���ص��������صĶ���</returns>
        public object CreateInstanceAndInvoke(string typeName, string methodName, params object[] parameters)
        {
            if (CreateInstance(typeName))
                return _object.Invoke(methodName, parameters);
            else return null;
        }
        /// <summary>
        /// ����һ���ⲿ����
        /// </summary>
        /// <param name="methodName">��������</param>
        /// <param name="parameters">����</param>
        /// <returns>���ص��������صĶ���</returns>
        public object Invoke(string methodName, params object[] parameters)
        {
            return _object.Invoke(methodName, parameters);
        }
        /// <summary>
        /// ����һ���ⲿ�ľ�̬����
        /// </summary>
        /// <param name="typeName">��������</param>
        /// <param name="methodName">��������</param>
        /// <param name="parameters">����</param>
        /// <returns>���ص��������صĶ���</returns>
        public object InvokeStatic(string typeName, string methodName, object[] parameters)
        {
            return _object.InvokeStatic(typeName, methodName, parameters);
        }
        /// <summary>
        /// ж�ص�ǰ�ĳ��򼯺�Ӧ�ó�����
        /// </summary>
        public void Unload()
        {
            AppDomain.Unload(_domain);
        }
    }
}
