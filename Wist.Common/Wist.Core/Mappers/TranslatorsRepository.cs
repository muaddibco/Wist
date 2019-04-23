using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;

namespace Wist.Core.Translators
{
    [RegisterDefaultImplementation(typeof(ITranslatorsRepository), Lifetime = LifetimeManagement.Singleton)]
    public class TranslatorsRepository : ITranslatorsRepository
    {
        private readonly Dictionary<string, Dictionary<string, ITranslator>> _translatorsPool;
        private readonly IApplicationContext _applicationContext;

        public TranslatorsRepository(IApplicationContext applicationContext, ITranslator[] mappers)
        {
            _translatorsPool = new Dictionary<string, Dictionary<string, ITranslator>>();

            foreach (ITranslator mapper in mappers)
            {
                if(!_translatorsPool.ContainsKey(mapper.Source))
                {
                    _translatorsPool.Add(mapper.Source, new Dictionary<string, ITranslator>());
                }

                if(!_translatorsPool[mapper.Source].ContainsKey(mapper.Target))
                {
                    _translatorsPool[mapper.Source].Add(mapper.Target, mapper);
                }
            }

            this._applicationContext = applicationContext;
        }

        public ITranslator<TFrom, TTo> GetInstance<TFrom, TTo>()
        {
            string from = typeof(TFrom).FullName;
            string to = typeof(TTo).FullName;

            ITranslator<TFrom, TTo> translator = GetInstance(from, to) as ITranslator<TFrom, TTo>;

            return translator;
        }

        public ITranslator GetInstance(string from, string to)
        {
            if (!_translatorsPool.ContainsKey(from) || !_translatorsPool[from].ContainsKey(to))
            {
                throw new TranslatorNotFoundException(from, to);
            }

            return _translatorsPool[from][to];
        }
    }
}
