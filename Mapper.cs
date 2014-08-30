using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TKQuery
{
    /// <summary>
    /// Contains methods that are used to map object fields with provided values
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public class Mapper<T1>// where T1 : new()
    {
        /// <summary>
        /// Maps passed object with given values
        /// </summary>
        /// <param name="data">key/value with aliases and values</param>
        /// <returns></returns>
        public T1 Map(List<IDictionary<string, object>> datas)
        {
            //start from scrach, so new instance of generic type and empty path string is needed
            var inst = new object();
            if (typeof(T1).GetConstructor(Type.EmptyTypes) != null)
            {
                inst = Activator.CreateInstance<T1>();
            }
            var obj = Process(inst, datas, "");
            //casting for valid return type            
            return (T1)Convert.ChangeType(obj, typeof(T1));
        }

        /// <summary>
        /// Recursive method which checks object type and tries to find values for its properties
        /// </summary>
        /// <param name="orginalObj"></param>
        /// <param name="data"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private object Process(object orginalObj, List<IDictionary<string, object>> datas, string path)
        {
            Type type = orginalObj.GetType();
            object returnObj = null;
            var properties = GetProperties(type);
            var validConstructor = GetValidConstructor(type.GetConstructors(), datas);
            if (datas.Count == 0)
            {
                return null;
            }
            //nested objects overflow check
            if (path.Count((c) => c.ToString() == ".") > 100)
            {
                return null;
            }
            if (IsList(type))
            {
                Type genericArgument = type.GetGenericArguments()[0];
                var method = type.GetMethod("Add");
                foreach (var data in datas)
                {
                    var propertyObject = CreateInstance(genericArgument, datas);
                    var tempDatas = new List<IDictionary<string, object>>();
                    tempDatas.Add(data);
                    var genericObject = Process(propertyObject, tempDatas, path);
                    method.Invoke(orginalObj, new[] { genericObject });
                }
                returnObj = orginalObj;
            }
            else if (IsExtendedType(type) && properties.Length > 0)
            {
                foreach (var property in properties)
                {
                    var propertyType = property.PropertyType;
                    //build path to find object property value
                    var sb = new StringBuilder();
                    if (path.Length > 0)
                    {
                        sb.Append(path);
                        sb.Append(".");
                    }
                    sb.Append(property.Name);
                    var propertyPath = sb.ToString();
                    var propertyObject = CreateInstance(propertyType, null);
                    returnObj = Process(propertyObject, datas, propertyPath);
                    //primitive type cannot be set to null, missing fields in results
                    SetValue(property, orginalObj, returnObj);
                }
                //return object with filled properties
                return orginalObj;
            }
            else if (validConstructor != null && validConstructor.GetParameters().Count() > 0)
            {
                var data = datas.First();
                var args = new List<object>();
                foreach (var parameter in validConstructor.GetParameters())
                {
                    var arg = data[parameter.Name];
                    args.Add(arg);
                }
                returnObj = validConstructor.Invoke(args.ToArray());
            }
            else
            {
                IDictionary<string, object> data = null;
                if (datas.Count > 0)
                {
                    data = datas.First();
                    //datas.Remove(data);
                }
                returnObj = GetValue(data, path);
            }
            //DBNull fix
            if (returnObj is DBNull)
            {
                returnObj = null;
            }
            //Primitive type fix
            if (returnObj == null)
            {
                returnObj = default(T1);
            }
            return returnObj;
        }

        private ConstructorInfo GetValidConstructor(ConstructorInfo[] constructors, List<IDictionary<string, object>> datas)
        {
            if (datas.Count() > 0)
            {
                var data = datas.First();
                if (data != null)
                {
                    foreach (var constructor in constructors)
                    {
                        var parameters = constructor.GetParameters();
                        var parametersString = string.Join(",", parameters.OrderBy(p => p.Name).Select(p => p.Name.ToLower()));
                        var keysString = string.Join(",", data.Keys.OrderBy(k => k).Select(k => k.ToLower()));
                        return constructor;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if provided type is a 'list'
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsList(Type type)
        {
            var hasArguments = type.GetGenericArguments().Length > 0;
            var hasCount = type.GetProperty("Count") != null;
            var hasAdd = type.GetMethod("Add") != null;
            return hasArguments && hasCount && hasAdd;
        }

        /// <summary>
        /// Sets value to property of original object
        /// </summary>
        /// <param name="property"></param>
        /// <param name="orginalObj"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private void SetValue(System.Reflection.PropertyInfo property, object orginalObj, object value)
        {
            if (property.PropertyType.IsPrimitive && value == null)
            {
                throw new Exception("No valid value for primitive property type");
            }
            object convertedValue = value;
            if (property.PropertyType == typeof(double))
            {
                if (value is string)
                {
                    double result;
                    if (!double.TryParse(value.ToString(), out result))
                    {
                        if (!double.TryParse(value.ToString().Replace(".", ","), out result))
                        {
                            throw new FormatException("Cannot parse to double");
                        }
                    }
                    convertedValue = result;
                }
            }
            else if (property.PropertyType == typeof(bool))
            {
                var valueString = value.ToString().ToLower();
                if (valueString == "true" || valueString == "1")
                {
                    convertedValue = true;
                }
                else if (valueString == "false" || valueString == "0")
                {
                    convertedValue = false;
                }
            }
            else if (property.PropertyType == typeof(char))
            {
                if (value is string)
                {
                    char result;
                    if (!char.TryParse(value.ToString(), out result))
                    {
                        throw new FormatException("Cannot parse to char");
                    }
                    convertedValue = result;
                }
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                if (value is string)
                {
                    DateTime result;
                    if (!DateTime.TryParse(value.ToString(), out result))
                    {
                        throw new FormatException("Cannot parse to datetime");
                    }
                    convertedValue = result;
                }
            }
            else if (property.PropertyType == typeof(int))
            {
                convertedValue = Convert.ToInt32(value);
            }
            property.SetValue(orginalObj, convertedValue);
        }

        /// <summary>
        /// Creates instance of class with parametrless constructor or just object
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        private object CreateInstance(Type propertyType, List<IDictionary<string, object>> datas)
        {
            if (datas != null && datas.Count > 0)
            {
                var data = datas.First();
                var constructors = propertyType.GetConstructors();
                if (data != null && GetValidConstructor(constructors, datas) != null)
                {
                    return Activator.CreateInstance(propertyType);
                }
            }
            var parametlessConstructor = propertyType.GetConstructor(Type.EmptyTypes);
            if (parametlessConstructor != null)
            {
                return Activator.CreateInstance(propertyType);
            }

            return new object();
        }

        /// <summary>
        /// Checks for class
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsExtendedType(Type type)
        {
            if (type.IsClass) { return true; }
            return false;
        }

        /// <summary>
        /// Search data keys and returns found value
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private object GetValue(IDictionary<string, object> data, string path)
        {
            object value = null;
            if (data != null)
            {
                var result = data.FirstOrDefault((k) => k.Key.ToLower() == path.ToLower());
                if (result.Key != null)
                {
                    data.Remove(result.Key);
                    value = result.Value;
                }
                else
                {
                    if (data.Count > 0 && path.Length == 0)
                    {
                        value = data.First().Value;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Gets object properties with valid flags
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private System.Reflection.PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        }
    }
}
