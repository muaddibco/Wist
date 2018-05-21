using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Mappers
{
    [ServiceContract]
    public interface IMapperFactory
    {
        IMapper<TFrom, TTo> GetMapper<TFrom, TTo>();
    }
}
