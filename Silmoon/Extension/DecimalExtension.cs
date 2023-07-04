﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Silmoon.Extension
{
    public static class DecimalExtension
    {
        static readonly CultureInfo enUS = new CultureInfo("en-US");
        static readonly CultureInfo zhCN = new CultureInfo("zh-CN");
        public static string ToUSD(this decimal value)
        {
            enUS.NumberFormat.CurrencyNegativePattern = 2;
            return value.ToString("C", enUS);
        }
        public static string ToRMB(this decimal value)
        {
            zhCN.NumberFormat.CurrencyNegativePattern = 2;
            return value.ToString("C", zhCN);
        }
        public static decimal Pow(this decimal x, int y)
        {
            decimal result = 1;
            for (int i = 0; i < y; i++)
            {
                result *= x;
            }
            return result;
        }
        public static BigInteger ToBigInteger(this decimal amount, int decimals)
        {
            BigInteger result = (BigInteger)(amount * 10m.Pow(decimals));
            return result;
        }
        public static string GetPercentString(this decimal value)
        {
            return (value * 100).ToString("0.00") + "%";
        }
        public static string ConvertToChinese(this decimal number)
        {
            var format = number.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A").Replace("0B0A", "@");
            var simplify = Regex.Replace(format, @"((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\.]|$))))", "${b}${z}");
            var result = Regex.Replace(simplify, ".", match => "负元空零壹贰叁肆伍陆柒捌玖空空空空空空整分角拾佰仟万亿兆京垓秭穰"[match.Value[0] - '-'].ToString());
            return result;
        }
    }
}
