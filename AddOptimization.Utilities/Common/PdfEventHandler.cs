using iText.IO.Font.Constants;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Properties;
using System;

namespace AddOptimization.Utilities.Common;


public class PdfEventHandler : IEventHandler
{
    protected string _footer;
    private string _previousFooter;
    private int _nextAreaStartPage;
    protected Document doc;

    public PdfEventHandler(Document doc)
    {
        this.doc = doc;
    }
    public void SetFooter(string footer)
    {
        _previousFooter = _footer;
        _footer = footer;
    }
    public void SetNextAreaStartPage(int newValue)
    {
        _nextAreaStartPage = newValue;
    }
    public void HandleEvent(Event currentEvent)
    {
        PdfDocumentEvent documentEvent = (PdfDocumentEvent)currentEvent;
        PdfPage page = documentEvent.GetPage();
        Rectangle pageSize = page.GetPageSize();
        int pageNumber = documentEvent.GetDocument().GetPageNumber(page);
        PdfFont font = null;
        try
        {
            font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);
        }
        catch (Exception)
        {

        }
        float coordX = ((pageSize.GetLeft() + doc.GetLeftMargin())
                                + (pageSize.GetRight() - doc.GetRightMargin())) / 2;
        float footerY = 5;
        new Canvas(page, page.GetPageSize()).SetFont(font)
                    .SetFontSize(10)
            .ShowTextAligned(_nextAreaStartPage>pageNumber? _previousFooter:_footer, coordX, footerY, TextAlignment.CENTER)
            .Close();
    }
}
