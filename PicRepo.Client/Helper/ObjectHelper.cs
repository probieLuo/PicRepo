using System.Reflection;

namespace PicRepo.Client.Helper;

//
// 摘要:
//     对象帮助类
public static class ObjectHelper
{
    //
    // 摘要:
    //     比较两个对象的属性值和字段值是否全部相等
    //
    // 参数:
    //   obj1:
    //
    //   obj2:
    //
    //   type:
    //
    // 类型参数:
    //   T:
    public static bool Compare<T>(T obj1, T obj2, Type type)
    {
        if (CompareProperties(obj1, obj2, type))
        {
            return CompareFields(obj1, obj2, type);
        }

        return false;
    }

    //
    // 摘要:
    //     判断两个相同引用类型的对象的字段值是否相等
    //
    // 参数:
    //   obj1:
    //     对象1
    //
    //   obj2:
    //     对象2
    //
    //   type:
    //     按type类型中的字段进行比较
    //
    // 类型参数:
    //   T:
    public static bool CompareFields<T>(T obj1, T obj2, Type type)
    {
        if (obj1 == null && obj2 == null)
        {
            return true;
        }

        if (obj1 == null || obj2 == null)
        {
            return false;
        }

        FieldInfo[] fields = type.GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            if (IsCanCompare(fieldInfo.FieldType))
            {
                object? value = fieldInfo.GetValue(obj1);
                if (value != null && !value.Equals(fieldInfo.GetValue(obj2)))
                {
                    return false;
                }
            }
            else if (!CompareFields(fieldInfo.GetValue(obj1), fieldInfo.GetValue(obj2), fieldInfo.FieldType))
            {
                return false;
            }
        }

