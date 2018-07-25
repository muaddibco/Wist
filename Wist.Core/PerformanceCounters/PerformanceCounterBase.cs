using System;
using System.Diagnostics;
using System.Security.Policy;

namespace Wist.Core.PerformanceCounters
{
    public class PerformanceCounterBase
    {
        protected string _instanceName;

        static PerformanceCounterBase()
        {
            Type t = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
        }

        protected PerformanceCounter _counter;

        public PerformanceCounterBase()
        {

        }

        // TODO: get expected type from attributes
        //public PerformanceCounterBase(string categoryName, string counterName, string instanceName, PerformanceCounterType expectedType, bool readOnly = false)
        //{
        //    _instanceName = instanceName;

        //    try
        //    {
        //        _counter = new PerformanceCounter(categoryName, counterName, instanceName, readOnly);
        //    }
        //    catch (System.InvalidOperationException ex)
        //    {
        //        //TODO: add logging
        //        throw ex;
        //    }
        //    catch (System.ArgumentNullException ex)
        //    {
        //        //TODO: add logging
        //        throw ex;
        //    }
        //    catch (System.UnauthorizedAccessException ex)
        //    {
        //        //TODO: add logging
        //        throw ex;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        //TODO: add logging
        //        throw ex;
        //    }

        //    if (!_counter.CounterType.Equals(expectedType))
        //    {
        //        //TODO: add logging
        //        _counter.RemoveInstance();
        //        throw new Exceptions.InvalidContractException($"Expected counter to be of type {expectedType}, instead the type was {_counter.CounterType}");
        //    }
        //}

        public virtual void Initialize(string categoryName, string counterName, string instanceName, PerformanceCounterType expectedType)
        {
            _instanceName = instanceName;

            try
            {
                _counter = new PerformanceCounter(categoryName, counterName, instanceName, false);
            }
            catch (System.InvalidOperationException ex)
            {
                //TODO: add logging
                throw ex;
            }
            catch (System.ArgumentNullException ex)
            {
                //TODO: add logging
                throw ex;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                //TODO: add logging
                throw ex;
            }
            catch (System.Exception ex)
            {
                //TODO: add logging
                throw ex;
            }

            if (!_counter.CounterType.Equals(expectedType))
            {
                //TODO: add logging
                _counter.RemoveInstance();
                throw new Exceptions.InvalidContractException($"Expected counter to be of type {expectedType}, instead the type was {_counter.CounterType}");
            }
        }

        public void RemoveInstance()
        {
            try
            {
                _counter.RemoveInstance();
            }
            catch (Exception ex)
            {
                // Nothing to be done, maybe this instance has already been removed
            }
        }

        public string CounterName
        {
            get
            {
                return _counter.CounterName;
            }
        }

        public long RawValue
        {
            get
            {
                return _counter.RawValue;
            }
            set
            {
                _counter.RawValue = value;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return _counter.ReadOnly;
            }
        }

        public CounterSample NextSample()
        {
            return _counter.NextSample();
        }
    }
}
