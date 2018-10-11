using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Parsers;
using Wist.Core.Translators;

namespace Wist.BlockLattice.SQLite.Mappers.Transactional
{
    public abstract class TransactionalMapperBase<TFrom, TTo> : TranslatorBase<TFrom, TTo>
    {
        protected readonly IBlockParsersRepository _blockParsersRepository;

        public TransactionalMapperBase(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
        {
            _blockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(PacketType.Transactional);
        }
    }
}
