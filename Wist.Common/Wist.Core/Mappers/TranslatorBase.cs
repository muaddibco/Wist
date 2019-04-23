namespace Wist.Core.Translators
{
    public abstract class TranslatorBase<TFrom, TTo> : ITranslator<TFrom, TTo>
    {
        public string Source => typeof(TFrom).FullName;

        public string Target => typeof(TTo).FullName;

        public abstract TTo Translate(TFrom obj);
        
        public object Translate(object obj)
        {
            return Translate((TFrom)obj);
        }
    }
}
