using Wist.Core.ExtensionMethods;
using Chaos.NaCl;
using Chaos.NaCl.Internal.Ed25519Ref10;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Binary;
using Org.BouncyCastle.Crypto.Digests;
using Wist.Crypto.Experiment.ConfidentialAssets;
using HashLib;

namespace Wist.Crypto.Experiment
{
    class Program
    {
        public static Key I = new Key { Bytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };

        public static Key H = new Key{ Bytes = new byte[] { 0x8b, 0x65, 0x59, 0x70, 0x15, 0x37, 0x99, 0xaf, 0x2a, 0xea, 0xdc, 0x9f, 0xf1, 0xad, 0xd0, 0xea, 0x6c, 0x72, 0x51, 0xd5, 0x41, 0x54, 0xcf, 0xa9, 0x2c, 0x17, 0x3a, 0x0d, 0xd3, 0x9c, 0x1f, 0x94 } };

        //H2 contains 2^i H in each index, i.e. H, 2H, 4H, 8H, ...
        //This is used for the range proofG
        //You can regenerate this by running python2 Test.py HPow2 in the MiniNero repo
        public static Key64 H2 = new Key64
        {
            Keys = new Key[] {
                new Key { Bytes = new byte[] {0x8b, 0x65, 0x59, 0x70, 0x15, 0x37, 0x99, 0xaf, 0x2a, 0xea, 0xdc, 0x9f, 0xf1, 0xad, 0xd0, 0xea, 0x6c, 0x72, 0x51, 0xd5, 0x41, 0x54, 0xcf, 0xa9, 0x2c, 0x17, 0x3a, 0x0d, 0xd3, 0x9c, 0x1f, 0x94}},
                new Key { Bytes = new byte[] {0x8f, 0xaa, 0x44, 0x8a, 0xe4, 0xb3, 0xe2, 0xbb, 0x3d, 0x4d, 0x13, 0x09, 0x09, 0xf5, 0x5f, 0xcd, 0x79, 0x71, 0x1c, 0x1c, 0x83, 0xcd, 0xbc, 0xca, 0xdd, 0x42, 0xcb, 0xe1, 0x51, 0x5e, 0x87, 0x12}},
                new Key { Bytes = new byte[] {0x12, 0xa7, 0xd6, 0x2c, 0x77, 0x91, 0x65, 0x4a, 0x57, 0xf3, 0xe6, 0x76, 0x94, 0xed, 0x50, 0xb4, 0x9a, 0x7d, 0x9e, 0x3f, 0xc1, 0xe4, 0xc7, 0xa0, 0xbd, 0xe2, 0x9d, 0x18, 0x7e, 0x9c, 0xc7, 0x1d}},
                new Key { Bytes = new byte[] {0x78, 0x9a, 0xb9, 0x93, 0x4b, 0x49, 0xc4, 0xf9, 0xe6, 0x78, 0x5c, 0x6d, 0x57, 0xa4, 0x98, 0xb3, 0xea, 0xd4, 0x43, 0xf0, 0x4f, 0x13, 0xdf, 0x11, 0x0c, 0x54, 0x27, 0xb4, 0xf2, 0x14, 0xc7, 0x39}},
                new Key { Bytes = new byte[] {0x77, 0x1e, 0x92, 0x99, 0xd9, 0x4f, 0x02, 0xac, 0x72, 0xe3, 0x8e, 0x44, 0xde, 0x56, 0x8a, 0xc1, 0xdc, 0xb2, 0xed, 0xc6, 0xed, 0xb6, 0x1f, 0x83, 0xca, 0x41, 0x8e, 0x10, 0x77, 0xce, 0x3d, 0xe8}},
                new Key { Bytes = new byte[] {0x73, 0xb9, 0x6d, 0xb4, 0x30, 0x39, 0x81, 0x9b, 0xda, 0xf5, 0x68, 0x0e, 0x5c, 0x32, 0xd7, 0x41, 0x48, 0x88, 0x84, 0xd1, 0x8d, 0x93, 0x86, 0x6d, 0x40, 0x74, 0xa8, 0x49, 0x18, 0x2a, 0x8a, 0x64}},
                new Key { Bytes = new byte[] {0x8d, 0x45, 0x8e, 0x1c, 0x2f, 0x68, 0xeb, 0xeb, 0xcc, 0xd2, 0xfd, 0x5d, 0x37, 0x9f, 0x5e, 0x58, 0xf8, 0x13, 0x4d, 0xf3, 0xe0, 0xe8, 0x8c, 0xad, 0x3d, 0x46, 0x70, 0x10, 0x63, 0xa8, 0xd4, 0x12}},
                new Key { Bytes = new byte[] {0x09, 0x55, 0x1e, 0xdb, 0xe4, 0x94, 0x41, 0x8e, 0x81, 0x28, 0x44, 0x55, 0xd6, 0x4b, 0x35, 0xee, 0x8a, 0xc0, 0x93, 0x06, 0x8a, 0x5f, 0x16, 0x1f, 0xa6, 0x63, 0x75, 0x59, 0x17, 0x7e, 0xf4, 0x04}},
                new Key { Bytes = new byte[] {0xd0, 0x5a, 0x88, 0x66, 0xf4, 0xdf, 0x8c, 0xee, 0x1e, 0x26, 0x8b, 0x1d, 0x23, 0xa4, 0xc5, 0x8c, 0x92, 0xe7, 0x60, 0x30, 0x97, 0x86, 0xcd, 0xac, 0x0f, 0xed, 0xa1, 0xd2, 0x47, 0xa9, 0xc9, 0xa7}},
                new Key { Bytes = new byte[] {0x55, 0xcd, 0xaa, 0xd5, 0x18, 0xbd, 0x87, 0x1d, 0xd1, 0xeb, 0x7b, 0xc7, 0x02, 0x3e, 0x1d, 0xc0, 0xfd, 0xf3, 0x33, 0x98, 0x64, 0xf8, 0x8f, 0xdd, 0x2d, 0xe2, 0x69, 0xfe, 0x9e, 0xe1, 0x83, 0x2d}},
                new Key { Bytes = new byte[] {0xe7, 0x69, 0x7e, 0x95, 0x1a, 0x98, 0xcf, 0xd5, 0x71, 0x2b, 0x84, 0xbb, 0xe5, 0xf3, 0x4e, 0xd7, 0x33, 0xe9, 0x47, 0x3f, 0xcb, 0x68, 0xed, 0xa6, 0x6e, 0x37, 0x88, 0xdf, 0x19, 0x58, 0xc3, 0x06}},
                new Key { Bytes = new byte[] {0xf9, 0x2a, 0x97, 0x0b, 0xae, 0x72, 0x78, 0x29, 0x89, 0xbf, 0xc8, 0x3a, 0xdf, 0xaa, 0x92, 0xa4, 0xf4, 0x9c, 0x7e, 0x95, 0x91, 0x8b, 0x3b, 0xba, 0x3c, 0xdc, 0x7f, 0xe8, 0x8a, 0xcc, 0x8d, 0x47}},
                new Key { Bytes = new byte[] {0x1f, 0x66, 0xc2, 0xd4, 0x91, 0xd7, 0x5a, 0xf9, 0x15, 0xc8, 0xdb, 0x6a, 0x6d, 0x1c, 0xb0, 0xcd, 0x4f, 0x7d, 0xdc, 0xd5, 0xe6, 0x3d, 0x3b, 0xa9, 0xb8, 0x3c, 0x86, 0x6c, 0x39, 0xef, 0x3a, 0x2b}},
                new Key { Bytes = new byte[] {0x3e, 0xec, 0x98, 0x84, 0xb4, 0x3f, 0x58, 0xe9, 0x3e, 0xf8, 0xde, 0xea, 0x26, 0x00, 0x04, 0xef, 0xea, 0x2a, 0x46, 0x34, 0x4f, 0xc5, 0x96, 0x5b, 0x1a, 0x7d, 0xd5, 0xd1, 0x89, 0x97, 0xef, 0xa7}},
                new Key { Bytes = new byte[] {0xb2, 0x9f, 0x8f, 0x0c, 0xcb, 0x96, 0x97, 0x7f, 0xe7, 0x77, 0xd4, 0x89, 0xd6, 0xbe, 0x9e, 0x7e, 0xbc, 0x19, 0xc4, 0x09, 0xb5, 0x10, 0x35, 0x68, 0xf2, 0x77, 0x61, 0x1d, 0x7e, 0xa8, 0x48, 0x94}},
                new Key { Bytes = new byte[] {0x56, 0xb1, 0xf5, 0x12, 0x65, 0xb9, 0x55, 0x98, 0x76, 0xd5, 0x8d, 0x24, 0x9d, 0x0c, 0x14, 0x6d, 0x69, 0xa1, 0x03, 0x63, 0x66, 0x99, 0x87, 0x4d, 0x3f, 0x90, 0x47, 0x35, 0x50, 0xfe, 0x3f, 0x2c}},
                new Key { Bytes = new byte[] {0x1d, 0x7a, 0x36, 0x57, 0x5e, 0x22, 0xf5, 0xd1, 0x39, 0xff, 0x9c, 0xc5, 0x10, 0xfa, 0x13, 0x85, 0x05, 0x57, 0x6b, 0x63, 0x81, 0x5a, 0x94, 0xe4, 0xb0, 0x12, 0xbf, 0xd4, 0x57, 0xca, 0xaa, 0xda}},
                new Key { Bytes = new byte[] {0xd0, 0xac, 0x50, 0x7a, 0x86, 0x4e, 0xcd, 0x05, 0x93, 0xfa, 0x67, 0xbe, 0x7d, 0x23, 0x13, 0x43, 0x92, 0xd0, 0x0e, 0x40, 0x07, 0xe2, 0x53, 0x48, 0x78, 0xd9, 0xb2, 0x42, 0xe1, 0x0d, 0x76, 0x20}},
                new Key { Bytes = new byte[] {0xf6, 0xc6, 0x84, 0x0b, 0x9c, 0xf1, 0x45, 0xbb, 0x2d, 0xcc, 0xf8, 0x6e, 0x94, 0x0b, 0xe0, 0xfc, 0x09, 0x8e, 0x32, 0xe3, 0x10, 0x99, 0xd5, 0x6f, 0x7f, 0xe0, 0x87, 0xbd, 0x5d, 0xeb, 0x50, 0x94}},
                new Key { Bytes = new byte[] {0x28, 0x83, 0x1a, 0x33, 0x40, 0x07, 0x0e, 0xb1, 0xdb, 0x87, 0xc1, 0x2e, 0x05, 0x98, 0x0d, 0x5f, 0x33, 0xe9, 0xef, 0x90, 0xf8, 0x3a, 0x48, 0x17, 0xc9, 0xf4, 0xa0, 0xa3, 0x32, 0x27, 0xe1, 0x97}},
                new Key { Bytes = new byte[] {0x87, 0x63, 0x22, 0x73, 0xd6, 0x29, 0xcc, 0xb7, 0xe1, 0xed, 0x1a, 0x76, 0x8f, 0xa2, 0xeb, 0xd5, 0x17, 0x60, 0xf3, 0x2e, 0x1c, 0x0b, 0x86, 0x7a, 0x5d, 0x36, 0x8d, 0x52, 0x71, 0x05, 0x5c, 0x6e}},
                new Key { Bytes = new byte[] {0x5c, 0x7b, 0x29, 0x42, 0x43, 0x47, 0x96, 0x4d, 0x04, 0x27, 0x55, 0x17, 0xc5, 0xae, 0x14, 0xb6, 0xb5, 0xea, 0x27, 0x98, 0xb5, 0x73, 0xfc, 0x94, 0xe6, 0xe4, 0x4a, 0x53, 0x21, 0x60, 0x0c, 0xfb}},
                new Key { Bytes = new byte[] {0xe6, 0x94, 0x50, 0x42, 0xd7, 0x8b, 0xc2, 0xc3, 0xbd, 0x6e, 0xc5, 0x8c, 0x51, 0x1a, 0x9f, 0xe8, 0x59, 0xc0, 0xad, 0x63, 0xfd, 0xe4, 0x94, 0xf5, 0x03, 0x9e, 0x0e, 0x82, 0x32, 0x61, 0x2b, 0xd5}},
                new Key { Bytes = new byte[] {0x36, 0xd5, 0x69, 0x07, 0xe2, 0xec, 0x74, 0x5d, 0xb6, 0xe5, 0x4f, 0x0b, 0x2e, 0x1b, 0x23, 0x00, 0xab, 0xcb, 0x42, 0x2e, 0x71, 0x2d, 0xa5, 0x88, 0xa4, 0x0d, 0x3f, 0x1e, 0xbb, 0xbe, 0x02, 0xf6}},
                new Key { Bytes = new byte[] {0x34, 0xdb, 0x6e, 0xe4, 0xd0, 0x60, 0x8e, 0x5f, 0x78, 0x36, 0x50, 0x49, 0x5a, 0x3b, 0x2f, 0x52, 0x73, 0xc5, 0x13, 0x4e, 0x52, 0x84, 0xe4, 0xfd, 0xf9, 0x66, 0x27, 0xbb, 0x16, 0xe3, 0x1e, 0x6b}},
                new Key { Bytes = new byte[] {0x8e, 0x76, 0x59, 0xfb, 0x45, 0xa3, 0x78, 0x7d, 0x67, 0x4a, 0xe8, 0x67, 0x31, 0xfa, 0xa2, 0x53, 0x8e, 0xc0, 0xfd, 0xf4, 0x42, 0xab, 0x26, 0xe9, 0xc7, 0x91, 0xfa, 0xda, 0x08, 0x94, 0x67, 0xe9}},
                new Key { Bytes = new byte[] {0x30, 0x06, 0xcf, 0x19, 0x8b, 0x24, 0xf3, 0x1b, 0xb4, 0xc7, 0xe6, 0x34, 0x60, 0x00, 0xab, 0xc7, 0x01, 0xe8, 0x27, 0xcf, 0xbb, 0x5d, 0xf5, 0x2d, 0xcf, 0xa4, 0x2e, 0x9c, 0xa9, 0xff, 0x08, 0x02}},
                new Key { Bytes = new byte[] {0xf5, 0xfd, 0x40, 0x3c, 0xb6, 0xe8, 0xbe, 0x21, 0x47, 0x2e, 0x37, 0x7f, 0xfd, 0x80, 0x5a, 0x8c, 0x60, 0x83, 0xea, 0x48, 0x03, 0xb8, 0x48, 0x53, 0x89, 0xcc, 0x3e, 0xbc, 0x21, 0x5f, 0x00, 0x2a}},
                new Key { Bytes = new byte[] {0x37, 0x31, 0xb2, 0x60, 0xeb, 0x3f, 0x94, 0x82, 0xe4, 0x5f, 0x1c, 0x3f, 0x3b, 0x9d, 0xcf, 0x83, 0x4b, 0x75, 0xe6, 0xee, 0xf8, 0xc4, 0x0f, 0x46, 0x1e, 0xa2, 0x7e, 0x8b, 0x6e, 0xd9, 0x47, 0x3d}},
                new Key { Bytes = new byte[] {0x9f, 0x9d, 0xab, 0x09, 0xc3, 0xf5, 0xe4, 0x28, 0x55, 0xc2, 0xde, 0x97, 0x1b, 0x65, 0x93, 0x28, 0xa2, 0xdb, 0xc4, 0x54, 0x84, 0x5f, 0x39, 0x6f, 0xfc, 0x05, 0x3f, 0x0b, 0xb1, 0x92, 0xf8, 0xc3}},
                new Key { Bytes = new byte[] {0x5e, 0x05, 0x5d, 0x25, 0xf8, 0x5f, 0xdb, 0x98, 0xf2, 0x73, 0xe4, 0xaf, 0xe0, 0x84, 0x64, 0xc0, 0x03, 0xb7, 0x0f, 0x1e, 0xf0, 0x67, 0x7b, 0xb5, 0xe2, 0x57, 0x06, 0x40, 0x0b, 0xe6, 0x20, 0xa5}},
                new Key { Bytes = new byte[] {0x86, 0x8b, 0xcf, 0x36, 0x79, 0xcb, 0x6b, 0x50, 0x0b, 0x94, 0x41, 0x8c, 0x0b, 0x89, 0x25, 0xf9, 0x86, 0x55, 0x30, 0x30, 0x3a, 0xe4, 0xe4, 0xb2, 0x62, 0x59, 0x18, 0x65, 0x66, 0x6a, 0x45, 0x90}},
                new Key { Bytes = new byte[] {0xb3, 0xdb, 0x6b, 0xd3, 0x89, 0x7a, 0xfb, 0xd1, 0xdf, 0x3f, 0x96, 0x44, 0xab, 0x21, 0xc8, 0x05, 0x0e, 0x1f, 0x00, 0x38, 0xa5, 0x2f, 0x7c, 0xa9, 0x5a, 0xc0, 0xc3, 0xde, 0x75, 0x58, 0xcb, 0x7a}},
                new Key { Bytes = new byte[] {0x81, 0x19, 0xb3, 0xa0, 0x59, 0xff, 0x2c, 0xac, 0x48, 0x3e, 0x69, 0xbc, 0xd4, 0x1d, 0x6d, 0x27, 0x14, 0x94, 0x47, 0x91, 0x42, 0x88, 0xbb, 0xea, 0xee, 0x34, 0x13, 0xe6, 0xdc, 0xc6, 0xd1, 0xeb}},
                new Key { Bytes = new byte[] {0x10, 0xfc, 0x58, 0xf3, 0x5f, 0xc7, 0xfe, 0x7a, 0xe8, 0x75, 0x52, 0x4b, 0xb5, 0x85, 0x00, 0x03, 0x00, 0x5b, 0x7f, 0x97, 0x8c, 0x0c, 0x65, 0xe2, 0xa9, 0x65, 0x46, 0x4b, 0x6d, 0x00, 0x81, 0x9c}},
                new Key { Bytes = new byte[] {0x5a, 0xcd, 0x94, 0xeb, 0x3c, 0x57, 0x83, 0x79, 0xc1, 0xea, 0x58, 0xa3, 0x43, 0xec, 0x4f, 0xcf, 0xf9, 0x62, 0x77, 0x6f, 0xe3, 0x55, 0x21, 0xe4, 0x75, 0xa0, 0xe0, 0x6d, 0x88, 0x7b, 0x2d, 0xb9}},
                new Key { Bytes = new byte[] {0x33, 0xda, 0xf3, 0xa2, 0x14, 0xd6, 0xe0, 0xd4, 0x2d, 0x23, 0x00, 0xa7, 0xb4, 0x4b, 0x39, 0x29, 0x0d, 0xb8, 0x98, 0x9b, 0x42, 0x79, 0x74, 0xcd, 0x86, 0x5d, 0xb0, 0x11, 0x05, 0x5a, 0x29, 0x01}},
                new Key { Bytes = new byte[] {0xcf, 0xc6, 0x57, 0x2f, 0x29, 0xaf, 0xd1, 0x64, 0xa4, 0x94, 0xe6, 0x4e, 0x6f, 0x1a, 0xeb, 0x82, 0x0c, 0x3e, 0x7d, 0xa3, 0x55, 0x14, 0x4e, 0x51, 0x24, 0xa3, 0x91, 0xd0, 0x6e, 0x9f, 0x95, 0xea}},
                new Key { Bytes = new byte[] {0xd5, 0x31, 0x2a, 0x4b, 0x0, 0xf6, 0x15, 0xa3, 0x31, 0xf6, 0x35, 0x2c, 0x2e, 0xd2, 0x1d, 0xac, 0x9e, 0x7c, 0x36, 0x39, 0x8b, 0x93, 0x9a, 0xec, 0x90, 0x1c, 0x25, 0x7f, 0x6c, 0xbc, 0x9e, 0x8e}},
                new Key { Bytes = new byte[] {0x55, 0x1d, 0x67, 0xfe, 0xfc, 0x7b, 0x5b, 0x9f, 0x9f, 0xdb, 0xf6, 0xaf, 0x57, 0xc9, 0x6c, 0x8a, 0x74, 0xd7, 0xe4, 0x5a, 0x00, 0x20, 0x78, 0xa7, 0xb5, 0xba, 0x45, 0xc6, 0xfd, 0xe9, 0x3e, 0x33}},
                new Key { Bytes = new byte[] {0xd5, 0x0a, 0xc7, 0xbd, 0x5c, 0xa5, 0x93, 0xc6, 0x56, 0x92, 0x8f, 0x38, 0x42, 0x80, 0x17, 0xfc, 0x7b, 0xa5, 0x02, 0x85, 0x4c, 0x43, 0xd8, 0x41, 0x49, 0x50, 0xe9, 0x6e, 0xcb, 0x40, 0x5d, 0xc3}},
                new Key { Bytes = new byte[] {0x07, 0x73, 0xe1, 0x8e, 0xa1, 0xbe, 0x44, 0xfe, 0x1a, 0x97, 0xe2, 0x39, 0x57, 0x3c, 0xfa, 0xe3, 0xe4, 0xe9, 0x5e, 0xf9, 0xaa, 0x9f, 0xaa, 0xbe, 0xac, 0x12, 0x74, 0xd3, 0xad, 0x26, 0x16, 0x04}},
                new Key { Bytes = new byte[] {0xe9, 0xaf, 0x0e, 0x7c, 0xa8, 0x93, 0x30, 0xd2, 0xb8, 0x61, 0x5d, 0x1b, 0x41, 0x37, 0xca, 0x61, 0x7e, 0x21, 0x29, 0x7f, 0x2f, 0x0d, 0xed, 0x8e, 0x31, 0xb7, 0xd2, 0xea, 0xd8, 0x71, 0x46, 0x60}},
                new Key { Bytes = new byte[] {0x7b, 0x12, 0x45, 0x83, 0x09, 0x7f, 0x10, 0x29, 0xa0, 0xc7, 0x41, 0x91, 0xfe, 0x73, 0x78, 0xc9, 0x10, 0x5a, 0xcc, 0x70, 0x66, 0x95, 0xed, 0x14, 0x93, 0xbb, 0x76, 0x03, 0x42, 0x26, 0xa5, 0x7b}},
                new Key { Bytes = new byte[] {0xec, 0x40, 0x05, 0x7b, 0x99, 0x54, 0x76, 0x65, 0x0b, 0x3d, 0xb9, 0x8e, 0x9d, 0xb7, 0x57, 0x38, 0xa8, 0xcd, 0x2f, 0x94, 0xd8, 0x63, 0xb9, 0x06, 0x15, 0x0c, 0x56, 0xaa, 0xc1, 0x9c, 0xaa, 0x6b}},
                new Key { Bytes = new byte[] {0x01, 0xd9, 0xff, 0x72, 0x9e, 0xfd, 0x39, 0xd8, 0x37, 0x84, 0xc0, 0xfe, 0x59, 0xc4, 0xae, 0x81, 0xa6, 0x70, 0x34, 0xcb, 0x53, 0xc9, 0x43, 0xfb, 0x81, 0x8b, 0x9d, 0x8a, 0xe7, 0xfc, 0x33, 0xe5}},
                new Key { Bytes = new byte[] {0x00, 0xdf, 0xb3, 0xc6, 0x96, 0x32, 0x8c, 0x76, 0x42, 0x45, 0x19, 0xa7, 0xbe, 0xfe, 0x8e, 0x0f, 0x6c, 0x76, 0xf9, 0x47, 0xb5, 0x27, 0x67, 0x91, 0x6d, 0x24, 0x82, 0x3f, 0x73, 0x5b, 0xaf, 0x2e}},
                new Key { Bytes = new byte[] {0x46, 0x1b, 0x79, 0x9b, 0x4d, 0x9c, 0xee, 0xa8, 0xd5, 0x80, 0xdc, 0xb7, 0x6d, 0x11, 0x15, 0x0d, 0x53, 0x5e, 0x16, 0x39, 0xd1, 0x60, 0x03, 0xc3, 0xfb, 0x7e, 0x9d, 0x1f, 0xd1, 0x30, 0x83, 0xa8}},
                new Key { Bytes = new byte[] {0xee, 0x03, 0x03, 0x94, 0x79, 0xe5, 0x22, 0x8f, 0xdc, 0x55, 0x1c, 0xbd, 0xe7, 0x07, 0x9d, 0x34, 0x12, 0xea, 0x18, 0x6a, 0x51, 0x7c, 0xcc, 0x63, 0xe4, 0x6e, 0x9f, 0xcc, 0xe4, 0xfe, 0x3a, 0x6c}},
                new Key { Bytes = new byte[] {0xa8, 0xcf, 0xb5, 0x43, 0x52, 0x4e, 0x7f, 0x02, 0xb9, 0xf0, 0x45, 0xac, 0xd5, 0x43, 0xc2, 0x1c, 0x37, 0x3b, 0x4c, 0x9b, 0x98, 0xac, 0x20, 0xce, 0xc4, 0x17, 0xa6, 0xdd, 0xb5, 0x74, 0x4e, 0x94}},
                new Key { Bytes = new byte[] {0x93, 0x2b, 0x79, 0x4b, 0xf8, 0x9c, 0x6e, 0xda, 0xf5, 0xd0, 0x65, 0x0c, 0x7c, 0x4b, 0xad, 0x92, 0x42, 0xb2, 0x56, 0x26, 0xe3, 0x7e, 0xad, 0x5a, 0xa7, 0x5e, 0xc8, 0xc6, 0x4e, 0x09, 0xdd, 0x4f}},
                new Key { Bytes = new byte[] {0x16, 0xb1, 0x0c, 0x77, 0x9c, 0xe5, 0xcf, 0xef, 0x59, 0xc7, 0x71, 0x0d, 0x2e, 0x68, 0x44, 0x1e, 0xa6, 0xfa, 0xcb, 0x68, 0xe9, 0xb5, 0xf7, 0xd5, 0x33, 0xae, 0x0b, 0xb7, 0x8e, 0x28, 0xbf, 0x57}},
                new Key { Bytes = new byte[] {0x0f, 0x77, 0xc7, 0x67, 0x43, 0xe7, 0x39, 0x6f, 0x99, 0x10, 0x13, 0x9f, 0x49, 0x37, 0xd8, 0x37, 0xae, 0x54, 0xe2, 0x10, 0x38, 0xac, 0x5c, 0x0b, 0x3f, 0xd6, 0xef, 0x17, 0x1a, 0x28, 0xa7, 0xe4}},
                new Key { Bytes = new byte[] {0xd7, 0xe5, 0x74, 0xb7, 0xb9, 0x52, 0xf2, 0x93, 0xe8, 0x0d, 0xde, 0x90, 0x5e, 0xb5, 0x09, 0x37, 0x3f, 0x3f, 0x6c, 0xd1, 0x09, 0xa0, 0x22, 0x08, 0xb3, 0xc1, 0xe9, 0x24, 0x08, 0x0a, 0x20, 0xca}},
                new Key { Bytes = new byte[] {0x45, 0x66, 0x6f, 0x8c, 0x38, 0x1e, 0x3d, 0xa6, 0x75, 0x56, 0x3f, 0xf8, 0xba, 0x23, 0xf8, 0x3b, 0xfa, 0xc3, 0x0c, 0x34, 0xab, 0xdd, 0xe6, 0xe5, 0xc0, 0x97, 0x5e, 0xf9, 0xfd, 0x70, 0x0c, 0xb9}},
                new Key { Bytes = new byte[] {0xb2, 0x46, 0x12, 0xe4, 0x54, 0x60, 0x7e, 0xb1, 0xab, 0xa4, 0x47, 0xf8, 0x16, 0xd1, 0xa4, 0x55, 0x1e, 0xf9, 0x5f, 0xa7, 0x24, 0x7f, 0xb7, 0xc1, 0xf5, 0x03, 0x02, 0x0a, 0x71, 0x77, 0xf0, 0xdd}},
                new Key { Bytes = new byte[] {0x7e, 0x20, 0x88, 0x61, 0x85, 0x6d, 0xa4, 0x2c, 0x8b, 0xb4, 0x6a, 0x75, 0x67, 0xf8, 0x12, 0x13, 0x62, 0xd9, 0xfb, 0x24, 0x96, 0xf1, 0x31, 0xa4, 0xaa, 0x90, 0x17, 0xcf, 0x36, 0x6c, 0xdf, 0xce}},
                new Key { Bytes = new byte[] {0x5b, 0x64, 0x6b, 0xff, 0x6a, 0xd1, 0x10, 0x01, 0x65, 0x03, 0x7a, 0x05, 0x56, 0x01, 0xea, 0x02, 0x35, 0x8c, 0x0f, 0x41, 0x05, 0x0f, 0x9d, 0xfe, 0x3c, 0x95, 0xdc, 0xcb, 0xd3, 0x08, 0x7b, 0xe0}},
                new Key { Bytes = new byte[] {0x74, 0x6d, 0x1d, 0xcc, 0xfe, 0xd2, 0xf0, 0xff, 0x1e, 0x13, 0xc5, 0x1e, 0x2d, 0x50, 0xd5, 0x32, 0x43, 0x75, 0xfb, 0xd5, 0xbf, 0x7c, 0xa8, 0x2a, 0x89, 0x31, 0x82, 0x8d, 0x80, 0x1d, 0x43, 0xab}},
                new Key { Bytes = new byte[] {0xcb, 0x98, 0x11, 0x0d, 0x4a, 0x6b, 0xb9, 0x7d, 0x22, 0xfe, 0xad, 0xbc, 0x6c, 0x0d, 0x89, 0x30, 0xc5, 0xf8, 0xfc, 0x50, 0x8b, 0x2f, 0xc5, 0xb3, 0x53, 0x28, 0xd2, 0x6b, 0x88, 0xdb, 0x19, 0xae}},
                new Key { Bytes = new byte[] {0x60, 0xb6, 0x26, 0xa0, 0x33, 0xb5, 0x5f, 0x27, 0xd7, 0x67, 0x6c, 0x40, 0x95, 0xea, 0xba, 0xbc, 0x7a, 0x2c, 0x7e, 0xde, 0x26, 0x24, 0xb4, 0x72, 0xe9, 0x7f, 0x64, 0xf9, 0x6b, 0x8c, 0xfc, 0x0e}},
                new Key { Bytes = new byte[] {0xe5, 0xb5, 0x2b, 0xc9, 0x27, 0x46, 0x8d, 0xf7, 0x18, 0x93, 0xeb, 0x81, 0x97, 0xef, 0x82, 0x0c, 0xf7, 0x6c, 0xb0, 0xaa, 0xf6, 0xe8, 0xe4, 0xfe, 0x93, 0xad, 0x62, 0xd8, 0x03, 0x98, 0x31, 0x04}},
                new Key { Bytes = new byte[] {0x05, 0x65, 0x41, 0xae, 0x5d, 0xa9, 0x96, 0x1b, 0xe2, 0xb0, 0xa5, 0xe8, 0x95, 0xe5, 0xc5, 0xba, 0x15, 0x3c, 0xbb, 0x62, 0xdd, 0x56, 0x1a, 0x42, 0x7b, 0xad, 0x0f, 0xfd, 0x41, 0x92, 0x31, 0x99}},
                new Key { Bytes = new byte[] {0xf8, 0xfe, 0xf0, 0x5a, 0x3f, 0xa5, 0xc9, 0xf3, 0xeb, 0xa4, 0x16, 0x38, 0xb2, 0x47, 0xb7, 0x11, 0xa9, 0x9f, 0x96, 0x0f, 0xe7, 0x3a, 0xa2, 0xf9, 0x01, 0x36, 0xae, 0xb2, 0x03, 0x29, 0xb8, 0x88}}
            }
        };

