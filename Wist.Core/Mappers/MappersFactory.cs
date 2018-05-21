using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Mappers
{
    public class MappersFactory : IMapperFactory
    {
        private readonly Dictionary<string, Dictionary<string, Stack<IMapper>>> _mappers;

        public MappersFactory(IMapper[] mappers)
        {
            _mappers = new Dictionary<string, Dictionary<string, Stack<IMapper>>>();

            foreach (IMapper mapper in mappers)
            {
                if(!_mappers.ContainsKey(mapper.MapFrom))
                {
                    _mappers.Add(mapper.MapFrom, new Dictionary<string, Stack<IMapper>>());
                }

                if(!_mappers[mapper.MapFrom].ContainsKey(mapper.MapTo))
                {
                    _mappers[mapper.MapFrom].Add(mapper.MapTo, new Stack<IMapper>());
                }

                _mappers[mapper.MapFrom][mapper.MapTo].Push(mapper);
            }
        }

        public IMapper<TFrom, TTo> GetMapper<TFrom, TTo>()
        {
            string mapFrom = typeof(TFrom).FullName;
            string mapTo = typeof(TTo).FullName;

            if (!_mappers.ContainsKey(mapFrom))
            {
                return null;
            }

            if (!_mappers[mapFrom].ContainsKey(mapTo))
            {
                return null;
            }

            IMapper<TFrom, TTo> mapper;
            if (_mappers[mapFrom][mapTo].Count > 1)
            {
                mapper = (IMapper<TFrom, TTo>)_mappers[mapFrom][mapTo].Pop();
            }
            else
            {
                IMapper<TFrom, TTo> mapperTemp = (IMapper<TFrom, TTo>)_mappers[mapFrom][mapTo].Pop();
                mapper = ServiceLocator.Current.GetInstance<IMapper<TFrom, TTo>>();
                _mappers[mapFrom][mapTo].Push(mapperTemp);
            }

            return mapper;
        }
    }
}
