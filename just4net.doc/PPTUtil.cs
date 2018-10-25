using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi;
using NetOffice.PowerPointApi.Enums;
using System;
using System.IO;
using System.Threading;

namespace just4net.doc
{
    public class PPTUtil : IOfficeUtil, IPDFConversion
    {
        private static Application app;
        private static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        protected override string ProcessName { get { return "POWERPNT.EXE"; } }

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
                    try { app = Create(); return true; }
                    finally { locker.ExitWriteLock(); }
                }
                return exists;
            }
            finally { locker.ExitUpgradeableReadLock(); }
        }

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

            if (!Check(true))
                return -1;

            Presentation presentation = null;
            try
            {
                presentation = app.Presentations.Open(sourceFile, MsoTriState.msoTrue, MsoTriState.msoFalse, MsoTriState.msoFalse);
                presentation.SaveAs(pdfFile, PpSaveAsFileType.ppSaveAsPDF);
                return 1;
            }
            catch (Exception ex)
            {
                Kill();
                throw new Exception("convert failed：" + sourceFile, ex);
            }
            finally
            {
                presentation?.Close();
                presentation?.Dispose();
            }
        }

        private Application Create()
        {
            Application application = new Application()
            {
                DisplayAlerts = PpAlertLevel.ppAlertsNone,
                AutomationSecurity = MsoAutomationSecurity.msoAutomationSecurityForceDisable
            };
            application.FileValidation = MsoFileValidationMode.msoFileValidationSkip;
            return application;
        }
    }
}
