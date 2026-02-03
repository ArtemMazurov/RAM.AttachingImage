using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RAM.PdfPacker.Helper
{
    public static class FileHelper
    {
        public static string CreateFilename(string screen, string refNbr) => $"Attachments for {screen}: {refNbr}.pdf";

        public static IEnumerable<PXResult<UploadFile, UploadFileRevision>> GetFiles(PXGraph graph, Guid? noteID) =>
            SelectFrom<UploadFile>
                .InnerJoin<UploadFileRevision>
                    .On<UploadFile.fileID.IsEqual<UploadFileRevision.fileID>
                    .And<UploadFile.lastRevisionID.IsEqual<UploadFileRevision.fileRevisionID>>>
                .InnerJoin<NoteDoc>
                    .On<NoteDoc.fileID.IsEqual<UploadFile.fileID>>
                .Where<NoteDoc.noteID.IsEqual<P.AsGuid>>
                .View
                .Select(graph, noteID)
                .AsEnumerable()
                .Cast<PXResult<UploadFile, UploadFileRevision>>();
    }
}