        public class RingCA
        {
            public byte[] DestinationPK { get; set; }
            public byte[] AssetCommitment { get; set; }
        }

        static void Main(string[] args)
        {
            ECParameters eCParameters = System.Security.Cryptography.ECDsa.Create().ExportExplicitParameters(true);
            byte[] testSeed = GetRandomSeed();

            

            GroupElementP3 p3_qq = ConfidentialAssetsHelper.CreateNonblindedAssetCommitment(testSeed);
            byte[] p3_bytes_1 = new byte[32];
            GroupOperations.ge_p3_tobytes(p3_bytes_1, 0, ref p3_qq);
            


            GroupOperations.ge_scalarmult_base(out GroupElementP3 p3test, testSeed, 0);
            GroupOperations.ge_p3_to_p2(out GroupElementP2 p2test, ref p3test);
            byte[] s1_p2 = new byte[32];
            GroupOperations.ge_tobytes(s1_p2, 0, ref p2test);

            byte[] s1 = new byte[32];
            GroupOperations.ge_p3_tobytes(s1, 0, ref p3test);

            GroupOperations.ge_frombytes_negate_vartime(out GroupElementP3 p3test_1, s1, 0);
            GroupOperations.ge_frombytes_negate_vartime(out GroupElementP3 p3test_2, s1_p2, 0);

            // 1. There is "record encryption key" - seems some random 32 byte array
            // 2. There is number of asset commitments from previous transaction where one of them is commitment of asset being transferred now
            // 3. Derive intermediate encryption key
            // 4. Derive asset encryption key
            // 5. Derive value encryption key - omit for now, assume value is always equals 1
            // 6. Create non blinded asset commitment
            // 7. Find index of asset commitment being transferred among all previous commitments
            // 8. Create blinded asset commitment
            // 9. Encrypt asset id
            // 10. Create asset range proof
            // 11. Produce asset descriptor
            byte[] sbytes = Encoding.ASCII.GetBytes("attack at dawn");
            byte[] tempHash = HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(sbytes).GetBytes();

            byte[] msg = new byte[] { 193, 222, 123, 49, 108, 175, 213, 232, 128, 114, 199, 60, 226, 220, 117, 65, 100, 159, 13, 194, 216, 126, 93, 35, 116, 173, 235, 165, 38, 84, 212, 68 };
            byte[] sk1 = new byte[] { 246, 184, 86, 53, 187, 51, 80, 143, 98, 224, 82, 139, 168, 34, 131, 77, 204, 201, 70, 229, 10, 204, 229, 179, 233, 164, 163, 45, 94, 201, 206, 6 };
            byte[] sk2 = GetRandomSeed();
            byte[][] pks = new byte[1][];
            GroupElementP3[] pks1 = new GroupElementP3[1];
            pks1[0] = MultiplyBasePoint(sk1);
            //pks[1] = MultiplyBasePoint(sk2);

            RingSignature rs = ConfidentialAssetsHelper.CreateRingSignature(msg, pks1, 0, sk1);
            bool res1 = ConfidentialAssetsHelper.VerifyRingSignature(rs, msg, pks1);

            int totalAssets = 1;
            int transferredAssetIndex = 0;
            byte[][] assetIds = new byte[totalAssets][];

            for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = GetRandomSeed();
            }

