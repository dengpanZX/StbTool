using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
namespace StbTool
{

    public class RSAHelper
    {
        private static string privateKey = "<RSAKeyValue><Modulus>rHVIsYGLoO3EUz404fWoPJbzbupE3H6B8nq6LLSfK3Qb+4ZTCiDsJx4/nH9BxJPCGBTwpBO+KAawehKdCkj786czIpfQSCITO2xUFcR5qTfRJvKqfiMrYi3cAS2dgMvKKAlcSp/vQq/n03OZYaVWXtuHACB0JibApSEMtx9dolk=</Modulus><Exponent>AQAB</Exponent><P>645kAMsMnkLFbv2XjDYbrM1Drz820gyhH99zcqIniUFi7hQx82C0++gJbb/bdi7Q+7WWKuBlmpm0MlQtilFUqw==</P><Q>u2z5f2p8Vqbz1IrcXCIxiymp4JAvjlwHuxcLRLUxMAX0ntCZRUY5nZohUOXd9Cc9e45AMTZ9tj3V8sVek5b9Cw==</Q><DP>EURKyFQaBK/YUR59sWV1+eDCCWKU3ijW1sNGbyy7wS/t1I3ea3y3R4/mwQjDSZJ89zaEX3g7em2x686H2A/GKQ==</DP><DQ>tAKWHqpHgXIX/arguhydOOtrHSNeiXwacLZRPExKUbVmnKH8k2w/Kf8+wFQGONj3VBPr01hipQX+1ox5qYb6wQ==</DQ><InverseQ>XVnFRVmng+Y1bUQSv75pvOIdieQvcvq5o67ZYV08upb9K/uwrOgM6hOjS98sqBG6qB/ayL28PdVFLT5OwjpXTQ==</InverseQ><D>DHp4Bg//kGdj2zoXDXp+79NkTdQ5o1KsCGWE7xMqqTQ/ihWQEJ2NOM9hfuteUOD4HFH3j4e2LKM/RbXvEv1w7XPClUo98jNMW5SkF1QcPdHncluKys9W054B6Uyxiti8CmXXJ6rLevob5Pci4jtSrH50taL1tI5RzZgJYkU6vdk=</D></RSAKeyValue>";
        static public string Decrypt(string base64code)
        {
            try
            {
                //Create a UnicodeEncoder to convert between byte array and string.
                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                //Create a new instance of RSACryptoServiceProvider to generate

                //public and private key data.
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSA.FromXmlString(privateKey);

                byte[] encryptedData;

                byte[] decryptedData;

                encryptedData = Encoding.Default.GetBytes(base64code);
                Console.WriteLine("333333333333333333");
                //Pass the data to DECRYPT, the private key information 
                //(using RSACryptoServiceProvider.ExportParameters(true),
                //and a boolean flag specifying no OAEP padding.
                decryptedData = RSADecrypt(encryptedData, RSA.ExportParameters(true), false);

                //Display the decrypted plaintext to the console. 
                return ByteConverter.GetString(decryptedData);
            }

            catch (Exception exc)
            {

                //Exceptions.LogException(exc);
                Console.WriteLine(exc.Message);
                return "";
            }
        }

        static public string Encrypt(string toEncryptString)
        {

            try
            {

                //Create a UnicodeEncoder to convert between byte array and string.
                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                //Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] dataToEncrypt = ByteConverter.GetBytes(toEncryptString);

                byte[] encryptedData;

                //byte[] decryptedData;

                //Create a new instance of RSACryptoServiceProvider to generate

                //public and private key data.
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSA.FromXmlString(privateKey);

                //Pass the data to ENCRYPT, the public key information 

                //(using RSACryptoServiceProvider.ExportParameters(false),

                //and a boolean flag specifying no OAEP padding.
                encryptedData = RSAEncrypt(dataToEncrypt, RSA.ExportParameters(false), false);

                string base64code = Convert.ToBase64String(encryptedData);

                return base64code;
            }

            catch (Exception exc)
            {
                //Catch this exception in case the encryption did
                //not succeed.
                //Exceptions.LogException(exc);
                Console.WriteLine(exc.Message);
                return "";
            }
        }

        static private byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSA.ImportParameters(RSAKeyInfo);
                //Encrypt the passed byte array and specify OAEP padding.  
                //OAEP padding is only available on Microsoft Windows XP or
                //later.  
                return RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
            }

            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                //Exceptions.LogException(e);
                Console.WriteLine(e.Message);
                return null;
            }
        }

        static private byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                //Create a new instance of RSACryptoServiceProvider.
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

                //Import the RSA Key information. This needs
                //to include the private key information.
                Console.WriteLine("aaaaaaaaaaaaaaaaaaa");
                RSA.ImportParameters(RSAKeyInfo);
                Console.WriteLine("44444444444444444");
                //Decrypt the passed byte array and specify OAEP padding.  
                //OAEP padding is only available on Microsoft Windows XP or
                //later.  
                return RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
            }

            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                //Exceptions.LogException(e);
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private static byte[] HexStringToBytes(string hex)
        {
            if (hex.Length == 0)
            {
                return new byte[] { 0 };
            }

            if (hex.Length % 2 == 1)
            {
                hex = "0" + hex;
            }

            byte[] result = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length / 2; i++)
            {
                result[i] = byte.Parse(hex.Substring(2 * i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return result;
        }
    }
}
