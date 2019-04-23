using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Properties;

namespace Wist.Core.ExtensionMethods
{
    public static class StringExtensionMethods
    {
        // values for '\0' to 'f' where 255 indicates invalid input character
        // starting from '\0' and not from '0' costs 48 bytes
        // but results 0 subtructions and less if conditions
        private static readonly byte[] _fromHexTable = new byte[] {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 0, 1,
            2, 3, 4, 5, 6, 7, 8, 9, 255, 255,
            255, 255, 255, 255, 255, 10, 11, 12, 13, 14,
            15, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 10, 11, 12,
            13, 14, 15
        };

        // same as above but valid values are multiplied by 16
        private static readonly byte[] _fromHexTable16 = new byte[] {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 0, 16,
            32, 48, 64, 80, 96, 112, 128, 144, 255, 255,
            255, 255, 255, 255, 255, 160, 176, 192, 208, 224,
            240, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 160, 176, 192,
            208, 224, 240
        };

        public static unsafe byte[] HexStringToByteArray(this string source)
        {
            // return an empty array in case of null or empty source
            if (string.IsNullOrEmpty(source))
            {
                return new byte[0]; // you may change it to return null
            }

            if (source.Length % 2 == 1) // source length must be even
            {
                throw new ArgumentException(Resources.ERR_STRING_LEN_EVEN);
            }

            int index = 0, // start position for parsing source
                len = source.Length >> 1; // initial length of result
            
            // take the first character address of source
            fixed (char* sourceRef = source)
            {
                if (*(int*)sourceRef == 7864368) // source starts with "0x"
                {
                    if (source.Length == 2) // source must not be just a "0x")
                        throw new ArgumentException();
                    index += 2; // start position (bypass "0x")
                    len -= 1; // result length (exclude "0x")
                }

                byte add = 0; // keeps a fromHexTable value
                byte[] result = new byte[len]; // initialization of result for known length
                // freeze fromHexTable16 position in memory
                fixed (byte* hiRef = _fromHexTable16)
                // freeze fromHexTable position in memory
                fixed (byte* lowRef = _fromHexTable)
                // take the first byte address of result
                fixed (byte* resultRef = result)
                {
                    // take first parsing position of source - allow inremental memory position
                    char* s = (char*)&sourceRef[index];
                    // take first byte position of result - allow incremental memory position
                    byte* r = resultRef;
                    // source has more characters to parse
                    while (*s != 0)
                    {
                        // check for non valid characters in pairs
                        // you may split it if you don't like its readbility
                        if (
                            // check for character > 'f'
                            *s > 102 ||
                            // assign source value to current result position and increment source position
                            // and check if is a valid character
                            (*r = hiRef[*s++]) == 255 ||
                            // check for character > 'f'
                            *s > 102 ||
                            // assign source value to "add" parameter and increment source position
                            // and check if is a valid character
                            (add = lowRef[*s++]) == 255
                            )
                            throw new ArgumentException();
                        // set final value of current result byte and move pointer to next byte
                        *r++ += add;
                    }
                    return result;
                }
            }
        }
    }
}