            GroupElementP3[] nonBlindedAssetCommitments = new GroupElementP3[totalAssets];

            for (int i = 0; i < totalAssets; i++)
            {
                nonBlindedAssetCommitments[i] = ConfidentialAssetsHelper.CreateNonblindedAssetCommitment(assetIds[i]);
            }

            byte[][] blindingFactors = new byte[totalAssets][];
            for (int i = 0; i < totalAssets; i++)
            {
                blindingFactors[i] = GetRandomSeed();
            }

            GroupElementP3[] blindedAssetCommitments = new GroupElementP3[totalAssets];
            for (int i = 0; i < totalAssets; i++)
            {
                blindedAssetCommitments[i] = ConfidentialAssetsHelper.BlindAssetCommitment(nonBlindedAssetCommitments[i], blindingFactors[i]);
            }

            byte[] recordEncryptionKey = GetRandomSeed();
            byte[] iek = ConfidentialAssetsHelper.DeriveIntermediateKey(recordEncryptionKey);
            byte[] aek = ConfidentialAssetsHelper.DeriveAssetKey(iek);

            GroupElementP3 newBlindedAssetCommitment = ConfidentialAssetsHelper.CreateBlindedAssetCommitment(blindedAssetCommitments[transferredAssetIndex], blindingFactors[transferredAssetIndex], aek, out byte[] newBlindingFactor);
            EncryptedAssetID encryptedAssetID = ConfidentialAssetsHelper.EncryptAssetId(assetIds[transferredAssetIndex], newBlindedAssetCommitment, newBlindingFactor, aek);
            AssetRangeProof assetRangeProof = ConfidentialAssetsHelper.CreateAssetRangeProof(newBlindedAssetCommitment, encryptedAssetID, blindedAssetCommitments, transferredAssetIndex, newBlindingFactor);

