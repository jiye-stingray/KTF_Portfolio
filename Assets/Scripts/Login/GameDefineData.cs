using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class GameDefineData
{

    #region GET

    public static bool GetBool(string key, bool defaultValue)
    {
        return Convert.ToBoolean(PlayerPrefs.GetInt(key, Convert.ToInt32(defaultValue)));
    }
    
    public static string GetString(string key, string defaultValue, bool decrypt = false)
    {
        if(decrypt == false)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
        return AESDecrypt128(PlayerPrefs.GetString(key, defaultValue));
    }
    
    public static int GetInt(string key, int defaultValue)
    {
        return PlayerPrefs.GetInt(key, (int)defaultValue);
    }
    
    public static double GetDouble(string key, double defaultValue)
    {
        return Convert.ToDouble(PlayerPrefs.GetFloat(key, Convert.ToSingle(defaultValue)));
    }
    
    public static float GetFloat(string key, float defaultValue)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    #endregion

    #region SET

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }
    
    public static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, Convert.ToInt32(value));
    }
    
    public static void SetString(string key, string value, bool encrypt = false)
    {
        if(encrypt == false)
        {
            PlayerPrefs.SetString(key, value);
            return;
        }
        PlayerPrefs.SetString(key, AESEncrypt128(value));
    }

    
    public static void SetDouble(string key, float value)
    {
        PlayerPrefs.SetFloat(key, Convert.ToSingle(value));
    }
    
    
    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    #endregion
    
    
    
    #region AES

    // 키로 사용하기 위한 암호 정의
    private static readonly string PASSWORD = "3ds1s334e4dcc7c4yz4554e732983h";

    // 인증키 정의
    private static readonly string KEY = PASSWORD.Substring(0, 128 / 8);

    
    // 암호화
    public static string AESEncrypt128(string plain)
    {
        try
        {
            if (string.IsNullOrEmpty(plain) == true)
                return null;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plain);

            RijndaelManaged myRijndael = new RijndaelManaged();
            myRijndael.Mode = CipherMode.CBC;
            myRijndael.Padding = PaddingMode.PKCS7;
            myRijndael.KeySize = 128;

            MemoryStream memoryStream = new MemoryStream();

            ICryptoTransform encryptor = myRijndael.CreateEncryptor(Encoding.UTF8.GetBytes(KEY), Encoding.UTF8.GetBytes(KEY));

            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();

            byte[] encryptBytes = memoryStream.ToArray();

            string encryptString = Convert.ToBase64String(encryptBytes);

            cryptoStream.Close();
            memoryStream.Close();

            return encryptString;

        }
        catch (Exception ex)
        {
            return null;
        }
    }
    
    
    // 복호화
    public static string AESDecrypt128(string encrypt)
    {
        try
        {
            if (string.IsNullOrEmpty(encrypt) == true)
                return null;

            byte[] encryptBytes = Convert.FromBase64String(encrypt);

            RijndaelManaged myRijndael = new RijndaelManaged();
            myRijndael.Mode = CipherMode.CBC;
            myRijndael.Padding = PaddingMode.PKCS7;
            myRijndael.KeySize = 128;

            MemoryStream memoryStream = new MemoryStream(encryptBytes);

            ICryptoTransform decryptor = myRijndael.CreateDecryptor(Encoding.UTF8.GetBytes(KEY), Encoding.UTF8.GetBytes(KEY));

            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            byte[] plainBytes = new byte[encryptBytes.Length];

            int plainCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);

            string plainString = Encoding.UTF8.GetString(plainBytes, 0, plainCount);

            cryptoStream.Close();
            memoryStream.Close();

            return plainString;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    #endregion

    public static void Save()
    {
        PlayerPrefs.Save();
    }
    
    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    public static void DeleteData(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }
}
