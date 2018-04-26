using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core
{
    public interface IFactory<T>
    {
        T Create();

        void Utilize(T obj);
    }

    public interface IFactory<T, Key>
    {
        T Create(Key key);

        void Utilize(T obj);
    }
}