            bool res = ConfidentialAssetsHelper.VerifyAssetRangeProof(assetRangeProof, newBlindedAssetCommitment, encryptedAssetID);





            return;
            // Confidential Transaction Commitment:
            // c = x*G + b*H = X + B
            // c = x*G + a*I = X + A
            // X = x*G
            // B = b*H
            // A = a*I
            // G - Ed25519 base point (LookupTables.Base)
            // H - cryptographic hash of G (LookupTables.BaseH)
            // I - another point of group G representing code of asset

            Key c = new Key();
            Key mask = new Key();
            RangeSig rangeSig = GetRangeSig(c, mask, 10);

            GroupElementP3 asset1_P3 = GetAsset(1);
            GroupElementP3 asset2_P3 = GetAsset(2);



            byte[] blindingInSeed = GetRandomSeed();
            byte[] blindingOutSeed = GetRandomSeed();

            GroupElementP3 in1F_P3;
            GroupOperations.ge_scalarmult_base(out in1F_P3, blindingInSeed, 0);
            GroupElementP3 out1F_P3;
            GroupOperations.ge_scalarmult_base(out out1F_P3, blindingOutSeed, 0);

            GroupElementCached in1F_Cached;
            GroupOperations.ge_p3_to_cached(out in1F_Cached, ref in1F_P3);

            GroupElementP1P1 out1_in1_Diff_P1P1;
            GroupOperations.ge_sub(out out1_in1_Diff_P1P1, ref out1F_P3, ref in1F_Cached);

            GroupElementP2 out1_in1_diff_P2;
            GroupOperations.ge_p1p1_to_p2(out out1_in1_diff_P2, ref out1_in1_Diff_P1P1);
            byte[] out1_in1_diff_P2_bytes = new byte[64];
            GroupOperations.ge_tobytes(out1_in1_diff_P2_bytes, 0, ref out1_in1_diff_P2);


            BigInteger blinding1Int_ = new BigInteger(blindingInSeed);
            BigInteger blinding2Int_ = new BigInteger(blindingOutSeed);
            BigInteger blindingsDiff_ = blinding2Int_ - blinding1Int_;
            byte[] blindingsSeedDiff = blindingsDiff_.ToByteArray();
            byte[] blindingsSeedDiff32 = new byte[32];
            Array.Copy(blindingsSeedDiff, 0, blindingsSeedDiff32, 0, Math.Min(blindingsSeedDiff.Length, blindingsSeedDiff32.Length));
            GroupElementP3 blindingsSeedDiff_P3;
            GroupOperations.ge_scalarmult_base(out blindingsSeedDiff_P3, blindingsSeedDiff32, 0);
            byte[] blindingsSeedDiff_P3_bytes = new byte[64];
            GroupOperations.ge_p3_tobytes(blindingsSeedDiff_P3_bytes, 0, ref blindingsSeedDiff_P3);





            byte[] asset1Code;
            byte[] asset2Code;

            byte[] asset1;
            byte[] asset2;

            byte[] asset1Seed = GetRandomSeed();
            byte[] asset2Seed = GetRandomSeed();
            Ed25519.KeyPairFromSeed(out asset1, out asset1Code, asset1Seed);
            Ed25519.KeyPairFromSeed(out asset2, out asset2Code, asset2Seed);

            byte[] blinding1;
            byte[] blinding2;

            byte[] in1F;
            byte[] out1F;

            Ed25519.KeyPairFromSeed(out in1F, out blinding1, blindingInSeed);
            Ed25519.KeyPairFromSeed(out out1F, out blinding2, blindingOutSeed);

            BigInteger in1 = new BigInteger(in1F);
            BigInteger A1 = new BigInteger(asset1);

            in1 += A1;

            byte[] in1bytes = in1.ToByteArray();

            BigInteger out1 = new BigInteger(out1F);
            out1 += A1;

            byte[] out1bytes = out1.ToByteArray();

            BigInteger out1MinusIn1 = out1 - in1;
            byte[] diffBytes = out1MinusIn1.ToByteArray();

            BigInteger blinding1Int = new BigInteger(blinding1);
            BigInteger blinding2Int = new BigInteger(blinding2);

            BigInteger blindingDiff = blinding2Int - blinding1Int;

            byte[] blindingDiffBytes = blindingDiff.ToByteArray();
            byte[] diff1 = Ed25519.PublicKeyFromSeed(blindingDiffBytes);

            byte[] assetDenom = GetRandomSeed();
        }

        private GroupElementP3 GetAssetCommitment(ulong amount, ulong assetId, out byte[] blindingSeed)
        {
            GroupElementP3 blindingP3;
            GetRandomBlinding(out blindingP3, out blindingSeed);

            GroupElementP3 assetP3 = GetAsset(assetId);

            GroupElementP3 commitmentP3 = GetCommitment(blindingP3, assetP3);

            return commitmentP3;
        }

        private GroupElementP3 GetCommitment(GroupElementP3 blindingP3, GroupElementP3 assetAmountP3)
        {
            GroupElementCached assetAmountChached;
            GroupOperations.ge_p3_to_cached(out assetAmountChached, ref assetAmountP3);

            GroupElementP1P1 commitmentP1P1;
            GroupOperations.ge_add(out commitmentP1P1, ref blindingP3, ref assetAmountChached);

            GroupElementP3 commitmentP3;
            GroupOperations.ge_p1p1_to_p3(out commitmentP3, ref commitmentP1P1);

            return commitmentP3;
        }

        private static GroupElementP3 GetAsset(ulong assetId)
        {
            byte[] assetIdBytes = BitConverter.GetBytes(assetId);
            byte[] assetIdSeed = new byte[32];
            Array.Copy(assetIdBytes, 0, assetIdSeed, 0, assetIdBytes.Length);

            GroupElementP3 assetP3 = GetAsset(assetIdSeed);

            return assetP3;
        }

        private static GroupElementP3 GetAsset(byte[] assetIdSeed)
        {
            GroupElementP3 assetP3;
            int res;
            ulong counter = 0;
            do
            {
                byte[] counterBytes = BitConverter.GetBytes(counter++);
                var hasher = new Sha512();
                hasher.Update(assetIdSeed, 0, assetIdSeed.Length);
                hasher.Update(counterBytes, 0, counterBytes.Length);
                byte[] hashBytes = hasher.Finish();
                res = GroupOperations.ge_frombytes_negate_vartime(out assetP3, hashBytes, 0);
            } while (res != 0);
            //GroupOperations.ge_scalarmult_base(out assetP3, assetIdSeed, 0);

            return assetP3;
        }

        private static GroupElementP3 GetBlinding(byte[] blindingSeed)
        {
            GroupElementP3 blindingP3;
            GroupOperations.ge_scalarmult_base(out blindingP3, blindingSeed, 0);

            return blindingP3;
        }

        private static void GetRandomBlinding(out GroupElementP3 blindingP3, out byte[] blindingSeed)
        {
            do
            {
                blindingSeed = GetRandomSeed();
            } while (blindingSeed[31] >= 127);

            blindingP3 = GetBlinding(blindingSeed);
        }

        private static byte[] GetRandomSeed()
        {
            byte[] seed = new byte[32];
            RNGCryptoServiceProvider.Create().GetNonZeroBytes(seed);

            return seed;
        }

        private static BitArray GetAmountBits(ulong a)
        {
            byte[] bytes = BitConverter.GetBytes(a);
            BitArray bitArray = new BitArray(bytes);

            return bitArray;
        }

        #region Confidential Ring Signatures

        //RingCT protocol
        //genRct: 
        //   creates an rctSig with all data necessary to verify the rangeProofs and that the signer owns one of the
        //   columns that are claimed as inputs, and that the sum of inputs  = sum of outputs.
        //   Also contains masked "amount" and "mask" so the receiver can see how much they received
        //verRct:
        //   verifies that all signatures (rangeProogs, MG sig, sum inputs = outputs) are correct
        //decodeRct: (c.f. https://eprint.iacr.org/2015/1098 section 5.1.1)
        //   uses the attached ecdh info to find the amounts represented by each output commitment 
        //   must know the destination private key to find the correct amount, else will return a random number
        //   Note: For txn fees, the last index in the amounts vector should contain that
        //   Thus the amounts vector will be "one" longer than the destinations vectort
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">????</param>
        /// <param name="inSk">collection of pairs of secrect keys and blinding factors of all inputs used for transaction</param>
        /// <param name="destinations">public keys of destinations</param>
        /// <param name="amounts">amounts per destination</param>
        /// <param name="mixRing">collection of collections of pairs of public key and Pedersen Commitment used for inputs where index of real collection of pairs designated by argument "index"</param>
        /// <param name="amount_keys">shared secret of corresponding destinations, i.e. multiplication of private key of sender with a public key of a receiver</param>
        /// <param name="index">index of real input collection of pairs of Public Key and Pedersen Commitment of inputs</param>
        /// <param name="outSk">collection of pairs of secrect keys and blinding factors of all outputs used for transaction</param>
        /// <returns></returns>
        private static RctSig GenerateRct(Key message, CtKeyList inSk, KeysList destinations, List<ulong> amounts, CtKeyMatrix mixRing, KeysList amount_keys, int index, ref CtKeyList outSk)
        {
            if (amounts.Count != destinations.Count || amounts.Count != destinations.Count + 1)
            {
                throw new ArgumentException(nameof(amounts), $"Different number of {nameof(amounts)}/{nameof(destinations)}");
            }

            if (amount_keys.Count != destinations.Count)
            {
                throw new ArgumentException(nameof(amount_keys), $"Different number of {nameof(amount_keys)}/{nameof(destinations)}");
            }

            if (index >= mixRing.Count)
            {
                throw new ArgumentException(nameof(index), $"Bad {nameof(index)} into {nameof(mixRing)}");
            }

            for (int n = 0; n < mixRing.Count; ++n)
            {
                if (mixRing[n].Count != inSk.Count)
                {
                    throw new ArgumentException(nameof(mixRing), $"Bad {nameof(mixRing)} size");
                }
            }

            KeysList masks = new KeysList(); //sk mask..
            RctSig rv = new RctSig
            {
                Message = message
            };

            for (int k = 0; k < destinations.Count; k++)
            {
                rv.OutPk.Add(new CtKey());
                rv.P.RangeSigs.Add(new RangeSig());
                rv.EcdhInfo.Add(new EcdhTuple());
                masks.Add(new Key());
                outSk.Add(new CtKey());
            }

            int i = 0;
            for (i = 0; i < destinations.Count; i++)
            {
                //add destination to sig
                Array.Copy(destinations[i].Bytes, 0, rv.OutPk[i].Dest.Bytes, 0, destinations[i].Bytes.Length);
                //compute range proof
                rv.P.RangeSigs[i] = GetRangeSig(rv.OutPk[i].Mask, outSk[i].Mask, amounts[i]);
                //mask amount and mask
                Array.Copy(outSk[i].Mask.Bytes, 0, rv.EcdhInfo[i].Mask, 0, outSk[i].Mask.Bytes.Length);

                rv.EcdhInfo[i].Amount = BitConverter.GetBytes(amounts[i]);
                EcdhEncode(rv.EcdhInfo[i], amount_keys[i]);
            }

            rv.MixRing = mixRing;
            rv.P.MGs.Add(ProveRctMG(get_pre_mlsag_hash(rv), rv.MixRing, inSk, outSk, rv.OutPk, index));
            return rv;
        }

