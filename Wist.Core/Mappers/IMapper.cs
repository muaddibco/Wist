using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Mappers
{
    public interface IMapper<TFrom, TTo> : IMapper
    {
        TTo Convert(TFrom obj);
    }

    [ExtensionPoint]
    public interface IMapper
    {
        string MapFrom { get; }
        string MapTo { get; }

        object Convert(object obj);
    }
}
