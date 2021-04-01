using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace ToolsUniversal
{
    public class EncryptionWin
    {
        /// <summary>
        /// Converts to hexadecimal lowercase
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ConvertToHex(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder(bytes.Length);

            for (int i = 0; i < bytes.Length; i++)
                builder.Append(bytes[i].ToString("x2"));

            return builder.ToString();
        }

        /// <summary>
        /// Uses UTF8
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ConvertToBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string Sha256(string str)
        {
            return hash(str, HashAlgorithmNames.Sha256);
        }

        private static IBuffer getBuffer(byte[] bytes)
        {
            return CryptographicBuffer.CreateFromByteArray(bytes);
        }

        private static IBuffer getBuffer(string str)
        {
            return CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
        }

        private static IBuffer getBuffer(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            return getBuffer(bytes);
        }

        private static string hash(string str, string algorithmName)
        {
            return hash(getBuffer(str), algorithmName);
        }

        private static string hash(Stream stream, string algorithmName)
        {
            return hash(getBuffer(stream), algorithmName);
        }

        private static string hash(byte[] bytes, string algorithmName)
        {
            return hash(getBuffer(bytes), algorithmName);
        }

        private static string hash(IBuffer buffBytes, string algorithmName)
        {
            //grab the algoritm
            HashAlgorithmProvider algorithm = HashAlgorithmProvider.OpenAlgorithm(algorithmName);

            //hash the data
            IBuffer buffHash = algorithm.HashData(buffBytes);

            //verify that hash succeeded
            if (buffHash.Length != algorithm.HashLength)
                throw new Exception("There was an error creating the hash.");

            //convert to string
            //return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffHash);
            return CryptographicBuffer.EncodeToHexString(buffHash);
        }

        public static string Sha256(byte[] bytes)
        {
            return hash(bytes, HashAlgorithmNames.Sha256);
        }

        public static string Sha1(byte[] bytes)
        {
            return hash(bytes, HashAlgorithmNames.Sha1);
        }

        public static string Sha1(Stream stream)
        {
            return hash(stream, HashAlgorithmNames.Sha1);
        }
    }
}
