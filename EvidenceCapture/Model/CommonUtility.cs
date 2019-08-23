using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model
{
    static class CommonUtility
    {
        /// <summary>
        /// stringからEnumを取得する
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static TEnum GetEnum<TEnum>(string value) where TEnum : struct
        {
            var type = typeof(TEnum);
            return (TEnum)Enum.Parse(type, value.ToLower());
        }

        /// <summary>
        /// intからEnumを取得する
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static TEnum GetEnum<TEnum>(int value) where TEnum : struct
        {
            var type = typeof(TEnum);
            return (TEnum)Enum.ToObject(type, value);
        }

    }
}
