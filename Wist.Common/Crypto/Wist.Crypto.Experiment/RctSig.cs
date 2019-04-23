using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wist.Crypto.Experiment
{

    //containers For CT operations
    //if it's  representing a private ctkey then "dest" contains the secret key of the address
    // while "mask" contains a where C = aG + bH is CT pedersen commitment and b is the amount
    // (store b, the amount, separately
    //if it's representing a public ctkey, then "dest" = P the address, mask = C the commitment
    public struct CtKey
    {
        private Key _dest;
        private Key _mask; //C here if public

        /// <summary>
        /// if it's  representing a private ctkey then "dest" contains the secret key of the address; 
        /// if it's representing a public ctkey, then "dest" = P the address; 
        /// </summary>
        public Key Dest { get => _dest; set => _dest = value; }

        /// <summary>
        /// if it's  representing a private ctkey then "mask" contains "a" (blinding factor) where "C = aG + bH" is CT pedersen commitment and b is the amount; 
        /// if it's representing a public ctkey, then "mask" is "C" - the commitment;
        /// </summary>
        public Key Mask { get => _mask; set => _mask = value; }
    };

    public class CtKeyList : List<CtKey>
    {
        public void Init(int size)
        {
            for (int i = 0; i < size; i++)
            {
                Add(new CtKey());
            }
        }
    }

    public class CtKeyMatrix : List<CtKeyList>
    {
    }

    public class Key : ICloneable
    {
        public Key(byte[] bytes)
        {
            Bytes = bytes;
        }

        public Key()
        {
            Bytes = new byte[32];
        }

        private byte[] _bytes;

        public byte[] Bytes
        {
            get
            {
                return _bytes;
            }

            set
            {
                if(value != null && value.Length != 32)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _bytes = value;
            }
        }

        public object Clone() => new Key((byte[])_bytes.Clone());
    }

    public class Key64
    {

        public Key64()
        {
            Keys = new Key[64];
            for (int i = 0; i < 64; i++)
            {
                Keys[i] = new Key();
            }
        }

        private Key[] _keys;

        public Key[] Keys
        {
            get
            {
                return _keys;
            }

            set
            {
                if(value != null && value.Length != 64)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _keys = value;
            }
        }
    }

    public class KeysList : List<Key>
    {
        public KeysList()
        {

        }

        public KeysList(int rows)
        {
            for (int i = 0; i < rows; i++)
            {
                Add(new Key());
            }
        }

        public KeysList(List<Key> keys)
        {
            foreach (Key key in keys)
            {
                Add(new Key((byte[])key.Bytes.Clone()));
            }
        }
    }

    public class KeysMatrix : List<KeysList>
    {
        public KeysMatrix()
        {

        }

        public KeysMatrix(int cols, int rows)
        {
            for (int i = 0; i < cols; i++)
            {
                KeysList keys = new KeysList();
                Add(keys);
                for (int j = 0; j < rows; j++)
                {
                    keys.Add(new Key());
                }
            }
        }

        public KeysMatrix(int cols, KeysList keysTemplate)
        {
            for (int i = 0; i < cols; i++)
            {
                KeysList keys = new KeysList(keysTemplate);
                Add(keys);
            }
        }
    }

    public class BoroSig
    {
        public BoroSig()
        {
            S0 = new Key64();
            S1 = new Key64();
            Ee = new Key();
        }

        private Key64 _s0;
        private Key64 _s1;
        private Key _ee;

        public Key64 S0 { get => _s0; set => _s0 = value; }
        public Key64 S1 { get => _s1; set => _s1 = value; }
        public Key Ee { get => _ee; set => _ee = value; }
    };

    //contains the data for an Borromean sig
    // also contains the "Ci" values such that
    // \sum Ci = C
    // and the signature proves that each Ci is either
    // a Pedersen commitment to 0 or to 2^i
    //thus proving that C is in the range of [0, 2^64]
    public class RangeSig
    {
        BoroSig _asig;
        Key64 _ci;

        public RangeSig()
        {
            Asig = new BoroSig();
            Ci = new Key64();
        }

        public BoroSig Asig { get => _asig; set => _asig = value; }

        /// <summary>
        /// Commitment per each bit
        /// </summary>
        public Key64 Ci { get => _ci; set => _ci = value; }
    };

    //just contains the necessary keys to represent MLSAG sigs
    //c.f. https://eprint.iacr.org/2015/1098
    public class MgSig
    {
        private KeysMatrix _ss;
        private Key _cc;
        private KeysList _ii;

        public MgSig()
        {
            CC = new Key();
        }

        public KeysMatrix SS { get => _ss; set => _ss = value; }
        public Key CC { get => _cc; set => _cc = value; }

        /// <summary>
        /// this field is not serialized because it can be reconstructed 
        /// </summary>
        internal KeysList II { get => _ii; set => _ii = value; }
    };

    //data for passing the amount to the receiver secretly
    // If the pedersen commitment to an amount is C = aG + bH,
    // "mask" contains a 32 byte key a
    // "amount" contains a hex representation (in 32 bytes) of a 64 bit number
    // "senderPk" is not the senders actual public key, but a one-time public key generated for
    // the purpose of the ECDH exchange
    public class EcdhTuple
    {
        public byte[] Mask { get; set; }
        public byte[] Amount { get; set; }
    };

    public class RctSigBase
    {
        private Key _message; //32 bytes
        private CtKeyMatrix _mixRing; //the set of all pubkeys / copy
        //pairs that you mix with
        private List<byte[]> _pseudoOuts; //C - for simple rct
        private List<EcdhTuple> _ecdhInfo;
        private CtKeyList _outPk;

        public RctSigBase()
        {
            MixRing = new CtKeyMatrix();
            PseudoOuts = new List<byte[]>();
            EcdhInfo = new List<EcdhTuple>();
            OutPk = new CtKeyList();
        }

        public Key Message { get => _message; set => _message = value; }
        public CtKeyMatrix MixRing { get => _mixRing; set => _mixRing = value; }
        public List<byte[]> PseudoOuts { get => _pseudoOuts; set => _pseudoOuts = value; }
        public List<EcdhTuple> EcdhInfo { get => _ecdhInfo; set => _ecdhInfo = value; }
        public CtKeyList OutPk { get => _outPk; set => _outPk = value; }
    };

    public class RctSigPrunable
    {
        private List<RangeSig> _rangeSigs;
        private List<MgSig> _mgs; // simple rct has N, full has 1
        //private KeysList _pseudoOuts; //C - for simple rct

        public List<RangeSig> RangeSigs { get => _rangeSigs; set => _rangeSigs = value; }
        public List<MgSig> MGs { get => _mgs; set => _mgs = value; }
        //public KeysList PseudoOuts { get => _pseudoOuts; set => _pseudoOuts = value; }
    };

    public class RctSig : RctSigBase
    {
        private RctSigPrunable _p;

        public RctSig()
            : base()
        {
            P = new RctSigPrunable();
        }

        public RctSigPrunable P { get => _p; set => _p = value; }
    };
}
