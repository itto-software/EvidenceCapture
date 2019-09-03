using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model
{
    static class NLogHelper
    {
        public static string AppLogdir => Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                , CommonConstants.ServiceName
                , "Logs");

        internal static void NLogInitilalize()
        {
            var conf = new LoggingConfiguration();


            // 急ログ出力先パス

            var oldlogs = Path.Combine(AppLogdir, "old");

            // ログ出力フォルダの生成
            if (!Directory.Exists(AppLogdir))
                Directory.CreateDirectory(AppLogdir);

            if (!Directory.Exists(oldlogs))
                Directory.CreateDirectory(oldlogs);

            // メッセージログの設定
            {

                var applogFile = new FileTarget("file");
                applogFile.Encoding = Encoding.Default;
                applogFile.Name = "applog";
                applogFile.FileName = Path.Combine(AppLogdir, "applog.csv");
                applogFile.ArchiveFileName = Path.Combine(oldlogs, "{#}_applog.csv");


                applogFile.ArchiveEvery = FileArchivePeriod.Day;
                applogFile.ArchiveNumbering = ArchiveNumberingMode.Date;
                applogFile.ArchiveDateFormat = "yyyyMMdd";
                applogFile.MaxArchiveFiles = 30;

                // 出力内容の定義
                var csvL = new CsvLayout();
                csvL.Delimiter = CsvColumnDelimiterMode.Comma;
                csvL.Columns.Add(new CsvColumn() { Name = "date", Layout = "${longdate}" });
                csvL.Columns.Add(new CsvColumn() { Name = "level", Layout = "${uppercase:${level}}" });
                csvL.Columns.Add(new CsvColumn() { Name = "threadid", Layout = "${threadid}" });
                csvL.Columns.Add(new CsvColumn() { Name = "callsite", Layout = "${callsite}" });
                csvL.Columns.Add(new CsvColumn() { Name = "message", Layout = "${message}" });
                csvL.Columns.Add(new CsvColumn() { Name = "exception", Layout = "${exception:format=tostring}" });
                applogFile.Layout = csvL;

#if DEBUG
                conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, applogFile));

#else
                conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, applogFile));

#endif

            }
            

#if DEBUG
            // デバッグ時はコンソール上に表示
            {
                var console = new ConsoleTarget("console");
                conf.AddTarget(console);
                conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, console));
            }
#endif
            LogManager.Configuration = conf;
        }
    }
}
