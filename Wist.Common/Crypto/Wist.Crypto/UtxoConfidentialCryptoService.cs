using Chaos.NaCl.Internal.Ed25519Ref10;
using HashLib;
using System.Linq;
using System.Security.Cryptography;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Crypto
{
    [RegisterDefaultImplementation(typeof(IUtxoConfidentialCryptoService), Lifetime = LifetimeManagement.Singleton)]
    public class UtxoConfidentialCryptoService : IUtxoConfidentialCryptoService
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public UtxoConfidentialCryptoService(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public IKey PublicViewKey { get; private set; }

        public IKey PublicSpendKey { get; private set; }


        public void GetRandomKeyPair(out byte[] secretKey, out byte[] publicKey)
        {
            secretKey = GetRandomSeed(true);
            publicKey = GetPublicKey(secretKey);
        }

        public void Initialize(byte[] privateViewKey, byte[] privateSpendKey)
        {
            PublicViewKey = _identityKeyProvider.GetKey(GetPublicKey(privateViewKey));
            PublicSpendKey = _identityKeyProvider.GetKey(GetPublicKey(privateSpendKey));
        }

        public RingSignature[] Sign(byte[] msg, byte[] keyImage, IKey[] publicKeys, byte[] secretKey, int index)
        {
            RingSignature[] signatures = new RingSignature[publicKeys.Length];

            byte[][] pubs = publicKeys.Select(pk => pk.Value.ToArray()).ToArray();

            GroupOperations.ge_frombytes(out GroupElementP3 keyImageP3, keyImage, 0);
            GroupElementCached[] image_pre = new GroupElementCached[8];
            GroupOperations.ge_dsm_precomp(image_pre, ref keyImageP3);

            byte[] sum = new byte[32], k = null, h = null;

            IHash hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
            hasher.TransformBytes(msg);

            for (int i = 0; i < publicKeys.Length; i++)
            {
                signatures[i] = new RingSignature();

                if (i == index)
                {
                    k = GetRandomSeed(true);
                    GroupOperations.ge_scalarmult_base(out GroupElementP3 tmp3, k, 0);
                    byte[] tmp3bytes = new byte[32];
                    GroupOperations.ge_p3_tobytes(tmp3bytes, 0, ref tmp3);
                    hasher.TransformBytes(tmp3bytes);
                    tmp3 = Hash2Point(pubs[i]);
                    GroupOperations.ge_scalarmult(out GroupElementP2 tmp2, k, ref tmp3);
                    byte[] tmp2bytes = new byte[32];
                    GroupOperations.ge_tobytes(tmp2bytes, 0, ref tmp2);
                    hasher.TransformBytes(tmp2bytes);
                }
                else
                {
                    signatures[i].C = GetRandomSeed(true);
                    signatures[i].R = GetRandomSeed(true);
                    GroupOperations.ge_frombytes(out GroupElementP3 tmp3, pubs[i], 0);
                    GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 tmp2, signatures[i].C, ref tmp3, signatures[i].R);
                    byte[] tmp2bytes = new byte[32];
                    GroupOperations.ge_tobytes(tmp2bytes, 0, ref tmp2);
                    hasher.TransformBytes(tmp2bytes);
                    tmp3 = Hash2Point(pubs[i]);
                    GroupOperations.ge_double_scalarmult_precomp_vartime(out tmp2, signatures[i].R, tmp3, signatures[i].C, image_pre);
                    tmp2bytes = new byte[32];
                    GroupOperations.ge_tobytes(tmp2bytes, 0, ref tmp2);
                    hasher.TransformBytes(tmp2bytes);
                    ScalarOperations.sc_add(sum, sum, signatures[i].C);
                }
            }

            h = hasher.TransformFinal().GetBytes();
            ScalarOperations.sc_sub(signatures[index].C, h, sum);
            ScalarOperations.sc_reduce32(signatures[index].C);
            ScalarOperations.sc_mulsub(signatures[index].R, signatures[index].C, secretKey, k);
            ScalarOperations.sc_reduce32(signatures[index].R);

            return signatures;
        }

        public bool Verify(byte[] msg, byte[] keyImage, IKey[] publicKeys, RingSignature[] signatures)
        {
            byte[][] pubs = publicKeys.Select(pk => pk.Value.ToArray()).ToArray();
            GroupOperations.ge_frombytes(out GroupElementP3 image_unp, keyImage, 0);

            GroupElementCached[] image_pre = new GroupElementCached[8];
            GroupOperations.ge_dsm_precomp(image_pre, ref image_unp);
            byte[] sum = new byte[32];

            IHash hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
            hasher.TransformBytes(msg);

            for (int i = 0; i < pubs.Length; i++)
            {
                if (ScalarOperations.sc_check(signatures[i].C) != 0 || ScalarOperations.sc_check(signatures[i].R) != 0)
                    return false;

                GroupOperations.ge_frombytes(out GroupElementP3 tmp3, pubs[i], 0);
                GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 tmp2, signatures[i].C, ref tmp3, signatures[i].R);
                byte[] tmp2bytes = new byte[32];
                GroupOperations.ge_tobytes(tmp2bytes, 0, ref tmp2);
                hasher.TransformBytes(tmp2bytes);
                tmp3 = Hash2Point(pubs[i]);
                GroupOperations.ge_double_scalarmult_precomp_vartime(out tmp2, signatures[i].R, tmp3, signatures[i].C, image_pre);
                tmp2bytes = new byte[32];
                GroupOperations.ge_tobytes(tmp2bytes, 0, ref tmp2);
                hasher.TransformBytes(tmp2bytes);
                ScalarOperations.sc_add(sum, sum, signatures[i].C);
            }

            byte[] h = hasher.TransformFinal().GetBytes();
            ScalarOperations.sc_reduce32(h);
            ScalarOperations.sc_sub(h, h, sum);

            int res = ScalarOperations.sc_isnonzero(h);

            return res == 0;
        }
        #region Private Functions

        private static byte[] GetRandomSeed(bool reduced = false)
        {
            byte[] seed = new byte[32];
            if (!reduced)
            {
                RNGCryptoServiceProvider.Create().GetNonZeroBytes(seed);
            }
            else
            {
                byte[] limit = { 0xe3, 0x6a, 0x67, 0x72, 0x8b, 0xce, 0x13, 0x29, 0x8f, 0x30, 0x82, 0x8c, 0x0b, 0xa4, 0x10, 0x39, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xf0 };
                bool isZero = false, less32 = false;
                do
                {
                    RNGCryptoServiceProvider.Create().GetNonZeroBytes(seed);
                    isZero = ScalarOperations.sc_isnonzero(seed) == 0;
                    less32 = Less32(seed, limit);
                } while (isZero && !less32);

                ScalarOperations.sc_reduce32(seed);
            }

            return seed;
        }

        private static GroupElementP3 Hash2Point(byte[] hashed)
        {
            byte[] hashValue = HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(hashed).GetBytes();
            //byte[] hashValue = HashFactory.Crypto.SHA3.CreateKeccak512().ComputeBytes(hashed).GetBytes();
            ScalarOperations.sc_reduce32(hashValue);
            GroupOperations.ge_fromfe_frombytes_vartime(out GroupElementP2 p2, hashValue, 0);
            GroupOperations.ge_mul8(out GroupElementP1P1 p1p1, ref p2);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 p3, ref p1p1);
            return p3;
        }

        private static bool Less32(byte[] k0, byte[] k1)
        {
            for (int n = 31; n >= 0; --n)
            {
                if (k0[n] < k1[n])
                    return true;
                if (k0[n] > k1[n])
                    return false;
            }
            return false;
        }

        private static byte[] GetPublicKey(byte[] secretKey)
        {
            byte[] pk = new byte[32];

            GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, secretKey, 0);
            GroupOperations.ge_p3_tobytes(pk, 0, ref p3);

            return pk;
        }


        #endregion Private Functions
    }
}

