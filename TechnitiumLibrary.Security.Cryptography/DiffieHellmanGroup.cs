﻿/*
Technitium Library
Copyright (C) 2015  Shreyas Zare (shreyas@technitium.com)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System.Numerics;

namespace TechnitiumLibrary.Security.Cryptography
{
    public enum DiffieHellmanGroupType : byte
    {
        None = 0,
        RFC3526 = 1
    }

    public class DiffieHellmanGroup
    {
        #region rfc3526 MODP groups

        static byte[] p2048 = new byte[] {
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0xC9, 0x0F, 0xDA, 0xA2,
                                    0x21, 0x68, 0xC2, 0x34,
                                    0xC4, 0xC6, 0x62, 0x8B,
                                    0x80, 0xDC, 0x1C, 0xD1,
                                    0x29, 0x02, 0x4E, 0x08,
                                    0x8A, 0x67, 0xCC, 0x74,
                                    0x02, 0x0B, 0xBE, 0xA6,
                                    0x3B, 0x13, 0x9B, 0x22,
                                    0x51, 0x4A, 0x08, 0x79,
                                    0x8E, 0x34, 0x04, 0xDD,
                                    0xEF, 0x95, 0x19, 0xB3,
                                    0xCD, 0x3A, 0x43, 0x1B,
                                    0x30, 0x2B, 0x0A, 0x6D,
                                    0xF2, 0x5F, 0x14, 0x37,
                                    0x4F, 0xE1, 0x35, 0x6D,
                                    0x6D, 0x51, 0xC2, 0x45,
                                    0xE4, 0x85, 0xB5, 0x76,
                                    0x62, 0x5E, 0x7E, 0xC6,
                                    0xF4, 0x4C, 0x42, 0xE9,
                                    0xA6, 0x37, 0xED, 0x6B,
                                    0x0B, 0xFF, 0x5C, 0xB6,
                                    0xF4, 0x06, 0xB7, 0xED,
                                    0xEE, 0x38, 0x6B, 0xFB,
                                    0x5A, 0x89, 0x9F, 0xA5,
                                    0xAE, 0x9F, 0x24, 0x11,
                                    0x7C, 0x4B, 0x1F, 0xE6,
                                    0x49, 0x28, 0x66, 0x51,
                                    0xEC, 0xE4, 0x5B, 0x3D,
                                    0xC2, 0x00, 0x7C, 0xB8,
                                    0xA1, 0x63, 0xBF, 0x05,
                                    0x98, 0xDA, 0x48, 0x36,
                                    0x1C, 0x55, 0xD3, 0x9A,
                                    0x69, 0x16, 0x3F, 0xA8,
                                    0xFD, 0x24, 0xCF, 0x5F,
                                    0x83, 0x65, 0x5D, 0x23,
                                    0xDC, 0xA3, 0xAD, 0x96,
                                    0x1C, 0x62, 0xF3, 0x56,
                                    0x20, 0x85, 0x52, 0xBB,
                                    0x9E, 0xD5, 0x29, 0x07,
                                    0x70, 0x96, 0x96, 0x6D,
                                    0x67, 0x0C, 0x35, 0x4E,
                                    0x4A, 0xBC, 0x98, 0x04,
                                    0xF1, 0x74, 0x6C, 0x08,
                                    0xCA, 0x18, 0x21, 0x7C,
                                    0x32, 0x90, 0x5E, 0x46,
                                    0x2E, 0x36, 0xCE, 0x3B,
                                    0xE3, 0x9E, 0x77, 0x2C,
                                    0x18, 0x0E, 0x86, 0x03,
                                    0x9B, 0x27, 0x83, 0xA2,
                                    0xEC, 0x07, 0xA2, 0x8F,
                                    0xB5, 0xC5, 0x5D, 0xF0,
                                    0x6F, 0x4C, 0x52, 0xC9,
                                    0xDE, 0x2B, 0xCB, 0xF6,
                                    0x95, 0x58, 0x17, 0x18,
                                    0x39, 0x95, 0x49, 0x7C,
                                    0xEA, 0x95, 0x6A, 0xE5,
                                    0x15, 0xD2, 0x26, 0x18,
                                    0x98, 0xFA, 0x05, 0x10,
                                    0x15, 0x72, 0x8E, 0x5A,
                                    0x8A, 0xAC, 0xAA, 0x68,
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0x00 //to keep BigInteger positive
                                };

        static byte[] p3072 = new byte[] {
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0xC9, 0x0F, 0xDA, 0xA2,
                                    0x21, 0x68, 0xC2, 0x34,
                                    0xC4, 0xC6, 0x62, 0x8B,
                                    0x80, 0xDC, 0x1C, 0xD1,
                                    0x29, 0x02, 0x4E, 0x08,
                                    0x8A, 0x67, 0xCC, 0x74,
                                    0x02, 0x0B, 0xBE, 0xA6,
                                    0x3B, 0x13, 0x9B, 0x22,
                                    0x51, 0x4A, 0x08, 0x79,
                                    0x8E, 0x34, 0x04, 0xDD,
                                    0xEF, 0x95, 0x19, 0xB3,
                                    0xCD, 0x3A, 0x43, 0x1B,
                                    0x30, 0x2B, 0x0A, 0x6D,
                                    0xF2, 0x5F, 0x14, 0x37,
                                    0x4F, 0xE1, 0x35, 0x6D,
                                    0x6D, 0x51, 0xC2, 0x45,
                                    0xE4, 0x85, 0xB5, 0x76,
                                    0x62, 0x5E, 0x7E, 0xC6,
                                    0xF4, 0x4C, 0x42, 0xE9,
                                    0xA6, 0x37, 0xED, 0x6B,
                                    0x0B, 0xFF, 0x5C, 0xB6,
                                    0xF4, 0x06, 0xB7, 0xED,
                                    0xEE, 0x38, 0x6B, 0xFB,
                                    0x5A, 0x89, 0x9F, 0xA5,
                                    0xAE, 0x9F, 0x24, 0x11,
                                    0x7C, 0x4B, 0x1F, 0xE6,
                                    0x49, 0x28, 0x66, 0x51,
                                    0xEC, 0xE4, 0x5B, 0x3D,
                                    0xC2, 0x00, 0x7C, 0xB8,
                                    0xA1, 0x63, 0xBF, 0x05,
                                    0x98, 0xDA, 0x48, 0x36,
                                    0x1C, 0x55, 0xD3, 0x9A,
                                    0x69, 0x16, 0x3F, 0xA8,
                                    0xFD, 0x24, 0xCF, 0x5F,
                                    0x83, 0x65, 0x5D, 0x23,
                                    0xDC, 0xA3, 0xAD, 0x96,
                                    0x1C, 0x62, 0xF3, 0x56,
                                    0x20, 0x85, 0x52, 0xBB,
                                    0x9E, 0xD5, 0x29, 0x07,
                                    0x70, 0x96, 0x96, 0x6D,
                                    0x67, 0x0C, 0x35, 0x4E,
                                    0x4A, 0xBC, 0x98, 0x04,
                                    0xF1, 0x74, 0x6C, 0x08,
                                    0xCA, 0x18, 0x21, 0x7C,
                                    0x32, 0x90, 0x5E, 0x46,
                                    0x2E, 0x36, 0xCE, 0x3B,
                                    0xE3, 0x9E, 0x77, 0x2C,
                                    0x18, 0x0E, 0x86, 0x03,
                                    0x9B, 0x27, 0x83, 0xA2,
                                    0xEC, 0x07, 0xA2, 0x8F,
                                    0xB5, 0xC5, 0x5D, 0xF0,
                                    0x6F, 0x4C, 0x52, 0xC9,
                                    0xDE, 0x2B, 0xCB, 0xF6,
                                    0x95, 0x58, 0x17, 0x18,
                                    0x39, 0x95, 0x49, 0x7C,
                                    0xEA, 0x95, 0x6A, 0xE5,
                                    0x15, 0xD2, 0x26, 0x18,
                                    0x98, 0xFA, 0x05, 0x10,
                                    0x15, 0x72, 0x8E, 0x5A,
                                    0x8A, 0xAA, 0xC4, 0x2D,
                                    0xAD, 0x33, 0x17, 0x0D,
                                    0x04, 0x50, 0x7A, 0x33,
                                    0xA8, 0x55, 0x21, 0xAB,
                                    0xDF, 0x1C, 0xBA, 0x64,
                                    0xEC, 0xFB, 0x85, 0x04,
                                    0x58, 0xDB, 0xEF, 0x0A,
                                    0x8A, 0xEA, 0x71, 0x57,
                                    0x5D, 0x06, 0x0C, 0x7D,
                                    0xB3, 0x97, 0x0F, 0x85,
                                    0xA6, 0xE1, 0xE4, 0xC7,
                                    0xAB, 0xF5, 0xAE, 0x8C,
                                    0xDB, 0x09, 0x33, 0xD7,
                                    0x1E, 0x8C, 0x94, 0xE0,
                                    0x4A, 0x25, 0x61, 0x9D,
                                    0xCE, 0xE3, 0xD2, 0x26,
                                    0x1A, 0xD2, 0xEE, 0x6B,
                                    0xF1, 0x2F, 0xFA, 0x06,
                                    0xD9, 0x8A, 0x08, 0x64,
                                    0xD8, 0x76, 0x02, 0x73,
                                    0x3E, 0xC8, 0x6A, 0x64,
                                    0x52, 0x1F, 0x2B, 0x18,
                                    0x17, 0x7B, 0x20, 0x0C,
                                    0xBB, 0xE1, 0x17, 0x57,
                                    0x7A, 0x61, 0x5D, 0x6C,
                                    0x77, 0x09, 0x88, 0xC0,
                                    0xBA, 0xD9, 0x46, 0xE2,
                                    0x08, 0xE2, 0x4F, 0xA0,
                                    0x74, 0xE5, 0xAB, 0x31,
                                    0x43, 0xDB, 0x5B, 0xFC,
                                    0xE0, 0xFD, 0x10, 0x8E,
                                    0x4B, 0x82, 0xD1, 0x20,
                                    0xA9, 0x3A, 0xD2, 0xCA,
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0x00 //to keep BigInteger positive
                                };

        static byte[] p4096 = new byte[] {
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0xC9, 0x0F, 0xDA, 0xA2,
                                    0x21, 0x68, 0xC2, 0x34,
                                    0xC4, 0xC6, 0x62, 0x8B,
                                    0x80, 0xDC, 0x1C, 0xD1,
                                    0x29, 0x02, 0x4E, 0x08,
                                    0x8A, 0x67, 0xCC, 0x74,
                                    0x02, 0x0B, 0xBE, 0xA6,
                                    0x3B, 0x13, 0x9B, 0x22,
                                    0x51, 0x4A, 0x08, 0x79,
                                    0x8E, 0x34, 0x04, 0xDD,
                                    0xEF, 0x95, 0x19, 0xB3,
                                    0xCD, 0x3A, 0x43, 0x1B,
                                    0x30, 0x2B, 0x0A, 0x6D,
                                    0xF2, 0x5F, 0x14, 0x37,
                                    0x4F, 0xE1, 0x35, 0x6D,
                                    0x6D, 0x51, 0xC2, 0x45,
                                    0xE4, 0x85, 0xB5, 0x76,
                                    0x62, 0x5E, 0x7E, 0xC6,
                                    0xF4, 0x4C, 0x42, 0xE9,
                                    0xA6, 0x37, 0xED, 0x6B,
                                    0x0B, 0xFF, 0x5C, 0xB6,
                                    0xF4, 0x06, 0xB7, 0xED,
                                    0xEE, 0x38, 0x6B, 0xFB,
                                    0x5A, 0x89, 0x9F, 0xA5,
                                    0xAE, 0x9F, 0x24, 0x11,
                                    0x7C, 0x4B, 0x1F, 0xE6,
                                    0x49, 0x28, 0x66, 0x51,
                                    0xEC, 0xE4, 0x5B, 0x3D,
                                    0xC2, 0x00, 0x7C, 0xB8,
                                    0xA1, 0x63, 0xBF, 0x05,
                                    0x98, 0xDA, 0x48, 0x36,
                                    0x1C, 0x55, 0xD3, 0x9A,
                                    0x69, 0x16, 0x3F, 0xA8,
                                    0xFD, 0x24, 0xCF, 0x5F,
                                    0x83, 0x65, 0x5D, 0x23,
                                    0xDC, 0xA3, 0xAD, 0x96,
                                    0x1C, 0x62, 0xF3, 0x56,
                                    0x20, 0x85, 0x52, 0xBB,
                                    0x9E, 0xD5, 0x29, 0x07,
                                    0x70, 0x96, 0x96, 0x6D,
                                    0x67, 0x0C, 0x35, 0x4E,
                                    0x4A, 0xBC, 0x98, 0x04,
                                    0xF1, 0x74, 0x6C, 0x08,
                                    0xCA, 0x18, 0x21, 0x7C,
                                    0x32, 0x90, 0x5E, 0x46,
                                    0x2E, 0x36, 0xCE, 0x3B,
                                    0xE3, 0x9E, 0x77, 0x2C,
                                    0x18, 0x0E, 0x86, 0x03,
                                    0x9B, 0x27, 0x83, 0xA2,
                                    0xEC, 0x07, 0xA2, 0x8F,
                                    0xB5, 0xC5, 0x5D, 0xF0,
                                    0x6F, 0x4C, 0x52, 0xC9,
                                    0xDE, 0x2B, 0xCB, 0xF6,
                                    0x95, 0x58, 0x17, 0x18,
                                    0x39, 0x95, 0x49, 0x7C,
                                    0xEA, 0x95, 0x6A, 0xE5,
                                    0x15, 0xD2, 0x26, 0x18,
                                    0x98, 0xFA, 0x05, 0x10,
                                    0x15, 0x72, 0x8E, 0x5A,
                                    0x8A, 0xAA, 0xC4, 0x2D,
                                    0xAD, 0x33, 0x17, 0x0D,
                                    0x04, 0x50, 0x7A, 0x33,
                                    0xA8, 0x55, 0x21, 0xAB,
                                    0xDF, 0x1C, 0xBA, 0x64,
                                    0xEC, 0xFB, 0x85, 0x04,
                                    0x58, 0xDB, 0xEF, 0x0A,
                                    0x8A, 0xEA, 0x71, 0x57,
                                    0x5D, 0x06, 0x0C, 0x7D,
                                    0xB3, 0x97, 0x0F, 0x85,
                                    0xA6, 0xE1, 0xE4, 0xC7,
                                    0xAB, 0xF5, 0xAE, 0x8C,
                                    0xDB, 0x09, 0x33, 0xD7,
                                    0x1E, 0x8C, 0x94, 0xE0,
                                    0x4A, 0x25, 0x61, 0x9D,
                                    0xCE, 0xE3, 0xD2, 0x26,
                                    0x1A, 0xD2, 0xEE, 0x6B,
                                    0xF1, 0x2F, 0xFA, 0x06,
                                    0xD9, 0x8A, 0x08, 0x64,
                                    0xD8, 0x76, 0x02, 0x73,
                                    0x3E, 0xC8, 0x6A, 0x64,
                                    0x52, 0x1F, 0x2B, 0x18,
                                    0x17, 0x7B, 0x20, 0x0C,
                                    0xBB, 0xE1, 0x17, 0x57,
                                    0x7A, 0x61, 0x5D, 0x6C,
                                    0x77, 0x09, 0x88, 0xC0,
                                    0xBA, 0xD9, 0x46, 0xE2,
                                    0x08, 0xE2, 0x4F, 0xA0,
                                    0x74, 0xE5, 0xAB, 0x31,
                                    0x43, 0xDB, 0x5B, 0xFC,
                                    0xE0, 0xFD, 0x10, 0x8E,
                                    0x4B, 0x82, 0xD1, 0x20,
                                    0xA9, 0x21, 0x08, 0x01,
                                    0x1A, 0x72, 0x3C, 0x12,
                                    0xA7, 0x87, 0xE6, 0xD7,
                                    0x88, 0x71, 0x9A, 0x10,
                                    0xBD, 0xBA, 0x5B, 0x26,
                                    0x99, 0xC3, 0x27, 0x18,
                                    0x6A, 0xF4, 0xE2, 0x3C,
                                    0x1A, 0x94, 0x68, 0x34,
                                    0xB6, 0x15, 0x0B, 0xDA,
                                    0x25, 0x83, 0xE9, 0xCA,
                                    0x2A, 0xD4, 0x4C, 0xE8,
                                    0xDB, 0xBB, 0xC2, 0xDB,
                                    0x04, 0xDE, 0x8E, 0xF9,
                                    0x2E, 0x8E, 0xFC, 0x14,
                                    0x1F, 0xBE, 0xCA, 0xA6,
                                    0x28, 0x7C, 0x59, 0x47,
                                    0x4E, 0x6B, 0xC0, 0x5D,
                                    0x99, 0xB2, 0x96, 0x4F,
                                    0xA0, 0x90, 0xC3, 0xA2,
                                    0x23, 0x3B, 0xA1, 0x86,
                                    0x51, 0x5B, 0xE7, 0xED,
                                    0x1F, 0x61, 0x29, 0x70,
                                    0xCE, 0xE2, 0xD7, 0xAF,
                                    0xB8, 0x1B, 0xDD, 0x76,
                                    0x21, 0x70, 0x48, 0x1C,
                                    0xD0, 0x06, 0x91, 0x27,
                                    0xD5, 0xB0, 0x5A, 0xA9,
                                    0x93, 0xB4, 0xEA, 0x98,
                                    0x8D, 0x8F, 0xDD, 0xC1,
                                    0x86, 0xFF, 0xB7, 0xDC,
                                    0x90, 0xA6, 0xC0, 0x8F,
                                    0x4D, 0xF4, 0x35, 0xC9,
                                    0x34, 0x06, 0x31, 0x99,
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0xFF, 0xFF, 0xFF, 0xFF,
                                    0x00 //to keep BigInteger positive
                                };

        #endregion

        #region variables

        DiffieHellmanGroupType _group;
        int _keySize;
        BigInteger _p;
        BigInteger _g;

        #endregion

        #region constructor

        private DiffieHellmanGroup(DiffieHellmanGroupType group, int keySize, BigInteger p, BigInteger g)
        {
            _group = group;
            _keySize = keySize;
            _p = p;
            _g = g;
        }

        #endregion

        #region shared

        public static DiffieHellmanGroup GetGroup(DiffieHellmanGroupType group, int keySize)
        {
            switch (group)
            {
                case DiffieHellmanGroupType.RFC3526:
                    switch (keySize)
                    {
                        case 2048:
                            return new DiffieHellmanGroup(group, keySize, new BigInteger(p2048), new BigInteger(2));

                        case 3072:
                            return new DiffieHellmanGroup(group, keySize, new BigInteger(p3072), new BigInteger(2));

                        case 4096:
                            return new DiffieHellmanGroup(group, keySize, new BigInteger(p4096), new BigInteger(2));

                        default:
                            throw new CryptoException("DiffieHellman key size not supported.");
                    }

                default:
                    throw new CryptoException("DiffieHellman group not supported.");
            }
        }

        #endregion

        #region properties

        public int KeySize
        { get { return _keySize; } }

        public DiffieHellmanGroupType Group
        { get { return _group; } }

        public BigInteger P
        { get { return _p; } }

        public BigInteger G
        { get { return _g; } }

        #endregion
    }
}
