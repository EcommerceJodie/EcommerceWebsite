using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Shared.Utils
{
    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }

    public class VnPayLibrary
    {
        public const string VERSION = "2.1.0";
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            if (_responseData.TryGetValue(key, out string retValue))
            {
                return retValue;
            }
            return string.Empty;
        }

        #region Request

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            StringBuilder dataBuilder = new StringBuilder();
            
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    dataBuilder.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                    Console.WriteLine($"[DEBUG-VNPAY-PARAM] {kv.Key}={kv.Value} | Encoded: {WebUtility.UrlEncode(kv.Value)}");
                }
            }
            
            string queryString = dataBuilder.ToString();
            

            string url = baseUrl + "?" + queryString;
            string signData = queryString;
            
            if (signData.Length > 0)
            {
                signData = signData.Remove(signData.Length - 1, 1);
            }
            
            Console.WriteLine($"[DEBUG-VNPAY] Sign data: {signData}");
            
            string vnpSecureHash = HmacSHA512(vnpHashSecret, signData);
            Console.WriteLine($"[DEBUG-VNPAY] Generated signature: {vnpSecureHash}");
            
            string paymentUrl = url + "vnp_SecureHash=" + vnpSecureHash;
            Console.WriteLine($"[DEBUG-VNPAY] URL thanh toán da t?o: {paymentUrl}");
            
            return paymentUrl;
        }

        #endregion

        #region Response process

        public bool ValidateSignature(string inputHash, string secretKey)
        {

            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }
            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }
            

            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            

            string rawData = data.ToString();
            if (rawData.Length > 0)
            {
                rawData = rawData.Substring(0, rawData.Length - 1);
            }
            
            Console.WriteLine($"[DEBUG-VNPAY] Response raw data for validation: {rawData}");
            

            string myChecksum = HmacSHA512(secretKey, rawData);
            Console.WriteLine($"[DEBUG-VNPAY] Generated checksum: {myChecksum}");
            Console.WriteLine($"[DEBUG-VNPAY] Input hash for comparison: {inputHash}");
            
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
        
        private string GetResponseData()
        {

            StringBuilder data = new StringBuilder();
            
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                {
                    data.Append(kv.Key + "=" + kv.Value + "&");
                }
            }
            

            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }
            
            return data.ToString();
        }

        #endregion

        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        public static string GetIpAddress(HttpContext context)
        {
            string ipAddress;
            try
            {
                ipAddress = context.Request.Headers["X-Forwarded-For"];

                if (string.IsNullOrEmpty(ipAddress) || (ipAddress.ToLower() == "unknown") || ipAddress.Length > 45)
                {
                    ipAddress = context.Connection.RemoteIpAddress?.ToString();
                }
            }
            catch (Exception ex)
            {
                ipAddress = "Invalid IP:" + ex.Message;
            }

            return ipAddress;
        }
    }
} 
