namespace Lwt;

using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// etag generator.
/// </summary>
public static class ETagGenerator
{
    /// <summary>
    /// get etag.
    /// </summary>
    /// <param name="key">key.</param>
    /// <param name="contentBytes">content.</param>
    /// <returns>etag.</returns>
    public static string GetETag(string key, byte[] contentBytes)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] combinedBytes = Combine(keyBytes, contentBytes);

        return GenerateETag(combinedBytes);
    }

    private static string GenerateETag(byte[] data)
    {
        using (var md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(data);
            string hex = BitConverter.ToString(hash);
            return hex.Replace("-", string.Empty);
        }
    }

    private static byte[] Combine(byte[] a, byte[] b)
    {
        byte[] c = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, c, 0, a.Length);
        Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
        return c;
    }
}