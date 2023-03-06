using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ADCS.SidExtension.PolicyModule;

class AsnUtils {
    /// <summary>
    /// Encodes NTDS CA Security extension from a SID string.
    /// </summary>
    /// <param name="sid">A string that represents security identifier (SID).</param>
    /// <returns>Encoded extension</returns>
    public static X509Extension EncodeSidExtension(String sid) {
        var nestedOidBytes = encodeOid("1.3.6.1.4.1.311.25.2.1".Split('.').Select(x => Convert.ToInt64(x)).ToList()).ToList();
        Byte[] bytes = encode(Encoding.ASCII.GetBytes(sid), 4);
        bytes = encode(bytes, 160);
        nestedOidBytes.AddRange(bytes);
        bytes = encode(nestedOidBytes.ToArray(), 160);
        Byte[] rawData = encode(bytes, 48);

        return new X509Extension("1.3.6.1.4.1.311.25.2", rawData, false);
    }
    /// <summary>
    /// Wraps encoded data to an ASN.1 type/structure.
    /// </summary>
    /// <remarks>This method do not check whether the data in <strong>rawData</strong> is valid data for specified enclosing type.</remarks>
    /// <param name="rawData">A byte array to wrap.</param>
    /// <param name="enclosingTag">An enumeration of <see cref="Asn1Type"/>.</param>
    /// <returns>Wrapped encoded byte array.</returns>
    /// <remarks>If <strong>rawData</strong> is null, an empty tag is encoded.</remarks>
    static Byte[] encode(Byte[] rawData, Byte enclosingTag) {
        if (rawData == null) {
            return new Byte[] { enclosingTag, 0 };
        }
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
    static Byte[] encodeOid(IList<Int64> tokens) {
        List<Byte> rawOid = new List<Byte>();
        for (Int32 token = 0; token < tokens.Count; token++) {
            // first two arcs are encoded in a single byte
            switch (token) {
                case 0:
                    rawOid.Add((Byte)(40 * tokens[token] + tokens[token + 1]));
                    continue;
                case 1:
                    continue;
            }
            Int16 bitLength = 0;
            Int64 temp = tokens[token];
            // calculate how many bits are occupied by the current integer value
            do {
                temp = (Int64)Math.Floor((Double)temp / 2);
                bitLength++;
            } while (temp > 0);
            // calculate how many additional bytes are required and encode each integer in a 7 bit.
            // 8th bit of the integer is shifted to the left and 8th bit is set to 1 to indicate that
            // additional bytes are related to the current OID arc. Details:
            // http://msdn.microsoft.com/en-us/library/bb540809(v=vs.85).aspx
            // loop may not execute if arc value is less than 128.
            for (Int32 index = (bitLength - 1) / 7; index > 0; index--) {
                rawOid.Add((Byte)(0x80 | ((tokens[token] >> (index * 7)) & 0x7f)));
            }
            rawOid.Add((Byte)(tokens[token] & 0x7f));
        }
        return rawOid.ToArray();
    }
}