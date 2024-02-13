using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using AddOptimization.Utilities.Helpers;
using System;

namespace AddOptimization.Utilities.Common;

public class ExcelExporterBase
{
    private ICellStyle _boldStyle;
    private ICellStyle _h1Style;
    private ICellStyle _h2Style;
    private ICellStyle _h3CenterAligned;
    private IWorkbook _workbook;
    private ISheet _currentSheet;
    private ICellStyle _inactiveStyle;
    private ICellStyle _normalStyle;
    private ICellStyle _successStyle;
    private ICellStyle _warningStyle;
    private ICellStyle _italicStyle;

    public ISheet CurrentSheet
    {
        get
        {
            if (_currentSheet != null)
                return _currentSheet;

            _currentSheet = Workbook.CreateSheet("Data");
            return _currentSheet;
        }
        set { _currentSheet = value; }
    }

    public ISheet CreateSheet(string name)
    {
        var sheet = _workbook.GetSheet(name);
        if (sheet != null)
        {
            return sheet;
        }
        sheet = _workbook.CreateSheet(name);
        return sheet;
    }

    public void MergeCells(int startRow, int endRow, int startCol, int endCol)
    {
        _currentSheet.AddMergedRegion(new CellRangeAddress(startRow, endRow, startCol, endCol));
    }

    public IWorkbook Workbook
    {
        get { return _workbook ?? (_workbook = new XSSFWorkbook()); }
        set { _workbook = value; }
    }

    private ICellStyle HighlightedStyle
    {
        get
        {
            if (_boldStyle != null) return _boldStyle;

            _boldStyle = Workbook.CreateCellStyle();

            var font = Workbook.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "Calibri";
            font.Boldweight = (short)FontBoldWeight.Bold;
            _boldStyle.SetFont(font);
            return _boldStyle;
        }
    }

    private ICellStyle H3CenterText
    {
        get
        {
            if (_h3CenterAligned != null) return _h3CenterAligned;

            _h3CenterAligned = Workbook.CreateCellStyle();
            _h3CenterAligned.Alignment = HorizontalAlignment.Center;
            var font = Workbook.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "Calibri";
            font.Boldweight = (short)FontBoldWeight.Bold;
            _h3CenterAligned.SetFont(font);
            return _h3CenterAligned;
        }
    }

    private ICellStyle Italic
    {
        get
        {
            if (_italicStyle != null) return _italicStyle;

            _italicStyle = Workbook.CreateCellStyle();
            var font = Workbook.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "Calibri";
            font.Boldweight = (short)FontBoldWeight.Normal;
            font.IsItalic = true;
            _italicStyle.SetFont(font);
            return _italicStyle;
        }
    }

    private ICellStyle H1Style
    {
        get
        {
            if (_h1Style != null) return _h1Style;

            _h1Style = Workbook.CreateCellStyle();
            _h1Style.Alignment = HorizontalAlignment.Center;
            var font = Workbook.CreateFont();
            font.FontHeightInPoints = 22;
            font.FontName = "Calibri";
            font.Boldweight = (short)FontBoldWeight.Bold;
            _h1Style.SetFont(font);
            return _h1Style;
        }
    }

    private ICellStyle H2Style
    {
        get
        {
            if (_h2Style != null) return _h2Style;

            _h2Style = Workbook.CreateCellStyle();
            _h2Style.Alignment = HorizontalAlignment.Center;
            var font = Workbook.CreateFont();
            font.FontHeightInPoints = 15;
            font.FontName = "Calibri";
            font.Boldweight = (short)FontBoldWeight.Bold;
            _h2Style.SetFont(font);
            return _h2Style;
        }
    }

    private ICellStyle InactiveStyle
    {
        get
        {
            if (_inactiveStyle != null) return _inactiveStyle;

            _inactiveStyle = Workbook.CreateCellStyle();
            _inactiveStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
            _inactiveStyle.FillPattern = FillPattern.SolidForeground;
            var font = Workbook.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "Calibri";
            font.Boldweight = (short)FontBoldWeight.Bold;
            _inactiveStyle.SetFont(font);
            return _inactiveStyle;
        }
    }

