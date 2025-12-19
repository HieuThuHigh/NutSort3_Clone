using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

#if USE_DATDEVJSON
using Newtonsoft.Json;
#endif

using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DatdevUlts.Csv2CsharpTools
{
    /// <summary>
    /// CẦN CÀI ĐẶT PACKAGE Newtonsoft.Json (com.unity.nuget.newtonsoft-json@3.0)
    /// </summary>
    public static class Csv2Csharp
    {
        private static List<string> allowedDataTypes = new List<string>
            { "string", "int", "bool", "float", "string[]", "int[]", "bool[]", "float[]" };

        public static string GenerateCodeCsv(string csvText)
        {
            // Đọc dữ liệu từ file CSV
            string[] lines = csvText.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }

            // Lấy thông tin từ các dòng
            string[] variableNames = lines[0].Split(',');
            string[] dataTypes = lines[1].Split(',');

            StringBuilder outPut = new StringBuilder();

            outPut.AppendLine(
                @"// Case use google sheet, sheet view permission must public
Csv2Csharp.CompleteHandle completeHandle = new Csv2Csharp.CompleteHandle(); 
                    var request = Csv2Csharp.Await_GGs_GetCsvText(_linkCsvGgSheetPublic, completeHandle);
                    while (request.MoveNext())
                    {
                        
                    }

                    csvText = completeHandle.Output;");
            outPut.AppendLine(
                "// var listJsonData = JsonConvert.DeserializeObject<List<Root>>(Csv2Csharp.CsvToJson(csvText));");
            outPut.AppendLine("public class Root");
            outPut.AppendLine("{");

            for (int i = 0; i < variableNames.Length; i++)
            {
                outPut.AppendLine(
                    $"    [JsonProperty(\"{variableNames[i]}\")] public {NormalizeDataTypeName(dataTypes[i])} {NormalizeVariableName(variableNames[i])};");
            }

            outPut.AppendLine("}");
            return outPut.ToString().Trim();
        }

        public static string CsvToJson(string csvText)
        {
            csvText = csvText.Trim();

            //Get properties's name, data type and sheet data
            IDictionary<int, string>
                propertyNames =
                    new Dictionary<int, string>(); //Dictionary of (column index, property name of that column)
            IDictionary<int, string>
                dataTypes = new Dictionary<int, string>(); //Dictionary of (column index, data type of that column)
            IDictionary<int, Dictionary<int, string>>
                values =
                    new Dictionary<int,
                        Dictionary<int, string>>(); //Dictionary of (row index, dictionary of (column index, value in cell))

            // Đọc dữ liệu từ file CSV
            string[] listRows = csvText.Split('\n');
            for (int i = 0; i < listRows.Length; i++)
            {
                listRows[i] = listRows[i].Trim();
            }

            int rowIndex = 0;

            foreach (string row in listRows)
            {
                int columnIndex = 0;

                foreach (string cellValue in row.Split(','))
                {
                    string value = cellValue;
                    if (rowIndex == 0)
                    {
                        //This row is properties's name row
                        propertyNames.Add(columnIndex, value);
                    }
                    else if (rowIndex == 1)
                    {
                        //This row is properties's data type row
                        dataTypes.Add(columnIndex, value);
                    }
                    else
                    {
                        //Data rows
                        //Because first row is name row and second row is data type row, so we will minus 2 from rowIndex to make data index start from 0
                        if (!values.ContainsKey(rowIndex - 2))
                        {
                            values.Add(rowIndex - 2, new Dictionary<int, string>());
                        }

                        values[rowIndex - 2].Add(columnIndex, value);
                    }

                    columnIndex++;
                }

                rowIndex++;
            }

            var fileName = "a";
            
            //Create list of Dictionaries (property name, value). Each dictionary represent for a object in a row of sheet.
            List<object> datas = new List<object>();
            foreach (int rowId in values.Keys)
            {
                bool thisRowHasError = false;
                Dictionary<string, object> data = new Dictionary<string, object>();
                foreach (int columnId in propertyNames.Keys)
                {
                    //Read through all columns in sheet, with each column, create a pair of property(string) and value(type depend on dataType[columnId])
                    if (thisRowHasError) break;
                    if ((!dataTypes.ContainsKey(columnId)) || (!allowedDataTypes.Contains(dataTypes[columnId])))
                        continue; //There is not any data type or this data type is strange. May be this column is used for comments. Skip this column.
                    if (!values[rowId].ContainsKey(columnId))
                    {
                        values[rowId].Add(columnId, "");
                    }

                    string strVal = values[rowId][columnId];

                    switch (dataTypes[columnId])
                    {
                        case "string":
                        {
                            data.Add(propertyNames[columnId], strVal);
                            break;
                        }
                        case "int":
                        {
                            int val = 0;
                            if (!string.IsNullOrEmpty(strVal))
                            {
                                try
                                {
                                    val = int.Parse(strVal);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(string.Format(
                                        "There is exception when parse value of property {0} of {1} class.\nDetail: {2}",
                                        propertyNames[columnId], fileName, e.ToString()));
                                    thisRowHasError = true;
                                    continue;
                                }
                            }

                            data.Add(propertyNames[columnId], val);
                            break;
                        }
                        case "bool":
                        {
                            bool val = false;
                            if (!string.IsNullOrEmpty(strVal))
                            {
                                try
                                {
                                    val = bool.Parse(strVal);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(string.Format(
                                        "There is exception when parse value of property {0} of {1} class.\nDetail: {2}",
                                        propertyNames[columnId], fileName, e.ToString()));
                                    continue;
                                }
                            }

                            data.Add(propertyNames[columnId], val);
                            break;
                        }
                        case "float":
                        {
                            float val = 0f;
                            if (!string.IsNullOrEmpty(strVal))
                            {
                                try
                                {
                                    val = float.Parse(strVal);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(string.Format(
                                        "There is exception when parse value of property {0} of {1} class.\nDetail: {2}",
                                        propertyNames[columnId], fileName, e.ToString()));
                                    continue;
                                }
                            }

                            data.Add(propertyNames[columnId], val);
                            break;
                        }
                        case "string[]":
                        {
                            string[] valArr = strVal.Split(new[] { ',' });
                            data.Add(propertyNames[columnId], valArr);
                            break;
                        }
                        case "int[]":
                        {
                            string[] strValArr = strVal.Split(new[] { ',' });
                            int[] valArr = new int[strValArr.Length];
                            if (string.IsNullOrEmpty(strVal.Trim()))
                            {
                                valArr = Array.Empty<int>();
                            }

                            bool error = false;
                            for (int i = 0; i < valArr.Length; i++)
                            {
                                int val = 0;
                                if (!string.IsNullOrEmpty(strValArr[i]))
                                {
                                    try
                                    {
                                        val = int.Parse(strValArr[i]);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError(string.Format(
                                            "There is exception when parse value of property {0} of {1} class.\nDetail: {2}",
                                            propertyNames[columnId], fileName, e.ToString()));
                                        error = true;
                                        break;
                                    }
                                }

                                valArr[i] = val;
                            }

                            if (error)
                                continue;
                            data.Add(propertyNames[columnId], valArr);
                            break;
                        }
                        case "bool[]":
                        {
                            string[] strValArr = strVal.Split(new[] { ',' });
                            bool[] valArr = new bool[strValArr.Length];
                            if (string.IsNullOrEmpty(strVal.Trim()))
                            {
                                valArr = Array.Empty<bool>();
                            }

                            bool error = false;
                            for (int i = 0; i < valArr.Length; i++)
                            {
                                bool val = false;
                                if (!string.IsNullOrEmpty(strValArr[i]))
                                {
                                    try
                                    {
                                        val = bool.Parse(strValArr[i]);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError(string.Format(
                                            "There is exception when parse value of property {0} of {1} class.\nDetail: {2}",
                                            propertyNames[columnId], fileName, e.ToString()));
                                        error = true;
                                        break;
                                    }
                                }

                                valArr[i] = val;
                            }

                            if (error)
                                continue;
                            data.Add(propertyNames[columnId], valArr);
                            break;
                        }
                        case "float[]":
                        {
                            string[] strValArr = strVal.Split(new[] { ',' });
                            float[] valArr = new float[strValArr.Length];
                            if (string.IsNullOrEmpty(strVal.Trim()))
                            {
                                valArr = Array.Empty<float>();
                            }

                            bool error = false;
                            for (int i = 0; i < valArr.Length; i++)
                            {
                                float val = 0f;
                                if (!string.IsNullOrEmpty(strValArr[i]))
                                {
                                    try
                                    {
                                        val = float.Parse(strValArr[i]);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError(string.Format(
                                            "There is exception when parse value of property {0} of {1} class.\nDetail: {2}",
                                            propertyNames[columnId], fileName, e.ToString()));
                                        error = true;
                                        break;
                                    }
                                }

                                valArr[i] = val;
                            }

                            if (error)
                                continue;
                            data.Add(propertyNames[columnId], valArr);
                            break;
                        }
                    }
                }

                if (!thisRowHasError)
                {
                    datas.Add(data);
                }
                else
                {
                    Debug.LogError("There's error!");
                }
            }

            #if USE_DATDEVJSON
            //Create json text
            string jsonText = JsonConvert.SerializeObject(datas);
            return jsonText;
            #else
            return "";
            #endif
        }

        public static string ExtractGoogleSheetId(string url)
        {
            const string identifier = "/spreadsheets/d/";
            int startIndex = url.IndexOf(identifier, StringComparison.Ordinal);

            if (startIndex != -1)
            {
                startIndex += identifier.Length;
                int endIndex = url.IndexOf('/', startIndex);

                if (endIndex != -1)
                {
                    return url.Substring(startIndex, endIndex - startIndex);
                }

                return url.Substring(startIndex);
            }

            return "";
        }

        public static IEnumerator Await_GGs_GetCsvText(string linkCsvGgSheetPublic, CompleteHandle completeHandle)
        {
            linkCsvGgSheetPublic =
                $"https://docs.google.com/spreadsheets/d/{ExtractGoogleSheetId(linkCsvGgSheetPublic)}/export?format=csv";
            using UnityWebRequest webRequest = UnityWebRequest.Get(linkCsvGgSheetPublic);
            webRequest.timeout = 10000;
            while (webRequest.result == UnityWebRequest.Result.InProgress)
            {
                yield return null;
            }

            completeHandle.Result = webRequest.result;
            completeHandle.Output = webRequest.downloadHandler.text;

            Debug.Log($"webRequest: {completeHandle.Result}");
        }

#if UNITY_EDITOR
        /// <summary>
        /// Only Editor
        /// </summary>
        /// <param name="assetPath">Eg: Assets/_Collect_Knife/Sources/Character Info CSV.csv</param>
        /// <returns>Text of file</returns>
        public static string GetCsvTextByPathEditor(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath).text;
        }
#endif

        public class CompleteHandle
        {
            public UnityWebRequest.Result Result { get; internal set; }
            public string Output { get; internal set; }
        }

        public static string NormalizeVariableName(string input)
        {
            string normalized = input.Trim();
            // Loại bỏ khoảng trắng và các ký tự không hợp lệ
            normalized = Regex.Replace(normalized, "[^a-zA-Z0-9_]", "_");

            // Loại bỏ các ký tự số ở đầu chuỗi
            normalized = Regex.Replace(normalized, "^[0-9]+", "_");

            return normalized;
        }

        public static string NormalizeDataTypeName(string input)
        {
            string normalized = input.Trim();

            return normalized;
        }

        public static object GetObject(string value, string dataType)
        {
            value = value.Trim();
            dataType = dataType.Trim();

            if (dataType.Contains("[]"))
            {
                var values = value.Split(',');

                switch (dataType.Replace("[]", "").ToLower())
                {
                    case "int":
                    {
                        int[] res = new int[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            res[i] = Convert.ToInt32(values[i]);
                        }

                        return res;
                    }
                    case "short":
                    {
                        short[] res = new short[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            res[i] = Convert.ToInt16(values[i]);
                        }

                        return res;
                    }
                    case "long":
                    {
                        long[] res = new long[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            res[i] = Convert.ToInt64(values[i]);
                        }

                        return res;
                    }
                    case "byte":
                    {
                        byte[] res = new byte[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            res[i] = Convert.ToByte(values[i]);
                        }

                        return res;
                    }
                    case "float":
                    {
                        float[] res = new float[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            res[i] = Convert.ToSingle(values[i]);
                        }

                        return res;
                    }
                    case "double":
                    {
                        double[] res = new double[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            res[i] = Convert.ToDouble(values[i]);
                        }

                        return res;
                    }
                    case "decimal":
                    {
                        decimal[] res = new decimal[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            res[i] = Convert.ToDecimal(values[i]);
                        }

                        return res;
                    }
                    case "char":
                    {
                        char[] res = new char[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            res[i] = Convert.ToChar(values[i]);
                        }

                        return res;
                    }
                    case "string":
                    {
                        string[] res = new string[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            res[i] = values[i];
                        }

                        return res;
                    }
                    case "bool":
                    {
                        bool[] res = new bool[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            res[i] = Convert.ToBoolean(values[i]);
                        }

                        return res;
                    }
                    default:
                        return null;
                }
            }

            switch (dataType.ToLower())
            {
                case "int":
                    return Convert.ToInt32(value);
                case "short":
                    return Convert.ToInt16(value);
                case "long":
                    return Convert.ToInt64(value);
                case "byte":
                    return Convert.ToByte(value);
                case "float":
                    return Convert.ToSingle(value);
                case "double":
                    return Convert.ToDouble(value);
                case "decimal":
                    return Convert.ToDecimal(value);
                case "char":
                    return Convert.ToChar(value);
                case "string":
                    return value;
                case "bool":
                    return Convert.ToBoolean(value);
                default:
                    return null;
            }
        }
    }
}