        //RingCT protocol
        //genRct: 
        //   creates an rctSig with all data necessary to verify the rangeProofs and that the signer owns one of the
        //   columns that are claimed as inputs, and that the sum of inputs  = sum of outputs.
        //   Also contains masked "amount" and "mask" so the receiver can see how much they received
        //verRct:
        //   verifies that all signatures (rangeProogs, MG sig, sum inputs = outputs) are correct
        //decodeRct: (c.f. https://eprint.iacr.org/2015/1098 section 5.1.1)
        //   uses the attached ecdh info to find the amounts represented by each output commitment 
        //   must know the destination private key to find the correct amount, else will return a random number    
        private static ulong DecodeRct(RctSig rv, Key sk, int i, ref Key mask)
        {
            if (i >= rv.EcdhInfo.Count)
            {
                throw new ArgumentException(nameof(i), "Bad index");
            }

            if (rv.OutPk.Count != rv.EcdhInfo.Count)
            {
                throw new ArgumentException(nameof(rv), $"Mismatched sizes of {nameof(rv.OutPk)} and {nameof(rv.EcdhInfo)}");
            }

            //mask amount and mask
            EcdhTuple ecdh_info = rv.EcdhInfo[i];
            EcdhDecode(ecdh_info, sk);
            mask.Bytes = ecdh_info.Mask;
            Key amount = new Key
            {
                Bytes = ecdh_info.Amount
            };

            Key C = rv.OutPk[i].Mask;

            Key Ctmp = new Key();
            if (ScalarOperations.sc_check(mask.Bytes) != 0)
            {
                throw new Exception("warning, bad ECDH mask");
            }

            if (ScalarOperations.sc_check(amount.Bytes) != 0)
            {
                throw new Exception("warning, bad ECDH amount");
            }

            ScalarmulBaseAddKeys2(Ctmp, mask, amount, H);
            if (!EqualKeys(C, Ctmp))
            {
                throw new Exception("warning, amount decoded incorrectly, will be unable to spend");
            }

            ulong res = BitConverter.ToUInt64(amount.Bytes, 0);

            return res;
        }

        private static ulong DecodeRct(RctSig rv, Key sk, int i)
        {
            Key mask = new Key();
            return DecodeRct(rv, sk, i, ref mask);
        }

        //RingCT protocol
        //genRct: 
        //   creates an rctSig with all data necessary to verify the rangeProofs and that the signer owns one of the
        //   columns that are claimed as inputs, and that the sum of inputs  = sum of outputs.
        //   Also contains masked "amount" and "mask" so the receiver can see how much they received
        //verRct:
        //   verifies that all signatures (rangeProogs, MG sig, sum inputs = outputs) are correct
        //decodeRct: (c.f. https://eprint.iacr.org/2015/1098 section 5.1.1)
        //   uses the attached ecdh info to find the amounts represented by each output commitment 
        //   must know the destination private key to find the correct amount, else will return a random number    
        private static bool VerRct(RctSig rv, bool semantics)
        {
            if (semantics)
            {
                if (rv.OutPk.Count != rv.P.RangeSigs.Count)
                {
                    throw new ArgumentException(nameof(rv), $"Mismatched sizes of {nameof(rv.OutPk)} and {rv.P.RangeSigs}");
                }

                if (rv.OutPk.Count == rv.EcdhInfo.Count)
                {
                    throw new ArgumentException(nameof(rv), $"Mismatched sizes of {nameof(rv.OutPk)} and {nameof(rv.EcdhInfo)}");
                }

                if (rv.P.MGs.Count != 1)
                {
                    throw new ArgumentException(nameof(rv), $"full rctSig has not one MG");
                }
            }

            // some rct ops can throw
            try
            {
                if (semantics)
                {
                    bool[] results = new bool[rv.OutPk.Count];

                    for (int i = 0; i < rv.OutPk.Count; i++)
                    {
                        results[i] = VerifyRangeSig(rv.OutPk[i].Mask, rv.P.RangeSigs[i]);
                    }

                    for (int i = 0; i < results.Length; ++i)
                    {
                        if (!results[i])
                        {
                            //LOG_PRINT_L1("Range proof verified failed for proof " << i);
                            return false;
                        }
                    }
                }

                if (!semantics)
                {
                    bool mgVerd = VerRctMG(rv.P.MGs[0], rv.MixRing, rv.OutPk, get_pre_mlsag_hash(rv));
                    if (!mgVerd)
                    {
                        //LOG_PRINT_L1("MG signature verification failed");
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                //LOG_PRINT_L1("Error in verRct: " << e.what());
                return false;
            }
        }

        //GetRangeSig and verRange
        //GetRangeSig gives C, and mask such that \sumCi = C
        //   c.f. https://eprint.iacr.org/2015/1098 section 5.1
        //   and Ci is a commitment to either 0 or 2^i, i=0,...,63
        //   thus this proves that "amount" is in [0, 2^64]
        //   mask is a such that C = aG + bH, and b = amount
        //verRange verifies that \sum Ci = C and that each Ci is a commitment to 0 or 2^i
        /// <summary>
        /// gives C, and mask such that \sumCi = C
        ///  c.f. https://eprint.iacr.org/2015/1098 section 5.1
        ///   and Ci is a commitment to either 0 or 2^i, i=0,...,63
        ///   thus this proves that "amount" is in [0, 2^64]
        ///   mask is a such that C = aG + bH, and b = amount
        /// </summary>
        /// <param name="c">An output value that will hold Pedersen Commitment associated with the certain amount</param>
        /// <param name="mask">will hold the blinding factor value used in the calculation of this Pedersen Commitment</param>
        /// <param name="amount">is the output amount for which the Pedersen Commitment will be calculated</param>
        /// <returns></returns>
        private static RangeSig GetRangeSig(Key commitment, Key blindingFactor, ulong amount)
        {
            if (commitment == null)
            {
                throw new ArgumentNullException(nameof(commitment));
            }

            if (blindingFactor == null)
            {
                throw new ArgumentNullException(nameof(blindingFactor));
            }

            Array.Clear(blindingFactor.Bytes, 0, blindingFactor.Bytes.Length);
            Array.Copy(I.Bytes, 0, commitment.Bytes, 0, I.Bytes.Length);
            BitArray bits = GetAmountBits(amount);
            RangeSig sig = new RangeSig();
            Key64 blindingFactorsPerBit = new Key64(); // random secret keys
            Key64 CiH = new Key64();
            for (int i = 0; i < bits.Length; i++)
            {
                // creating random blinding factor
                blindingFactorsPerBit.Keys[i].Bytes = GetRandomSeed();
                if (bits[i])
                {
                    ScalarmulBaseAddKeys(sig.Ci.Keys[i], blindingFactorsPerBit.Keys[i], H2.Keys[i]);
                }
                else
                {
                    ScalarmultBase(sig.Ci.Keys[i], blindingFactorsPerBit.Keys[i]);
                }

                SubKeys(CiH.Keys[i], sig.Ci.Keys[i], H2.Keys[i]);
                ScalarOperations.sc_add(blindingFactor.Bytes, blindingFactor.Bytes, blindingFactorsPerBit.Keys[i].Bytes);
                AddKeys(commitment, commitment, sig.Ci.Keys[i]);
            }
            sig.Asig = GenBorromean(blindingFactorsPerBit, sig.Ci, CiH, bits);
            return sig;
        }

        //proveRange and verRange
        //proveRange gives C, and mask such that \sumCi = C
        //   c.f. https://eprint.iacr.org/2015/1098 section 5.1
        //   and Ci is a commitment to either 0 or 2^i, i=0,...,63
        //   thus this proves that "amount" is in [0, 2^64]
        //   mask is a such that C = aG + bH, and b = amount
        //verRange verifies that \sum Ci = C and that each Ci is a commitment to 0 or 2^i
        private static bool VerifyRangeSig(Key C, RangeSig rangeSig)
        {
            try
            {
                GroupElementP3[] CiH = new GroupElementP3[64], asCi = new GroupElementP3[64];
                int i = 0;
                GroupElementP3 Ctmp_p3 = LookupTables.ge_p3_identity;
                for (i = 0; i < 64; i++)
                {
                    // faster equivalent of:
                    // subKeys(CiH[i], as.Ci[i], H2[i]);
                    // addKeys(Ctmp, Ctmp, as.Ci[i]);
                    GroupElementCached cached;
                    GroupElementP3 p3;
                    GroupElementP1P1 p1;
                    if (GroupOperations.ge_frombytes_negate_vartime(out p3, H2.Keys[i].Bytes, 0) != 0)
                    {
                        throw new Exception("point conv failed");
                    }
                    GroupOperations.ge_p3_to_cached(out cached, ref p3);
                    if (GroupOperations.ge_frombytes_negate_vartime(out asCi[i], rangeSig.Ci.Keys[i].Bytes, 0) != 0)
                    {
                        throw new Exception("point conv failed");
                    }

                    GroupOperations.ge_sub(out p1, ref asCi[i], ref cached);
                    GroupOperations.ge_p3_to_cached(out cached, ref asCi[i]);
                    GroupOperations.ge_p1p1_to_p3(out CiH[i], ref p1);
                    GroupOperations.ge_add(out p1, ref Ctmp_p3, ref cached);
                    GroupOperations.ge_p1p1_to_p3(out Ctmp_p3, ref p1);
                }
                Key Ctmp = new Key();
                GroupOperations.ge_p3_tobytes(Ctmp.Bytes, 0, ref Ctmp_p3);
                if (!EqualKeys(C, Ctmp))
                    return false;
                if (!VerifyBorromean(rangeSig.Asig, asCi, CiH))
                    return false;
                return true;
            }
            // we can get deep throws from ge_frombytes_vartime if input isn't valid
            catch
            {
                return false;
            }
        }

        //Ring-ct MG sigs
        //Prove: 
        //   c.f. https://eprint.iacr.org/2015/1098 section 4. definition 10. 
        //   This does the MG sig on the "dest" part of the given key matrix, and 
        //   the last row is the sum of input commitments from that column - sum output commitments
        //   this shows that sum inputs = sum outputs
        //Ver:    
        //   verifies the above sig is created correctly
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="pubs">collection of collections of pairs of public key and Pedersen Commitment used for inputs where index of real collection of pairs designated by argument "index"</param>
        /// <param name="inSk">collection of pairs of secrect keys and blinding factors of all inputs used for transaction</param>
        /// <param name="outSk">collection of pairs of secrect keys and blinding factors of all outputs used for transaction</param>
        /// <param name="outPk">collection of pairs of public keys of receiver and pedersen commitments of amount sent to him</param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static MgSig ProveRctMG(Key message, CtKeyMatrix pubs, CtKeyList inSk, CtKeyList outSk, CtKeyList outPk, int index)
        {
            //setup vars
            int cols = pubs.Count;
            if (cols == 0)
            {
                throw new ArgumentException(nameof(pubs), $"Empty {nameof(pubs)}");
            }

            int rows = pubs[0].Count;
            if (rows == 0)
            {
                throw new ArgumentException(nameof(pubs), $"Empty {nameof(pubs)}");
            }

            for (int k = 1; k < cols; k++)
            {
                if (pubs[k].Count != rows)
                {
                    throw new ArgumentException(nameof(pubs), $"{nameof(pubs)} is not rectangular");
                }
            }

            if (inSk.Count != rows)
            {
                throw new ArgumentException(nameof(inSk), $"Bad {nameof(inSk)} size");
            }

            if (outSk.Count != outPk.Count)
            {
                throw new ArgumentException(nameof(outSk), $"Bad {nameof(outSk)}/{nameof(outPk)} size");
            }

            KeysList sk = new KeysList();
            for (int k = 0; k < rows + 1; k++)
            {
                sk.Add(new Key());
            }

            KeysList tmp = new KeysList();
            for (int k = 0; k < rows + 1; k++)
            {
                Key key = new Key();
                Array.Copy(I.Bytes, 0, key.Bytes, 0, I.Bytes.Length);
                tmp.Add(key);
            }

            KeysMatrix M = new KeysMatrix();
            //create the matrix to mg sig
            for (int i = 0; i < cols; i++)
            {
                KeysList keys = new KeysList();
                for (int j = 0; j < rows + 1; j++)
                {
                    Key key = new Key();
                    keys.Add(key);

                    if (j == rows)
                    {
                        Array.Copy(I.Bytes, 0, key.Bytes, 0, I.Bytes.Length);
                    }
                }
                for (int j = 0; j < rows; j++)
                {
                    M[i][j] = pubs[i][j].Dest;
                    ScalarmulBaseAddKeys(M[i][rows], M[i][rows], pubs[i][j].Mask); //add input commitments in last row
                }
            }

            Array.Clear(sk[rows].Bytes, 0, sk[rows].Bytes.Length);
            for (int j = 0; j < rows; j++)
            {
                Array.Copy(inSk[j].Dest.Bytes, 0, sk[j].Bytes, 0, inSk[j].Dest.Bytes.Length);
                ScalarOperations.sc_add(sk[rows].Bytes, sk[rows].Bytes, inSk[j].Mask.Bytes); //add blinding factor in last row
            }

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < outPk.Count; j++)
                {
                    SubKeys(M[i][rows], M[i][rows], outPk[j].Mask); //subtract output Ci's in last row
                }

                ////subtract txn fee output in last row
                ////SubKeys(M[i][rows], M[i][rows], txnFeeKey);
            }

            for (int j = 0; j < outPk.Count; j++)
            {
                ScalarOperations.sc_sub(sk[rows].Bytes, sk[rows].Bytes, outSk[j].Mask.Bytes); //subtract output masks in last row..
            }

            MgSig result = MLSAG_Gen(message, M, sk, index, rows);

            for (int i = 0; i < sk.Count; i++)
            {
                Array.Clear(sk[i].Bytes, 0, sk[i].Bytes.Length);
            }

            return result;
        }

