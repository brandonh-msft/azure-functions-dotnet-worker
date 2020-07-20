﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionsDotNetWorker.Converters
{
    public class ParameterConverterManager
    {
        private readonly IEnumerable<IParameterConverter> _converters;

        public ParameterConverterManager(IEnumerable<IParameterConverter> converters)
        {
            _converters = converters;
        }

        public bool TryConvert(object source, Type targetType, string name, out object target)
        {
            foreach (IParameterConverter converter in _converters)
            {
                if (converter.TryConvert(source, targetType, name, out target))
                {
                    return true;
                }
            }

            target = default;
            return false;
        }
    }
}