        return true;
    }

    //
    // 摘要:
    //     判断两个相同引用类型的对象的属性值是否相等
    //
    // 参数:
    //   obj1:
    //     对象1
    //
    //   obj2:
    //     对象2
    //
    //   type:
    //     按type类型中的属性进行比较
    //
    // 类型参数:
    //   T:
    public static bool CompareProperties<T>(T obj1, T obj2, Type type)
    {
        if (obj1 == null && obj2 == null)
        {
            return true;
        }

        if (obj1 == null || obj2 == null)
        {
            return false;
        }

        PropertyInfo[] properties = type.GetProperties();
        foreach (PropertyInfo propertyInfo in properties)
        {
            if (IsCanCompare(propertyInfo.PropertyType))
            {
                if (propertyInfo.GetValue(obj1, null) != propertyInfo.GetValue(obj2, null))
                {
                    return false;
                }

                continue;
            }

            return CompareProperties(propertyInfo.GetValue(obj1, null), propertyInfo.GetValue(obj2, null), propertyInfo.PropertyType);
        }

        return true;
    }

    //
    // 摘要:
    //     不同类型的属性复制
    //
    // 参数:
    //   source:
    //     源对象
    //
    //   dest:
    //     目标对象
    //
    // 类型参数:
    //   DT:
    //     目标对象的类型
    //
    //   ST:
    //     源对象的类型
    public static DT DeepCopy<DT, ST>(ST source, DT dest)
    {
        if (source == null || dest == null)
        {
            throw new ArgumentNullException("Source or destination object cannot be null.");
        }

        PropertyInfo[] properties = source.GetType().GetProperties();
        PropertyInfo[] properties2 = dest.GetType().GetProperties();
        foreach (PropertyInfo destProp in properties2)
        {
            if (destProp.CanWrite)
            {
                PropertyInfo propertyInfo = properties.SingleOrDefault((PropertyInfo s) => s.Name == destProp.Name);
                if (propertyInfo != null && propertyInfo.PropertyType == destProp.PropertyType)
                {
                    destProp.SetValue(dest, propertyInfo.GetValue(source, null), null);
                }
            }
        }

        return dest;
    }

    //
    // 摘要:
    //     不同类型的属性复制
    //
    // 参数:
    //   source:
    //     源对象
    //
    //   dest:
    //     目标对象
    //
    //   needSameType:
    //     是否必要类型一致，否时支持T和T?的相互复制
    //
    // 类型参数:
    //   DT:
    //     目标对象的类型
    //
    //   ST:
    //     源对象的类型
    public static DT DeepCopy<DT, ST>(ST source, DT dest, bool needSameType)
    {
        if (source == null || dest == null)
        {
            throw new ArgumentNullException("Source or destination object cannot be null.");
        }

        if (needSameType)
        {
            return DeepCopy(source, dest);
        }

        PropertyInfo[] properties = source.GetType().GetProperties();
        PropertyInfo[] properties2 = dest.GetType().GetProperties();
        foreach (PropertyInfo destProp in properties2)
        {
            if (!destProp.CanWrite)
            {
                continue;
            }

            PropertyInfo propertyInfo = properties.SingleOrDefault((PropertyInfo s) => s.Name == destProp.Name);
            if (!(propertyInfo != null))
            {
                continue;
            }

            try
            {
                if (propertyInfo.PropertyType == destProp.PropertyType)
                {
                    destProp.SetValue(dest, propertyInfo.GetValue(source, null), null);
                }
                else if (destProp.PropertyType.IsGenericType && destProp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && destProp.PropertyType.GetGenericArguments()[0] == propertyInfo.PropertyType)
                {
                    destProp.SetValue(dest, propertyInfo.GetValue(source, null), null);
                }
                else if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && propertyInfo.PropertyType.GetGenericArguments()[0] == destProp.PropertyType)
                {
                    object value = propertyInfo.GetValue(source, null);
                    if (value != null)
                    {
                        destProp.SetValue(dest, value, null);
                    }
                }
                else if (propertyInfo.PropertyType != typeof(string) && destProp.PropertyType == typeof(string))
                {
                    destProp.SetValue(dest, propertyInfo.GetValue(source, null)?.ToString() ?? "", null);
                }
                else
                {
                    if (!(propertyInfo.PropertyType == typeof(string)) || !(destProp.PropertyType != typeof(string)))
                    {
                        continue;
                    }

                    if (destProp.PropertyType.IsEnum)
                    {
                        destProp.SetValue(dest, Enum.Parse(destProp.PropertyType, propertyInfo.GetValue(source, null)?.ToString() ?? ""), null);
                    }
                    else if (destProp.PropertyType.IsGenericType && destProp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        Type conversionType = destProp.PropertyType.GetGenericArguments()[0];
                        string value2 = propertyInfo.GetValue(source, null) as string;
                        if (string.IsNullOrEmpty(value2))
                        {
                            destProp.SetValue(dest, null, null);
                        }
                        else
                        {
                            destProp.SetValue(dest, Convert.ChangeType(value2, conversionType), null);
                        }
                    }
                    else
                    {
                        destProp.SetValue(dest, Convert.ChangeType(propertyInfo.GetValue(source, null).ToString(), destProp.PropertyType), null);
                    }

                    continue;
                }
            }
            catch (Exception)
            {
            }
        }

        return dest;
    }

    //
    // 摘要:
    //     不同类型对象的属性复制
    //
    // 参数:
    //   source:
    //     源对象
    //
    //   dest:
    //     目标对象
    //
    //   typeToString:
    //     是：支持T转string，否：支持string转T
    //
    // 类型参数:
    //   DT:
    //     目标对象的类型
    //
    //   ST:
    //     源对象的类型
    //
    // 异常:
    //   T:System.Exception:
    public static DT DeepCopy2<DT, ST>(ST source, DT dest, bool typeToString)
    {
        if (source == null || dest == null)
        {
            throw new ArgumentNullException("Source or destination object cannot be null.");
        }

        PropertyInfo[] properties = source.GetType().GetProperties();
        PropertyInfo[] properties2 = dest.GetType().GetProperties();
        foreach (PropertyInfo destProp in properties2)
        {
            if (!destProp.CanWrite)
            {
                continue;
            }

            PropertyInfo propertyInfo = properties.SingleOrDefault((PropertyInfo s) => s.Name == destProp.Name);
            if (!(propertyInfo != null))
            {
                continue;
            }

            if (propertyInfo.PropertyType == destProp.PropertyType)
            {
                destProp.SetValue(dest, propertyInfo.GetValue(source, null), null);
            }
            else if (destProp.PropertyType.IsGenericType && destProp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && destProp.PropertyType.GetGenericArguments()[0] == propertyInfo.PropertyType)
            {
                destProp.SetValue(dest, propertyInfo.GetValue(source, null), null);
            }
            else if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && propertyInfo.PropertyType.GetGenericArguments()[0] == destProp.PropertyType)
            {
                object value = propertyInfo.GetValue(source, null);
                if (value != null)
                {
                    destProp.SetValue(dest, value, null);
                }
            }
            else if (typeToString && destProp.PropertyType == typeof(string))
            {
                destProp.SetValue(dest, propertyInfo.GetValue(source, null)?.ToString(), null);
            }
            else if (!typeToString && propertyInfo.PropertyType == typeof(string))
            {
                string value2 = propertyInfo.GetValue(source, null) as string;
                if (string.IsNullOrEmpty(value2))
                {
                    throw new Exception("属性【" + propertyInfo.Name + "】的值为空");
                }

                if (destProp.PropertyType.IsGenericType && destProp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    destProp.SetValue(dest, Convert.ChangeType(value2, destProp.PropertyType.GetGenericArguments()[0]), null);
                }
                else
                {
                    destProp.SetValue(dest, Convert.ChangeType(value2, destProp.PropertyType), null);
                }
            }
        }

        return dest;
    }

    //
    // 摘要:
    //     深度复制对象属性
    //
    // 参数:
    //   source:
    //
    // 类型参数:
    //   T:
    public static T DeepCopy<T>(T source)
    {
        if (source == null)
        {
            throw new ArgumentNullException("source", "Source object cannot be null.");
        }

        T val = Activator.CreateInstance<T>();
        PropertyInfo[] properties = source.GetType().GetProperties();
        foreach (PropertyInfo propertyInfo in properties)
        {
            if (propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(val, propertyInfo.GetValue(source, null), null);
            }
        }

        return val;
    }

    //
    // 摘要:
    //     获取对象指定属性的值
    //
    // 参数:
    //   obj:
    //
    //   propertyName:
    //
    // 类型参数:
    //   T:
    public static object? GetPropertieValue<T>(T obj, string propertyName)
    {
        if (obj == null)
        {
            return null;
        }

        PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        if (properties.Length == 0)
        {
            return null;
        }

        PropertyInfo propertyInfo = properties.FirstOrDefault((PropertyInfo s) => s.Name == propertyName);
        if (propertyInfo != null)
        {
            return propertyInfo.GetValue(obj, null);
        }

        return null;
    }

    //
    // 摘要:
    //     该类型是否可直接进行值的比较
    //
    // 参数:
    //   type:
    private static bool IsCanCompare(Type type)
    {
        if (type.IsValueType)
        {
            return true;
        }

        if (type.FullName == typeof(string).FullName)
        {
            return true;
        }

        return false;
    }
}