        //Ring-ct MG sigs
        //Prove: 
        //   c.f. https://eprint.iacr.org/2015/1098 section 4. definition 10. 
        //   This does the MG sig on the "dest" part of the given key matrix, and 
        //   the last row is the sum of input commitments from that column - sum output commitments
        //   this shows that sum inputs = sum outputs
        //Ver:    
        //   verifies the above sig is created corretly
        private static bool VerRctMG(MgSig mg, CtKeyMatrix pubs, CtKeyList outPk, Key message)
        {
            //setup vars
            int cols = pubs.Count;
            if (cols == 0)
            {
                throw new ArgumentException(nameof(pubs), $"Empty {nameof(pubs)}");
            }
            int rows = pubs[0].Count;
            if (rows == 0)
            {
                throw new ArgumentException(nameof(pubs), $"Empty {nameof(pubs)}");
            }
            for (int k = 1; k < cols; k++)
            {
                if (pubs[k].Count != rows)
                {
                    throw new ArgumentException(nameof(pubs), $"{nameof(pubs)} is not rectangular");
                }
            }

            KeysList tmp = new KeysList();
            for (int k = 0; k < rows + 1; k++)
            {
                Key key = new Key();
                Array.Copy(I.Bytes, 0, key.Bytes, 0, I.Bytes.Length);
                tmp.Add(key);
            }


            KeysMatrix M = new KeysMatrix();
            //create the matrix to mg sig
            for (int j = 0; j < rows; j++)
            {
                M.Add(new KeysList());
                for (int i = 0; i < cols; i++)
                {
                    M[i].Add(new Key());
                    M[i][j] = pubs[i][j].Dest;
                    ScalarmulBaseAddKeys(M[i][rows], M[i][rows], pubs[i][j].Mask); //add Ci in last row
                }
            }
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < outPk.Count; j++)
                {
                    SubKeys(M[i][rows], M[i][rows], outPk[j].Mask); //subtract output Ci's in last row
                }
            }

            return MLSAG_Ver(message, M, mg, rows);
        }

        #endregion

        #region Borromean

        //Borromean (c.f. gmax/andytoshi's paper)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="blindingFactorsPerBit">Blinding factors used for creating commitment per bit</param>
        /// <param name="commitmentsPerBit">Commitment per each bit</param>
        /// <param name="P2"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        private static BoroSig GenBorromean(Key64 blindingFactorsPerBit, Key64 commitmentsPerBit, Key64 P2, BitArray indices)
        {
            Key64[] L = new Key64[] { new Key64(), new Key64() };
            Key64 alpha = new Key64();
            Key c = new Key();
            int naught = 0, prime = 0, ii = 0, jj = 0;
            BoroSig bb = new BoroSig();
            for (ii = 0; ii < 64; ii++)
            {
                naught = indices[ii] ? 1 : 0;
                prime = ((indices[ii] ? 1 : 0) + 1) % 2;
                alpha.Keys[ii].Bytes = GetRandomSeed();
                ScalarmultBase(L[naught].Keys[ii], alpha.Keys[ii]);
                if (naught == 0)
                {

                    bb.S1.Keys[ii].Bytes = GetRandomSeed();
                    c.Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(L[naught].Keys[ii].Bytes).GetBytes();
                    ScalarmulBaseAddKeys2(L[prime].Keys[ii], bb.S1.Keys[ii], c, P2.Keys[ii]);
                }
            }
            byte[] buf = new byte[64 * 32];
            for (int i = 0; i < 64; i++)
            {
                Array.Copy(L[1].Keys[i].Bytes, 0, buf, 32 * i, L[1].Keys[i].Bytes.Length);
            }
            bb.Ee.Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(buf).GetBytes(); //or L[1]..
            Key LL = new Key(), cc = new Key();
            for (jj = 0; jj < 64; jj++)
            {
                if (!indices[jj])
                {
                    ScalarOperations.sc_mulsub(bb.S0.Keys[jj].Bytes, blindingFactorsPerBit.Keys[jj].Bytes, bb.Ee.Bytes, alpha.Keys[jj].Bytes);
                }
                else
                {
                    bb.S0.Keys[jj].Bytes = GetRandomSeed();
                    ScalarmulBaseAddKeys2(LL, bb.S0.Keys[jj], bb.Ee, commitmentsPerBit.Keys[jj]); //different L0
                    cc.Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(LL.Bytes).GetBytes();
                    ScalarOperations.sc_mulsub(bb.S1.Keys[jj].Bytes, blindingFactorsPerBit.Keys[jj].Bytes, cc.Bytes, alpha.Keys[jj].Bytes);
                }
            }
            return bb;
        }

        //see above.
        private static bool VerifyBorromean(BoroSig bb, GroupElementP3[] P1, GroupElementP3[] P2)
        {
            Key64 Lv1 = new Key64();
            Key chash = new Key(), LL = new Key();
            int ii = 0;
            GroupElementP2 p2;
            for (ii = 0; ii < 64; ii++)
            {
                // equivalent of: addKeys2(LL, bb.s0[ii], bb.ee, P1[ii]);
                GroupOperations.ge_double_scalarmult_vartime(out p2, bb.Ee.Bytes, ref P1[ii], bb.S0.Keys[ii].Bytes);
                GroupOperations.ge_tobytes(LL.Bytes, 0, ref p2);
                chash.Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(LL.Bytes).GetBytes();
                // equivalent of: addKeys2(Lv1[ii], bb.s1[ii], chash, P2[ii]);
                GroupOperations.ge_double_scalarmult_vartime(out p2, chash.Bytes, ref P2[ii], bb.S1.Keys[ii].Bytes);
                GroupOperations.ge_tobytes(Lv1.Keys[ii].Bytes, 0, ref p2);
            }
            byte[] buf = new byte[64 * 32];
            for (int i = 0; i < 64; i++)
            {
                Array.Copy(Lv1.Keys[i].Bytes, 0, buf, 32 * i, Lv1.Keys[i].Bytes.Length);
            }
            Key eeComputed = new Key
            {
                Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(buf).GetBytes() //hash function fine
            };
            return EqualKeys(eeComputed, bb.Ee);
        }

        private bool VerifyBorromean(BoroSig bb, Key64 P1, Key64 P2)
        {
            GroupElementP3[] P1_p3 = new GroupElementP3[64], P2_p3 = new GroupElementP3[64];
            for (int i = 0; i < 64; ++i)
            {
                if (GroupOperations.ge_frombytes_negate_vartime(out P1_p3[i], P1.Keys[i].Bytes, 0) != 0)
                {
                    throw new ArgumentException();
                }

                if (GroupOperations.ge_frombytes_negate_vartime(out P2_p3[i], P2.Keys[i].Bytes, 0) != 0)
                {
                    throw new ArgumentException();
                }
            }
            return VerifyBorromean(bb, P1_p3, P2_p3);
        }

        #endregion Borromean

        #region MLSAG

        //Multilayered Spontaneous Anonymous Group Signatures (MLSAG signatures)
        //This is a just slightly more efficient version than the ones described below
        //(will be explained in more detail in Ring Multisig paper
        //These are aka MG signatures in earlier drafts of the ring ct paper
        // c.f. https://eprint.iacr.org/2015/1098 section 2. 
        // Gen creates a signature which proves that for some column in the keymatrix "pk"
        //   the signer knows a secret key for each row in that column
        // Ver verifies that the MG sig was created correctly        
        private static MgSig MLSAG_Gen(Key message, KeysMatrix pk, KeysList sk, int index, int dsRows)
        {
            MgSig mgSig = new MgSig();
            int cols = pk.Count;
            if (cols < 2)
            {
                throw new ArgumentException("Error! What is c if count of pk = 1!");
            }
            if (index >= cols)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            int rows = pk[0].Count;
            if (rows == 0)
            {
                throw new ArgumentException(nameof(pk), "Empty pk");
            }
            for (int i1 = 1; i1 < cols; i1++)
            {
                if (pk[i1].Count != rows)
                {
                    throw new ArgumentException(nameof(pk), "PK matrix is not rectangular");
                }
            }

            if (sk.Count != rows)
            {
                throw new ArgumentException(nameof(sk), $"Bad {nameof(sk)} size");
            }

            if (dsRows > rows)
            {
                throw new ArgumentException(nameof(dsRows), $"Bad {nameof(dsRows)} size");
            }

            int i = 0, j = 0, ii = 0;
            Key c = new Key(), c_old = new Key(), L = new Key(), R = new Key();
            List<GroupElementCached[]> Ip = new List<GroupElementCached[]>(dsRows);
            for (int k = 0; k < dsRows; k++)
            {
                Ip.Add(new GroupElementCached[8]);
            }

            mgSig.II = new KeysList();
            for (int k = 0; k < dsRows; k++)
            {
                mgSig.II.Add(new Key());
            }

            KeysList alpha = new KeysList();
            for (int k = 0; k < rows; k++)
            {
                alpha.Add(new Key());
            }

            KeysList aG = new KeysList();
            for (int k = 0; k < rows; k++)
            {
                aG.Add(new Key());
            }

            mgSig.SS = new KeysMatrix();
            for (int k = 0; k < cols; k++)
            {
                KeysList keys = new KeysList();
                for (int m = 0; m < rows; m++)
                {
                    keys.Add(new Key());
                }
                mgSig.SS.Add(keys);
            }

            KeysList aHP = new KeysList();
            for (int k = 0; k < dsRows; k++)
            {
                aHP.Add(new Key());
            }

            KeysList toHash = new KeysList();
            for (int k = 0; k < 1 + 3 * dsRows + 2 * (rows - dsRows); k++)
            {
                toHash.Add(new Key());
            }

            toHash[0] = message;

            for (i = 0; i < dsRows; i++)
            {
                toHash[3 * i + 1] = pk[index][i];
                Key Hi = HashToPoint(pk[index][i]);
                Mlsag_Prepare(Hi, sk[i], out Key alphaI, out Key aGi, aHP[i], mgSig.II[i]);
                alpha[i] = alphaI; // alphaI - generated secret key
                aG[i] = aGi; // aGi - generated public key from alphaI
                toHash[3 * i + 2] = aG[i];
                toHash[3 * i + 3] = aHP[i];
                Precomp(Ip[i], mgSig.II[i]);
            }
            int ndsRows = 3 * dsRows; //non Double Spendable Rows (see identity chains paper)
            for (i = dsRows, ii = 0; i < rows; i++, ii++)
            {
                SkpkGen(out Key alphai, out Key aGi); //need to save alphas for later..
                alpha[i] = alphai; aG[i] = aGi;
                toHash[ndsRows + 2 * ii + 1] = pk[index][i];
                toHash[ndsRows + 2 * ii + 2] = aG[i];
            }

            Mlsag_Hash(toHash, out c_old);


            i = (index + 1) % cols;
            if (i == 0)
            {
                Array.Copy(c_old.Bytes, 0, mgSig.CC.Bytes, 0, c_old.Bytes.Length);
            }

            while (i != index)
            {
                for (int k = 0; k < rows; k++)
                {
                    mgSig.SS[i].Add(new Key { Bytes = GetRandomSeed() });
                }
                Array.Clear(c.Bytes, 0, 32);
                for (j = 0; j < dsRows; j++)
                {
                    ScalarmulBaseAddKeys2(L, mgSig.SS[i][j], c_old, pk[i][j]);
                    Key Hi = HashToPoint(pk[i][j]);
                    ScalarmulBaseAddKeys3(R, mgSig.SS[i][j], Hi, c_old, Ip[j]);
                    toHash[3 * j + 1] = pk[i][j];
                    toHash[3 * j + 2] = L;
                    toHash[3 * j + 3] = R;
                }
                for (j = dsRows, ii = 0; j < rows; j++, ii++)
                {
                    ScalarmulBaseAddKeys2(L, mgSig.SS[i][j], c_old, pk[i][j]);
                    toHash[ndsRows + 2 * ii + 1] = pk[i][j];
                    toHash[ndsRows + 2 * ii + 2] = L;
                }
                Mlsag_Hash(toHash, out c);
                Array.Copy(c.Bytes, 0, c_old.Bytes, 0, c.Bytes.Length);
                i = (i + 1) % cols;

                if (i == 0)
                {
                    Array.Copy(c_old.Bytes, 0, mgSig.CC.Bytes, 0, c_old.Bytes.Length);
                }
            }
            Mlsag_Sign(c, sk, alpha, rows, dsRows, mgSig.SS[index]);
            return mgSig;
        }

        //Multilayered Spontaneous Anonymous Group Signatures (MLSAG signatures)
        //This is a just slghtly more efficient version than the ones described below
        //(will be explained in more detail in Ring Multisig paper
        //These are aka MG signatutes in earlier drafts of the ring ct paper
        // c.f. https://eprint.iacr.org/2015/1098 section 2. 
        // Gen creates a signature which proves that for some column in the keymatrix "pk"
        //   the signer knows a secret key for each row in that column
        // Ver verifies that the MG sig was created correctly            
        private static bool MLSAG_Ver(Key message, KeysMatrix pk, MgSig rv, int dsRows)
        {

            int cols = pk.Count;
            if (cols <= 1)
            {
                throw new ArgumentException(nameof(pk), $"Error! What is c if {nameof(cols)} = 1!");
            }

            int rows = pk[0].Count;
            if (rows == 0)
            {
                throw new ArgumentException(nameof(pk), $"Empty {nameof(pk)}");
            }

            for (int k = 1; k < cols; ++k)
            {
                if (pk[k].Count != rows)
                {
                    throw new ArgumentException(nameof(pk), $"{nameof(pk)} is not rectangular");
                }
            }

            if (rv.II.Count != dsRows)
            {
                throw new ArgumentException(nameof(rv), $"Bad {rv.II} size");
            }

            if (rv.SS.Count != cols)
            {
                throw new ArgumentException(nameof(rv), $"Bad {rv.SS} size");
            }

            for (int k = 0; k < cols; ++k)
            {
                if (rv.SS[k].Count != rows)
                {
                    throw new ArgumentException(nameof(rv), $"{rv.SS} is not rectangular");
                }
            }
            if (dsRows > rows)
            {
                throw new ArgumentException(nameof(dsRows), $"Bad {nameof(dsRows)} value");
            }

            for (int i = 0; i < rv.SS.Count; ++i)
            {
                for (int j = 0; j < rv.SS[i].Count; ++j)
                {
                    if (ScalarOperations.sc_check(rv.SS[i][j].Bytes) != 0)
                    {
                        throw new ArgumentException(nameof(rv.SS), $"Bad {rv.SS} slot");
                    }
                }
            }

            if (ScalarOperations.sc_check(rv.CC.Bytes) != 0)
            {
                throw new ArgumentException(nameof(rv.CC), $"Bad {nameof(rv.CC)}");
            }

            Key c = new Key(), L = new Key(), R = new Key(), Hi = new Key();
            Key c_old = new Key();
            Array.Copy(rv.CC.Bytes, 0, c_old.Bytes, 0, rv.CC.Bytes.Length);
            List<GroupElementCached[]> Ip = new List<GroupElementCached[]>();
            for (int i = 0; i < dsRows; i++)
            {
                Ip.Add(new GroupElementCached[8]);
                for (int j = 0; j < 8; j++)
                {
                    Ip[i][j] = new GroupElementCached();
                }
            }
            for (int i = 0; i < dsRows; i++)
            {
                Precomp(Ip[i], rv.II[i]);
            }
            int ndsRows = 3 * dsRows; //non Double Spendable Rows (see identity chains paper
            int toHashSize = 1 + 3 * dsRows + 2 * (rows - dsRows);
            KeysList toHash = new KeysList();
            for (int k = 0; k < toHashSize; k++)
            {
                toHash.Add(new Key());
            }

            toHash[0] = message;
            int i1 = 0;
            while (i1 < cols)
            {
                Array.Clear(c.Bytes, 0, c.Bytes.Length);
                for (int j = 0; j < dsRows; j++)
                {
                    ScalarmulBaseAddKeys2(L, rv.SS[i1][j], c_old, pk[i1][j]);
                    Hi = HashToPoint(pk[i1][j]);
                    if (Hi.Bytes.Equals32(I.Bytes))
                    {
                        throw new Exception("Data hashed to point at infinity");
                    }
                    ScalarmulBaseAddKeys3(R, rv.SS[i1][j], Hi, c_old, Ip[j]);
                    toHash[3 * j + 1] = pk[i1][j];
                    toHash[3 * j + 2] = L;
                    toHash[3 * j + 3] = R;
                }
                for (int j = dsRows, ii = 0; j < rows; j++, ii++)
                {
                    ScalarmulBaseAddKeys2(L, rv.SS[i1][j], c_old, pk[i1][j]);
                    toHash[ndsRows + 2 * ii + 1] = pk[i1][j];
                    toHash[ndsRows + 2 * ii + 2] = L;
                }
                c = FastHash(toHash);
                Array.Copy(c.Bytes, 0, c_old.Bytes, 0, c.Bytes.Length);
                i1++;
            }
            ScalarOperations.sc_sub(c.Bytes, c_old.Bytes, rv.CC.Bytes);
            return ScalarOperations.sc_isnonzero(c.Bytes) == 0;
        }

        private static bool Mlsag_Prepare(Key H, Key xx, out Key a, out Key aG, Key aHP, Key II)
        {
            SkpkGen(out a, out aG);
            ScalarmultKey(aHP, H, a);
            ScalarmultKey(II, H, xx);
            return true;
        }

        private static bool Mlsag_Prepare(out Key a, out Key aG)
        {
            SkpkGen(out a, out aG);
            return true;
        }

        private static bool Mlsag_Sign(Key c, KeysList xx, KeysList alpha, int rows, int dsRows, KeysList ss)
        {
            if (c == null)
            {
                throw new ArgumentNullException(nameof(c));
            }

            if (xx == null)
            {
                throw new ArgumentNullException(nameof(xx));
            }

            if (alpha == null)
            {
                throw new ArgumentNullException(nameof(alpha));
            }

            if (dsRows > rows)
            {
                throw new ArgumentException(nameof(dsRows), $"{nameof(dsRows)} greater than {nameof(rows)}");
            }

            if (xx.Count != rows)
            {
                throw new ArgumentException(nameof(xx), $"{nameof(xx)} size does not match {nameof(rows)}");
            }

            if (alpha.Count != rows)
            {
                throw new ArgumentException(nameof(alpha), $"{nameof(alpha)} size does not match {nameof(rows)}");
            }

            if (ss.Count != rows)
            {
                throw new ArgumentException(nameof(ss), $"{nameof(ss)} size does not match {nameof(rows)}");
            }

            for (int j = 0; j < rows; j++)
            {
                ScalarOperations.sc_mulsub(ss[j].Bytes, c.Bytes, xx[j].Bytes, alpha[j].Bytes);
            }

            return true;
        }

        private static bool Mlsag_Hash(KeysList toHash, out Key hash)
        {
            byte[] buf = new byte[toHash.Count * 32];
            for (int i = 0; i < toHash.Count; i++)
            {
                Array.Copy(toHash[i].Bytes, 0, buf, i * 32, 32);
            }
            hash = new Key
            {
                Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(buf).GetBytes()
            };
            return true;
        }

        #endregion MLSAG

        #region Curve Additions / Subtractions / Multiplications

        //addKeys1
        //aGB = aG + B where a is a scalar, G is the basepoint, and B is a point
        private static void ScalarmulBaseAddKeys(Key aGB, Key a, Key B)
        {
            if (aGB == null)
            {
                throw new ArgumentNullException(nameof(aGB));
            }

            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (B == null)
            {
                throw new ArgumentNullException(nameof(B));
            }

            Key agKey = new Key();
            GroupOperations.ge_scalarmult_base(out GroupElementP3 agP3, a.Bytes, 0);
            GroupOperations.ge_p3_tobytes(agKey.Bytes, 0, ref agP3);
            AddKeys(aGB, agKey, B);
        }

        //addKeys3
        //aAbB = a*A + b*B where a, b are scalars, A, B are curve points
        //B must be input after applying "precomp"
        private static void ScalarmulBaseAddKeys3(Key aAbB, Key a, Key A, Key b, GroupElementCached[] B)
        {
            if (aAbB == null)
            {
                throw new ArgumentNullException(nameof(aAbB));
            }

            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (A == null)
            {
                throw new ArgumentNullException(nameof(A));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (B == null)
            {
                throw new ArgumentNullException(nameof(B));
            }

            if (GroupOperations.ge_frombytes_negate_vartime(out GroupElementP3 A2, A.Bytes, 0) != 0)
            {
                throw new ArgumentException(nameof(A));
            }

            GroupOperations.ge_double_scalarmult_precomp_vartime(out GroupElementP2 rv, a.Bytes, A2, b.Bytes, B);
            GroupOperations.ge_tobytes(aAbB.Bytes, 0, ref rv);
        }

        //addKeys2
        //aGbB = aG + bB where a, b are scalars, G is the basepoint and B is a point
        private static void ScalarmulBaseAddKeys2(Key aGbB, Key a, Key b, Key bPoint)
        {
            if (aGbB == null)
            {
                throw new ArgumentNullException(nameof(aGbB));
            }

            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (bPoint == null)
            {
                throw new ArgumentNullException(nameof(bPoint));
            }

            GroupElementP2 rv;
            GroupElementP3 bPointP3;
            if (GroupOperations.ge_frombytes_negate_vartime(out bPointP3, bPoint.Bytes, 0) != 0)
            {
                throw new ArgumentException(nameof(bPoint), $"Failed to convert to {nameof(GroupElementP3)}");
            }
            GroupOperations.ge_double_scalarmult_vartime(out rv, a.Bytes, ref bPointP3, b.Bytes);
            GroupOperations.ge_tobytes(aGbB.Bytes, 0, ref rv);
        }

        /// <summary>
        /// for curve points: AB = A + B
        /// </summary>
        /// <param name="ab"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private static void AddKeys(Key ab, Key a, Key b)
        {
            if (ab == null)
            {
                throw new ArgumentNullException(nameof(ab));
            }

            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (GroupOperations.ge_frombytes_negate_vartime(out GroupElementP3 aP3, a.Bytes, 0) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(a), $"Failed to convert to {nameof(GroupElementP3)}");
            }
            if (GroupOperations.ge_frombytes_negate_vartime(out GroupElementP3 bP3, b.Bytes, 0) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(b), $"Failed to convert to {nameof(GroupElementP3)}");
            }

            GroupElementCached bCached;
            GroupOperations.ge_p3_to_cached(out bCached, ref bP3);

            GroupElementP1P1 abP1P1;
            GroupOperations.ge_add(out abP1P1, ref aP3, ref bCached);

            GroupElementP3 abP3;
            GroupOperations.ge_p1p1_to_p3(out abP3, ref abP1P1);

            GroupOperations.ge_p3_tobytes(ab.Bytes, 0, ref abP3);
        }

        //does a * G where a is a scalar and G is the curve basepoint
        private static void ScalarmultBase(Key aG, Key a)
        {
            if (aG == null)
            {
                throw new ArgumentNullException(nameof(aG));
            }

            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            GroupElementP3 point;
            Array.Copy(a.Bytes, 0, aG.Bytes, 0, a.Bytes.Length);
            ScalarOperations.sc_reduce32(aG.Bytes);
            GroupOperations.ge_scalarmult_base(out point, aG.Bytes, 0);
            GroupOperations.ge_p3_tobytes(aG.Bytes, 0, ref point);
        }

        //does a * P where a is a scalar and P is an arbitrary point
        private static void ScalarmultKey(Key aP, Key P, Key a)
        {
            if (aP == null)
            {
                throw new ArgumentNullException(nameof(aP));
            }

            if (P == null)
            {
                throw new ArgumentNullException(nameof(P));
            }

            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            GroupElementP3 A;
            GroupElementP2 R;
            if (GroupOperations.ge_frombytes_negate_vartime(out A, P.Bytes, 0) != 0)
            {
                throw new ArgumentException();
            }
            GroupOperations.ge_scalarmult(out R, a.Bytes, ref A);
            GroupOperations.ge_tobytes(aP.Bytes, 0, ref R);
        }

        /// <summary>
        ///subtract Keys (subtracts curve points)
        ///AB = A - B where A, B are curve points
        /// </summary>
        /// <param name="ab"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private static void SubKeys(Key ab, Key a, Key b)
        {
            if (ab == null)
            {
                throw new ArgumentNullException(nameof(ab));
            }

            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (GroupOperations.ge_frombytes_negate_vartime(out GroupElementP3 aP3, a.Bytes, 0) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(a), $"Failed to convert to {nameof(GroupElementP3)}");
            }
            if (GroupOperations.ge_frombytes_negate_vartime(out GroupElementP3 bP3, b.Bytes, 0) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(b), $"Failed to convert to {nameof(GroupElementP3)}");
            }

            GroupElementCached bCached;
            GroupOperations.ge_p3_to_cached(out bCached, ref bP3);

            GroupElementP1P1 abP1P1;
            GroupOperations.ge_sub(out abP1P1, ref aP3, ref bCached);

            GroupElementP3 abP3;
            GroupOperations.ge_p1p1_to_p3(out abP3, ref abP1P1);

            GroupOperations.ge_p3_tobytes(ab.Bytes, 0, ref abP3);
        }

        //checks if A, B are equal in terms of bytes (may say no if one is a non-reduced scalar)
        //without doing curve operations
        private static bool EqualKeys(Key a, Key b)
        {
            bool rv = true;
            for (int i = 0; i < 32; ++i)
            {
                if (a.Bytes[i] != b.Bytes[i])
                {
                    rv = false;
                }
            }
            return rv;
        }

        #endregion Curve Additions / Subtractions / Multiplications

        //generates a random secret and corresponding public key
        private static void SkpkGen(out Key sk, out Key pk)
        {
            sk = new Key
            {
                Bytes = GetRandomSeed()
            };

            pk = new Key();
            ScalarmultBase(pk, sk);
        }

        private static Key HashToPoint(Key hh)
        {
            Key pointk = new Key();
            GroupElementP2 point;
            GroupElementP1P1 point2;
            GroupElementP3 res;
            Key h = new Key();
            h.Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(hh.Bytes).GetBytes();
            GroupOperations.ge_fromfe_frombytes_vartime(out point, h.Bytes, 0);
            GroupOperations.ge_mul8(out point2, ref point);
            GroupOperations.ge_p1p1_to_p3(out res, ref point2);
            GroupOperations.ge_p3_tobytes(pointk.Bytes, 0, ref res);
            return pointk;
        }


        //Does some precomputation to make addKeys3 more efficient
        // input B a curve point and output a ge_dsmp which has precomputation applied
        private static void Precomp(GroupElementCached[] rv, Key B)
        {
            if (rv == null)
            {
                throw new ArgumentNullException(nameof(rv));
            }

            if (rv.Length != 8)
            {
                throw new ArgumentOutOfRangeException(nameof(rv), "Expected exactly 8 items");
            }

            if (GroupOperations.ge_frombytes_negate_vartime(out GroupElementP3 B2, B.Bytes, 0) != 0)
            {
                throw new ArgumentException(nameof(B));
            }
            GroupOperations.ge_dsm_precomp(rv, ref B2);
        }

        //Elliptic Curve Diffie Helman: encodes and decodes the amount b and mask a
        // where C= aG + bH
        private static void EcdhEncode(EcdhTuple unmasked, Key sharedSec)
        {
            if (sharedSec == null)
            {
                throw new ArgumentNullException(nameof(sharedSec));
            }

            Key sharedSec1 = new Key
            {
                Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(sharedSec.Bytes).GetBytes()
            };

            Key sharedSec2 = new Key
            {
                Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(sharedSec1.Bytes).GetBytes()
            };

            //encode
            ScalarOperations.sc_add(unmasked.Mask, unmasked.Mask, sharedSec1.Bytes);
            ScalarOperations.sc_add(unmasked.Amount, unmasked.Amount, sharedSec2.Bytes);
        }

        private static void EcdhDecode(EcdhTuple masked, Key sharedSec)
        {
            if (sharedSec == null)
            {
                throw new ArgumentNullException(nameof(sharedSec));
            }

            Key sharedSec1 = new Key
            {
                Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(sharedSec.Bytes).GetBytes()
            };

            Key sharedSec2 = new Key
            {
                Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(sharedSec1.Bytes).GetBytes()
            };

            //decode
            ScalarOperations.sc_sub(masked.Mask, masked.Mask, sharedSec1.Bytes);
            ScalarOperations.sc_sub(masked.Amount, masked.Amount, sharedSec2.Bytes);
        }

        private static Key FastHash(KeysList keys)
        {
            byte[] buf = new byte[keys.Count * 32];
            int i = 0;
            foreach (Key key in keys)
            {
                Array.Copy(key.Bytes, 0, buf, 32 * i++, 32);
            }

            Key res = new Key
            {
                Bytes = HashLib.HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(buf).GetBytes()
            };

            return res;
        }

        private static Key get_pre_mlsag_hash(RctSig rv)
        {
            KeysList hashes = new KeysList
            {
                rv.Message
            };
            Key prehash;

            //TODO: Code below serializes content of RctSig and creates hash based on it. Must be implemented in real working environment
            //byte[] h;
            //std::stringstream ss;
            //binary_archive<true> ba(ss);
            //CHECK_AND_ASSERT_THROW_MES(!rv.mixRing.empty(), "Empty mixRing");
            //int inputs = rv.MixRing[0].Count;
            //int outputs = rv.EcdhInfo.Count;
            //CHECK_AND_ASSERT_THROW_MES(const_cast<rctSig&>(rv).serialize_rctsig_base(ba, inputs, outputs),            "Failed to serialize rctSigBase");
            //cryptonote::get_blob_hash(ss.str(), h);
            //hashes.push_back(hash2rct(h));

            KeysList kv = new KeysList();

            foreach (RangeSig r in rv.P.RangeSigs)
            {
                for (int n = 0; n < 64; ++n)
                    kv.Add(r.Asig.S0.Keys[n]);
                for (int n = 0; n < 64; ++n)
                    kv.Add(r.Asig.S1.Keys[n]);
                kv.Add(r.Asig.Ee);
                for (int n = 0; n < 64; ++n)
                    kv.Add(r.Ci.Keys[n]);
            }

            hashes.Add(FastHash(kv));
            prehash = FastHash(hashes);
            return prehash;
        }

        private static GroupElementP3 MultiplyBasePoint(byte[] k)
        {
            GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, k, 0);
            return p3;
        }
    }
}
