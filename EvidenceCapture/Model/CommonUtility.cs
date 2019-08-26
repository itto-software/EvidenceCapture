using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        /// <summary>ファイルエクスプローラーを開く</summary>
        /// <param name="path"></param>
        internal static void Explorer(string path)
        {
            if (!System.IO.Directory.Exists(path) && !System.IO.File.Exists(path))
            {
                // todo エラー処理
                return;
            }
            System.Diagnostics.Process.Start(path);
        }

        internal static List<int> GetLevelsByStr(string targetPattern)
        {
            var levels = new List<int>();


            var matche = Regex.Matches(targetPattern, "[0-9]+");

            foreach (var mstr in matche)
            {
                levels.Add(int.Parse(mstr.ToString()));
            }
            levels.Reverse();

            return levels;



        }

        internal static string GetGroupNameByLevels(List<int> levels)
        {
            var sourceStr = ApplicationSettings.Instance.GroupPattern;

            var re = new Regex("\\[n\\]");

            levels.Reverse();
            levels.ForEach(
                level =>
                {
                    sourceStr = re.Replace(sourceStr, level.ToString(), 1);

                });
            levels.Reverse();
            return sourceStr;
        }
    }
}
