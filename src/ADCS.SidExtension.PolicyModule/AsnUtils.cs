using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ADCS.SidExtension.PolicyModule;

class AsnUtils {
    static readonly Byte[] _nestedOidBytes = [6, 10, 43, 6, 1, 4, 1, 130, 55, 25, 2, 1];
    /// <summary>
    /// Encodes NTDS CA Security extension from a SID string.
    /// </summary>
    /// <param name="sid">A string that represents security identifier (SID).</param>
    /// <returns>Encoded extension</returns>
    public static X509Extension EncodeSidExtension(String sid) {
        var data = new List<Byte>(_nestedOidBytes);
        Byte[] bytes = encode(Encoding.ASCII.GetBytes(sid), 4);
        bytes = encode(bytes, 160);
        data.AddRange(bytes);
        bytes = encode(data.ToArray(), 160);
        Byte[] rawData = encode(bytes, 48);

        return new X509Extension("1.3.6.1.4.1.311.25.2", rawData, false);
    }
    /// <summary>
    /// Wraps encoded data to an ASN.1 type/structure.
    /// </summary>
    /// <remarks>This method do not check whether the data in <strong>rawData</strong> is valid data for specified enclosing type.</remarks>
    /// <param name="rawData">A byte array to wrap.</param>
    /// <param name="enclosingTag">Tag number to wrap data into.</param>
    /// <returns>Wrapped encoded byte array.</returns>
    /// <remarks>If <strong>rawData</strong> is null, an empty tag is encoded.</remarks>
    static Byte[] encode(Byte[] rawData, Byte enclosingTag) {
        Byte[] retValue;
        if (rawData.Length < 128) {
            retValue = new Byte[rawData.Length + 2];
            retValue[0] = enclosingTag;
            retValue[1] = (Byte)rawData.Length;
            rawData.CopyTo(retValue, 2);
        } else {
            Byte[] lenBytes = new Byte[4];
            Int32 num = rawData.Length;
            Int32 counter = 0;
            while (num >= 256) {
                lenBytes[counter] = (Byte)(num & 255);
                num >>= 8;
                counter++;
            }
            // 3 is: len byte and enclosing tag
            retValue = new Byte[rawData.Length + 3 + counter];
            rawData.CopyTo(retValue, 3 + counter);
            retValue[0] = enclosingTag;
            retValue[1] = (Byte)(129 + counter);
            retValue[2] = (Byte)num;
            Int32 n = 3;
            for (Int32 i = counter - 1; i >= 0; i--) {
                retValue[n] = lenBytes[i];
                n++;
            }
        }
        return retValue;
    }
}