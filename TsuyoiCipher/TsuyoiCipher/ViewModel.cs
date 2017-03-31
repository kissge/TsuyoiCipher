﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace TsuyoiCipher
{
    class ViewModel: INotifyPropertyChanged
    {
        string plaintext;
        public string Plaintext
        {
            get
            {
                return plaintext;
            }
            set
            {
                plaintext = value;
                OnPropertyChanged("Plaintext");
                Ciphertext = Convert.ToBase64String(Encoding.UTF8.GetBytes(StringCompressor.CompressString(plaintext)));
            }
        }

        string ciphertext = "";
        public string Ciphertext
        {
            get
            {
                return ciphertext;
            }
            set
            {
                ciphertext = value;
                OnPropertyChanged("Ciphertext");
                string _plaintext;
                try
                {
                    _plaintext = StringCompressor.DecompressString(Encoding.UTF8.GetString(Convert.FromBase64String(ciphertext)));
                }
                catch (Exception)
                {
                    _plaintext = "";
                }
                plaintext = _plaintext;
                OnPropertyChanged("Plaintext");
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Source: http://stackoverflow.com/a/17993002/3341110
    /// </summary>
    internal static class StringCompressor
    {
        /// <summary>
        /// Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        /// <summary>
        /// Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns></returns>
        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
