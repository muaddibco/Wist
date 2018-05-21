using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Mappers
{
    public abstract class MapperBase<TFrom, TTo> : IMapper<TFrom, TTo>
    {
        public string MapFrom => typeof(TFrom).FullName;

        public string MapTo => typeof(TTo).FullName;

        public abstract TTo Convert(TFrom obj);
        
        public object Convert(object obj)
        {
            return Convert((TFrom)obj);
        }
    }
}