    private ICellStyle NormalStyle
    {
        get
        {
            if (_normalStyle != null) return _normalStyle;

            _normalStyle = Workbook.CreateCellStyle();
            _normalStyle.Alignment = HorizontalAlignment.Left;
            var font = Workbook.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "Calibri";
            font.Boldweight = (short)FontBoldWeight.Normal;
            _normalStyle.SetFont(font);
            return _normalStyle;
        }
    }

    private ICellStyle SuccessStyle
    {
        get
        {
            if (_successStyle != null) return _successStyle;

            _successStyle = Workbook.CreateCellStyle();

            var font = Workbook.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "Calibri";
            font.Color = IndexedColors.Green.Index;
            _successStyle.SetFont(font);
            return _successStyle;
        }
    }

    private ICellStyle WarningStyle
    {
        get
        {
            if (_warningStyle != null) return _warningStyle;

            _warningStyle = Workbook.CreateCellStyle();

            var font = Workbook.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "Calibri";
            font.Color = IndexedColors.Orange.Index;

            _warningStyle.SetFont(font);
            return _warningStyle;
        }
    }

    protected enum Style
    {
        None,
        H3,
        H3CenterText,
        H1,
        H2,
        Error,
        InActive,
        Warning,
        Success,
        Italic
    }

    protected IRow DataRow(int no)
    {
        var row = CurrentSheet.GetRow(no);
        if (row != null)
            return row;
        row = CurrentSheet.CreateRow(no);
        row.RowStyle = HighlightedStyle;
        return row;
    }

    protected IRow HighlightedRow(int no)
    {
        var row = CurrentSheet.CreateRow(no);
        row.RowStyle = HighlightedStyle;
        return row;
    }

    private void SetStyle(ICell cell, Style highlight)
    {
        switch (highlight)
        {
            case Style.None:
                cell.CellStyle = NormalStyle;
                break;

            case Style.Error:
                cell.CellStyle = WarningStyle;
                break;

            case Style.H3:
                cell.CellStyle = HighlightedStyle;
                break;

            case Style.Success:
                cell.CellStyle = SuccessStyle;
                break;

            case Style.Warning:
                cell.CellStyle = WarningStyle;
                break;

            case Style.InActive:
                cell.CellStyle = InactiveStyle;
                break;

            case Style.H1:
                cell.CellStyle = H1Style;
                break;

            case Style.H2:
                cell.CellStyle = H2Style;
                break;

            case Style.H3CenterText:
                cell.CellStyle = H3CenterText;
                break;

            case Style.Italic:
                cell.CellStyle = Italic;
                break;
        }
    }

    protected void SetValue(IRow row, int colNo, long value, Style style = Style.None)
    {
        var cell = row.CreateCell(colNo);
        SetStyle(cell, style);
        cell.SetCellValue(value);
    }


    protected void SetValue(IRow row, int colNo, Style style = Style.None)
    {
        var cell = row.CreateCell(colNo);
        SetStyle(cell, style);
        cell.SetCellValue("");
    }

    protected void SetValue(IRow row, int colNo, DateTime? value,
        DateTimeHelper.DateStringType stringType = DateTimeHelper.DateStringType.DateOnly, Style style = Style.None)
    {
        var cell = row.CreateCell(colNo);
        SetStyle(cell, style);
        cell.SetCellValue(DateTimeHelper.ToString(value, stringType));
    }

    protected void SetValue(IRow row, int colNo, double value, Style style = Style.None)
    {
        var cell = row.CreateCell(colNo);
        SetStyle(cell, style);
        cell.SetCellValue(value);
    }

    protected void SetValue(IRow row, int colNo, decimal? value, Style style = Style.None)
    {
        var cell = row.CreateCell(colNo);
        SetStyle(cell, style);
        if (value == null)
            cell.SetCellValue("");
        else
            cell.SetCellValue(Convert.ToDouble(value));
    }

    private void SetValue(IRow row, int colNo, DateTime value, Style style = Style.None)
    {
        var cell = row.CreateCell(colNo);
        SetStyle(cell, style);
        cell.SetCellValue(value);
    }

    protected void SetValue(IRow row, int colNo, string value, Style style = Style.None)
    {
        var cell = row.GetCell(colNo) ?? row.CreateCell(colNo);
        SetStyle(cell, style);
        cell.SetCellValue(value);
    }
}
