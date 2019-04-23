using System;
using System.Text;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class IssueAssetParser : TransactionalBlockParserBase
    {
        public IssueAssetParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_IssueAsset;

        protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
        {
            IssueAsset block = null;

            if(version == 1)
            {
                int readBytes = 0;
                byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte strLen = spanBody.Slice(readBytes, 1).ToArray()[0];
                readBytes++;

                string issuedAssetInfo = Encoding.ASCII.GetString(spanBody.Slice(readBytes, strLen).ToArray());
                readBytes += strLen;

                AssetIssuance assetIssuance = new AssetIssuance
                {
                    AssetId = assetId,
                    IssuedAssetInfo = issuedAssetInfo
                };

                block = new IssueAsset
                {
                    AssetIssuance = assetIssuance
                };

                transactionalBlockBase = block;
                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
