using NetOffice.OfficeApi.Enums;
using NetOffice.WordApi;
using NetOffice.WordApi.Enums;
using System;
using System.IO;
using System.Threading;

namespace just4net.doc
{
    public class WordUtil : IOfficeUtil, IPDFConversion
    {
        private static Application app = null;
        private static readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        protected override string ProcessName { get { return "WINWORD.EXE"; } }

        public int ConvertToPDF(string sourceFile, string pdfFile)
        {
            if (string.IsNullOrEmpty(sourceFile))
                throw new ArgumentNullException(nameof(sourceFile));
            if (string.IsNullOrEmpty(pdfFile))
                throw new ArgumentNullException(nameof(pdfFile));

            if (!File.Exists(sourceFile))
                throw new FileNotFoundException($"source file '{sourceFile}' doesn't exists");

            if (File.Exists(pdfFile))
                File.Delete(pdfFile);

            Check(true);
            Document document = null;
            try
            {
                document = app.Documents.Open(sourceFile);
                if (document != null)
                    document.SaveAs(pdfFile, WdSaveFormat.wdFormatPDF);
                return 1;
            }
            catch(Exception ex)
            {
                Kill();
                throw new Exception("convert failed: " + sourceFile, ex);
            }
            finally
            {
                document?.Close(WdSaveOptions.wdDoNotSaveChanges);
                document?.Dispose();
            }
        }

        public override bool Check(bool createIfNotExist)
        {
            locker.EnterUpgradeableReadLock();
            try
            {
                bool exists = true;
                if (app == null)
                    exists = false;
                else
                {
                    try { object obj = app.Build; }
                    catch { exists = false; }
                }

                if (!exists && createIfNotExist)
                {
                    Kill();
                    locker.EnterWriteLock();
                    try
                    {
                        app = Create();
                        return true;
                    }
                    finally { locker.ExitWriteLock(); }
                }
                return exists;
            }
            finally { locker.ExitUpgradeableReadLock(); }
        }
        
        private static Application Create()
        {
            Application application = new Application();
            application.DisplayAlerts = WdAlertLevel.wdAlertsNone;
            application.ScreenUpdating = false;
            application.Visible = false;
            application.FileValidation = MsoFileValidationMode.msoFileValidationSkip;
            return application;
        }
    }
}
