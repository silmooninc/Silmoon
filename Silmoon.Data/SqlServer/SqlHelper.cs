﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Data.SqlTypes;
using Silmoon.Runtime.Collections;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using Newtonsoft.Json;
using Silmoon.Extension;
using FieldInfo = Silmoon.Runtime.FieldInfo;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Silmoon.Data.SqlServer
{
    public class SqlHelper
    {
        static string[] regTypeClassName = new string[] { "DateTime", "String", "Boolean", "Int16", "UInt16", "Int32", "UInt32", "Int64", "UInt64", "Decimal", "Guid", "ObjectId", "Byte[]", "Int32[]", "String[]" };
        public static (T[] Results, NameObjectCollection<object>[] DataCollections) MakeObjects<T>(SqlDataReader reader, string[] excludedField = null) where T : new()
        {
            List<T> result = new List<T>();
            List<NameObjectCollection<object>> data = new List<NameObjectCollection<object>>();

            while (reader.Read())
            {
                var r = MakeObject<T>(reader, excludedField, false);
                result.Add(r.Result);
                data.Add(r.DataCollection);
            }
            reader.Close();
            return (result.ToArray(), data.ToArray());
        }
        public static (T Result, NameObjectCollection<object> DataCollection) MakeObject<T>(SqlDataReader reader, string[] excludedField = null, bool closeReader = true) where T : new()
        {
            T obj = new T();
            return MakeObject(reader, obj, excludedField, closeReader);
        }
        public static (T Result, NameObjectCollection<object> DataCollection) MakeObject<T>(SqlDataReader reader, T obj, string[] excludedField = null, bool closeReader = true) where T : new()
        {
            NameObjectCollection<object> data = new NameObjectCollection<object>();
            for (int i = 0; i < reader.FieldCount; i++)
                data.Add(reader.GetName(i), reader[i]);


            var propertyInfos = obj.GetType().GetProperties();
            foreach (PropertyInfo item in propertyInfos)
            {
                string name = item.Name;
                if (excludedField?.Contains(name) ?? false) continue;

                Type type = item.PropertyType;
                if (reader[name] != DBNull.Value)
                {
                    //try
                    //{
                    if (type.IsEnum)
                    {
                        if (reader[name] is string)
                            item.SetValue(obj, Enum.Parse(type, (string)reader[name]), null);
                        else if (reader[name] is int)
                            item.SetValue(obj, (int)reader[name], null);
                    }
                    else if (type.IsArray && type != typeof(byte[]))
                    {
                        var val = (string)reader[name];
                        if (string.IsNullOrEmpty(val)) continue;
                        var res = JsonSerializer.Deserialize(val, type);
                        item.SetValue(obj, res, null);
                    }
                    else if (regTypeClassName.Contains(type.Name))
                        item.SetValue(obj, reader[name], null);
                    else
                        item.SetValue(obj, JsonConvert.DeserializeObject((string)reader[name], type), null);
                }
            }
            if (closeReader) reader.Close();
            return (obj, data);
        }

        public static (T[] Results, NameObjectCollection<object>[] DataCollections) MakeObjects<T>(DataTable dt, string[] excludedField = null) where T : new()
        {
            T[] result = new T[dt.Rows.Count];
            NameObjectCollection<object>[] data = new NameObjectCollection<object>[dt.Rows.Count];

            for (int i = 0; i < result.Length; i++)
            {
                var r = MakeObject<T>(dt.Rows[i], excludedField);
                result[i] = r.Result;
                data[i] = r.DataCollection;
            }

            return (result, data);
        }
        public static (T Result, NameObjectCollection<object> DataCollection) MakeObject<T>(DataRow row, string[] excludedField = null) where T : new()
        {
            T obj = new T();
            return MakeObject(row, obj, excludedField);
        }
        public static (T Result, NameObjectCollection<object> DataCollection) MakeObject<T>(DataRow row, T obj, string[] excludedField = null) where T : new()
        {
            NameObjectCollection<object> data = new NameObjectCollection<object>();
            for (int i = 0; i < row.Table.Columns.Count; i++)
                data.Add(row.Table.Columns[i].ColumnName, row[i]);

            var propertyInfos = obj.GetType().GetProperties();
            foreach (PropertyInfo item in propertyInfos)
            {
                string name = item.Name;
                if (excludedField?.Contains(name) ?? false) continue;
                Type type = item.PropertyType;
                if (row[name] != DBNull.Value)
                {
                    if (type.IsEnum)
                    {
                        if (row[name] is string)
                            item.SetValue(obj, Enum.Parse(type, (string)row[name]), null);
                        else if (row[name] is int)
                            item.SetValue(obj, (int)row[name], null);
                    }
                    else if (type.IsArray && type != typeof(byte[]))
                    {
                        var val = (string)row[name];
                        if (string.IsNullOrEmpty(val)) continue;
                        var res = JsonSerializer.Deserialize(val, type);
                        item.SetValue(obj, res, null);
                    }
                    else if (regTypeClassName.Contains(type.Name))
                        item.SetValue(obj, row[name], null);
                    else
                        item.SetValue(obj, JsonConvert.DeserializeObject((string)row[name], type), null);
                }
            }

            return (obj, data);
        }

        public static void AddSqlCommandParameters(SqlCommand sqlCommand, Dictionary<string, FieldInfo> fieldInfos, params string[] paraNames)
        {
            if (fieldInfos != null)
            {
                foreach (var item in fieldInfos)
                {
                    string name = item.Value.Name;
                    if (paraNames.Contains(name))
                    {
                        var value = item.Value.Value;
                        Type type = item.Value.Type;

                        if (!sqlCommand.Parameters.Contains(name))
                        {
                            if (value != null)
                            {
                                if (type.IsEnum)
                                {
                                    ///这里存储在数据库中的枚举使用字符串（.ToString()）
                                    //sqlCommand.Parameters.AddWithValue(name, value.ToString());

                                    ///这里存储在数据库中的枚举使用数字
                                    sqlCommand.Parameters.AddWithValue(name, value);
                                }
                                else if (type.Name == "DateTime" && ((DateTime)value) == DateTime.MinValue)
                                    sqlCommand.Parameters.AddWithValue(name, SqlDateTime.MinValue);
                                else if (type.IsArray && type.Name != "Byte[]")
                                {
                                    string s = JsonSerializer.Serialize(value, value.GetType());
                                    sqlCommand.Parameters.AddWithValue(name, s);
                                }
                                else if (regTypeClassName.Contains(type.Name))
                                    sqlCommand.Parameters.AddWithValue(name, value);
                                else
                                    sqlCommand.Parameters.AddWithValue(name, value.ToJsonString());
                            }
                            else
                            {
                                if (type.Name == "Byte[]")
                                    sqlCommand.Parameters.AddWithValue(name, SqlBinary.Null);
                                else
                                    sqlCommand.Parameters.AddWithValue(name, DBNull.Value);
                            }
                        }
                    }
                }
            }
        }
        public static void AddSqlCommandParameters(SqlCommand sqlCommand, Dictionary<string, FieldInfo> fieldInfos)
        {
            if (fieldInfos != null)
            {
                foreach (var field in fieldInfos)
                {
                    string name = field.Value.Name;
                    var value = field.Value.Value;
                    Type type = field.Value.Type;

                    if (!sqlCommand.Parameters.Contains(name))
                    {
                        if (value != null)
                        {
                            if (type.IsEnum)
                            {
                                ///这里存储在数据库中的枚举使用字符串（.ToString()）
                                //sqlCommand.Parameters.AddWithValue(name, value.ToString());

                                ///这里存储在数据库中的枚举使用数字
                                sqlCommand.Parameters.AddWithValue(name, value);
                            }
                            else if (type.Name == "DateTime" && ((DateTime)value) == DateTime.MinValue)
                                sqlCommand.Parameters.AddWithValue(name, SqlDateTime.MinValue);
                            else if (type.IsArray && type.Name != "Byte[]")
                            {
                                string s = JsonSerializer.Serialize(value, value.GetType());
                                sqlCommand.Parameters.AddWithValue(name, s);
                            }
                            else if (regTypeClassName.Contains(type.Name))
                                sqlCommand.Parameters.AddWithValue(name, value);
                            else
                                sqlCommand.Parameters.AddWithValue(name, value.ToJsonString());
                        }
                        else
                        {
                            if (type.Name == "Byte[]")
                                sqlCommand.Parameters.AddWithValue(name, SqlBinary.Null);
                            else
                                sqlCommand.Parameters.AddWithValue(name, DBNull.Value);
                        }
                    }
                }
            }
        }
    }
}
