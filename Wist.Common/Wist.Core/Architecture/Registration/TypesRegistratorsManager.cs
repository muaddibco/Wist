using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Text;

namespace Wist.Core.Architecture.Registration
{
    internal class TypesRegistratorsManager
    {
        [ImportMany]
        public IEnumerable<TypeRegistratorBase> _registrators { get; set; }

        public IEnumerable<TypeRegistratorBase> GetAllRegistrators(ComposablePartCatalog catalog)
        {
            //Create the CompositionContainer with the parts in the catalog
            var mefContainer = new CompositionContainer(catalog);

            //Fill the imports of this object
            mefContainer.ComposeParts(this);

            // remove duplicates in case of multiple assembly copy
            DeduplicateRegistrators();

            return _registrators;
        }

        private void DeduplicateRegistrators()
        {
            var deduplicated = new List<TypeRegistratorBase>();

            if (_registrators == null)
            {
                return;
            }

            var added = new HashSet<Type>();

            foreach (var registrator in _registrators)
            {
                if (added.Contains(registrator.GetType()))
                {
                    continue;
                }

                added.Add(registrator.GetType());

                deduplicated.Add(registrator);
            }

            _registrators = deduplicated;
        }
    }
}
