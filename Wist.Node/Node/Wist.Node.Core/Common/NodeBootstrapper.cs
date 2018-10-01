using CommonServiceLocator;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Wist.Core.Architecture;
using Wist.Node.Core.Exceptions;

namespace Wist.Node.Core.Common
{
    public class NodeBootstrapper : Bootstrapper
    {
        private readonly string[] _catalogItems = new string[] { "Wist.Crypto.dll", "Chaos.NaCl.dll", "Wist.Network.dll", "Wist.Node.Core.dll" };

        public NodeBootstrapper(CancellationToken ct) : base(ct)
        {
        }

        public override void Run(IDictionary<string, string> args)
        {
            _log.Info("Starting NodeBootstrap Run");

            try
            {
                if (args == null)
                {
                    throw new System.ArgumentNullException(nameof(args));
                }

                if(!args.ContainsKey("secretKey"))
                {
                    throw new MandatoryInputArgumentIsMissingException("secretKey");
                }

                byte[] sk = GetSecretKey(args["secretKey"]);

                base.Run();

                StartNode(sk);
            }
            finally
            {
                _log.Info("NodeBootstrap Run completed");
            }
        }

        protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
        {
            return base.EnumerateCatalogItems(rootFolder)
                .Concat(_catalogItems)
                .Concat(Directory.EnumerateFiles(rootFolder, "Wist.BlockLattice.*.dll").Select(f => new FileInfo(f).Name));
        }

        #region Private Functions

        protected virtual void StartNode(byte[] secretKey)
        {
            if (secretKey == null)
            {
                throw new System.ArgumentNullException(nameof(secretKey));
            }

            _log.Info("Starting Node");
            try
            {
                NodeMain nodeMain = ServiceLocator.Current.GetInstance<NodeMain>();

                nodeMain.Initialize(secretKey, _cancellationToken);

                nodeMain.Start();
            }
            finally
            {
                _log.Info("Starting Node completed");
            }
        }

        private static byte[] GetSecretKey(string secretKeyExpression)
        {
            byte[] sk = new byte[32];

            bool isValid = true;

            if (secretKeyExpression.Length != 64)
            {
                isValid = false;
            }
            else
            {
                for (int i = 0; i < 32; i++)
                {
                    string byteValueExpression = $"{secretKeyExpression[i * 2]}{secretKeyExpression[i * 2 + 1]}";

                    if (byte.TryParse(byteValueExpression, NumberStyles.HexNumber, null, out byte byteValue))
                    {
                        sk[i] = byteValue;
                    }
                    else
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            if (!isValid)
            {
                throw new SecretKeyInvalidException();
            }

            return sk;
        }

        #endregion Private Functions
    }
}
