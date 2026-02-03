using System;
using PdfSharp;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.PM;
using PX.SM;
using RAM.PdfPacker.Helper;

namespace RAM.PdfPacker.PM
{
    public class ProformaEntryExt : PXGraphExtension<ProformaEntry>
    {
        public static bool IsActive() => true;

        [PXOverride]
        public virtual void ReleaseDocument(PMProforma doc, Action<PMProforma> baseMethod)
        {
            using PXTransactionScope ts = new PXTransactionScope();

            ARInvoiceEntry invoiceEntry = null;
            PXGraph.InstanceCreated.AddHandler<ARInvoiceEntry>(graph => invoiceEntry = graph);

            baseMethod.Invoke(doc);

            if (invoiceEntry?.Document?.Current != null)
            {
                CreateCombinedPdf(invoiceEntry);
            }

            ts.Complete();
        }

        private void CreateCombinedPdf(ARInvoiceEntry invoiceEntry)
        {
            var pdfPacker = new PdfPacker(PageSize.Letter);
            foreach (PMProformaTransactLine tranLine in Base.TransactionLines.Select())
            {
                foreach (var file in FileHelper.GetFiles(Base, tranLine.NoteID))
                {
                    pdfPacker.Combine(file, file);
                }
            }

            if (pdfPacker.Pack?.PageCount > 0)
            {
                byte[] pdfBytes = pdfPacker.SaveToMemory().ToArray();
                AttachFileToInvoice(invoiceEntry, pdfBytes);
            }
        }

        private void AttachFileToInvoice(ARInvoiceEntry invoiceEntry, byte[] pdfBytes)
        {
            string fileName = CreateFilename("Pro Froma", Base.Document.Current.RefNbr);

            var fileInfo = new PX.SM.FileInfo(fileName, null, pdfBytes);

            var fileMaint = PXGraph.CreateInstance<UploadFileMaintenance>();
            fileMaint.SaveFile(fileInfo);

            PXNoteAttribute.ForcePassThrow<ARInvoice.noteID>(invoiceEntry.Document.Cache);
            PXNoteAttribute.AttachFile(
                invoiceEntry.Document.Cache,
                invoiceEntry.Document.Current,
                fileInfo
            );
        }

        private static string CreateFilename(string screen, string refNbr) => $"Attachments for {screen}: {refNbr}.pdf";
      
    }
}
