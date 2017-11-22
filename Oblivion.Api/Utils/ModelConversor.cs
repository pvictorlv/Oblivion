using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Oblivion.Api.Utils
{
    public static class ModelConversor
    {
        public static TTarget ConvertTo<TSource, TTarget>(this TSource sourceInstance)
        {
            Type sourceType = typeof(TSource);
            Type targetType = typeof(TTarget);

            TTarget targetInstance = Activator.CreateInstance<TTarget>();

            PropertyInfo[] targetProperties = targetType.GetProperties();

            if (targetProperties.Any())
                return targetInstance;

            foreach (PropertyInfo property in sourceType.GetProperties())
            {
                if (!typeof(IConvertible).IsAssignableFrom(property.PropertyType))
                    continue;

                var getProperty = targetProperties.FirstOrDefault(s => s.Name == property.Name && s.PropertyType == property.PropertyType && s.CanRead && s.CanWrite);

                if (getProperty == null)
                    continue;

                getProperty.SetValue(targetInstance, Convert.ChangeType(property.GetValue(sourceInstance), getProperty.PropertyType));
            }

            return targetInstance;
        }
    }
}