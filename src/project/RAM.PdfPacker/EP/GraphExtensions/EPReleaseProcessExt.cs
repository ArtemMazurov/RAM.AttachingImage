using PdfSharp;
using PX.Data;
using PX.Objects.EP;
using PX.SM;
using RAM.PdfPacker.Helper;
using System;

namespace RAM.PdfPacker.EP
{
    public class EPReleaseProcessExt : PXGraphExtension<EPReleaseProcess>
    {
        public static bool IsActive() => true;

        [PXOverride]
        public virtual void ReleaseDocProc(EPExpenseClaim claim, Action<EPExpenseClaim> baseMethod)
        {
            using PXTransactionScope ts = new PXTransactionScope();

            ExpenseClaimEntry claimEntry = null;
            PXGraph.InstanceCreated.AddHandler<ExpenseClaimEntry>(graph => claimEntry = graph);

            baseMethod.Invoke(claim);

            if (claimEntry.ExpenseClaim?.Current != null)
            {
                CreateCombinedPdf(claimEntry);
            }

            ts.Complete();

        }

        private void CreateCombinedPdf(ExpenseClaimEntry claimEntry)
        {
            var pdfPacker = new PdfPacker(PageSize.Letter);
            foreach (EPExpenseClaimDetails tranLine in claimEntry.ExpenseClaimDetails.Select())
            {
                foreach (var file in FileHelper.GetFiles(Base, tranLine.NoteID))
                {
                    pdfPacker.Combine(file, file);
                }
            }

            if (pdfPacker.Pack?.PageCount > 0)
            {
                byte[] pdfBytes = pdfPacker.SaveToMemory().ToArray();
                AttachFileToDocument(claimEntry, pdfBytes);
            }
        }

        private void AttachFileToDocument(ExpenseClaimEntry claimEntry, byte[] pdfBytes)
        {
            string fileName = FileHelper.CreateFilename("Expense Claim", claimEntry.ExpenseClaim.Current.RefNbr);

            var fileInfo = new PX.SM.FileInfo(fileName, null, pdfBytes);

            var fileMaint = PXGraph.CreateInstance<UploadFileMaintenance>();
            fileMaint.SaveFile(fileInfo);

            PXNoteAttribute.ForcePassThrow<EPExpenseClaim.noteID>(claimEntry.ExpenseClaim.Cache);
            PXNoteAttribute.AttachFile(
                claimEntry.ExpenseClaim.Cache,
                claimEntry.ExpenseClaim.Current,
                fileInfo
            );
        }

    }
}
