using System.Text;

namespace CMS.Membership
{
    internal static class Base32
    {
        /// <summary>
        /// Returns <see paramref="data"/> in base32 encoding without padding. 
        /// </summary>
        internal static string Encode(byte[] data)
        {
            int inByteSize = 8;
            int outByteSize = 5;
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

            int i = 0;
            int index = 0;
            int digit = 0;
            int currentByte, nextByte;
            StringBuilder result = new StringBuilder((data.Length + 7) * inByteSize / outByteSize);

            while (i < data.Length)
            {
                currentByte = (data[i] >= 0) ? data[i] : (data[i] + 256);

                if (index > (inByteSize - outByteSize))
                {
                    if ((i + 1) < data.Length)
                    {
                        nextByte = (data[i + 1] >= 0) ? data[i + 1] : (data[i + 1] + 256);
                    }
                    else
                    {
                        nextByte = 0;
                    }

                    digit = currentByte & (0xFF >> index);
                    index = (index + outByteSize) % inByteSize;
                    digit <<= index;
                    digit |= nextByte >> (inByteSize - index);
                    i++;
                }
                else
                {
                    digit = (currentByte >> (inByteSize - (index + outByteSize))) & 0x1F;
                    index = (index + outByteSize) % inByteSize;
                    if (index == 0)
                        i++;
                }
                result.Append(alphabet[digit]);
            }

            return result.ToString();
        }
    }
}
