using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;

namespace Wist.Core.Translators
{
    [RegisterDefaultImplementation(typeof(ITranslatorsFactory), Lifetime = LifetimeManagement.Singleton)]
    public class TranslatorsFactory : ITranslatorsFactory
    {
        private readonly Dictionary<string, Dictionary<string, Stack<ITranslator>>> _translatorsPool;

        public TranslatorsFactory(ITranslator[] mappers)
        {
            _translatorsPool = new Dictionary<string, Dictionary<string, Stack<ITranslator>>>();

            foreach (ITranslator mapper in mappers)
            {
                if(!_translatorsPool.ContainsKey(mapper.Source))
                {
                    _translatorsPool.Add(mapper.Source, new Dictionary<string, Stack<ITranslator>>());
                }

                if(!_translatorsPool[mapper.Source].ContainsKey(mapper.Target))
                {
                    _translatorsPool[mapper.Source].Add(mapper.Target, new Stack<ITranslator>());
                }

                _translatorsPool[mapper.Source][mapper.Target].Push(mapper);
            }
        }

        public ITranslator<TFrom, TTo> GetTranslator<TFrom, TTo>()
        {
            string from = typeof(TFrom).FullName;
            string to = typeof(TTo).FullName;

            if (!_translatorsPool.ContainsKey(from) || !_translatorsPool[from].ContainsKey(to))
            {
                throw new TranslatorNotFoundException(from, to);
            }

            ITranslator<TFrom, TTo> translator;
            if (_translatorsPool[from][to].Count > 1)
            {
                translator = (ITranslator<TFrom, TTo>)_translatorsPool[from][to].Pop();
            }
            else
            {
                ITranslator<TFrom, TTo> translatorTemp = (ITranslator<TFrom, TTo>)_translatorsPool[from][to].Pop();
                translator = ServiceLocator.Current.GetInstance<ITranslator<TFrom, TTo>>();
                _translatorsPool[from][to].Push(translatorTemp);
            }

            return translator;
        }
    }
}
