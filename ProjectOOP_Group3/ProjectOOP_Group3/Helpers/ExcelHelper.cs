using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace ProjectOOP_Group3.Helpers
{
    public static class ExcelHelper
    {
        public static byte[] CreateExcelFile(List<Dictionary<string, object>> data, string title = "")
        {
            var sb = new StringBuilder();
            
            // XML Header cho Excel (SpreadsheetML format)
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
            sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
            sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
            
            // Styles
            sb.AppendLine("<Styles>");
            sb.AppendLine("<Style ss:ID=\"Title\">");
            sb.AppendLine("<Font ss:Bold=\"1\" ss:Size=\"16\"/>");
            sb.AppendLine("<Alignment ss:Horizontal=\"Center\"/>");
            sb.AppendLine("</Style>");
            sb.AppendLine("<Style ss:ID=\"Header\">");
            sb.AppendLine("<Font ss:Bold=\"1\"/>");
            sb.AppendLine("<Interior ss:Color=\"#CCCCCC\" ss:Pattern=\"Solid\"/>");
            sb.AppendLine("<Borders>");
            sb.AppendLine("<Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            sb.AppendLine("<Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            sb.AppendLine("<Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            sb.AppendLine("<Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            sb.AppendLine("</Borders>");
            sb.AppendLine("</Style>");
            sb.AppendLine("<Style ss:ID=\"Cell\">");
            sb.AppendLine("<Borders>");
            sb.AppendLine("<Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            sb.AppendLine("<Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            sb.AppendLine("<Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            sb.AppendLine("<Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            sb.AppendLine("</Borders>");
            sb.AppendLine("</Style>");
            sb.AppendLine("</Styles>");
            
            sb.AppendLine("<Worksheet ss:Name=\"Thống Kê\">");
            sb.AppendLine("<Table>");
            
            // Title row
            if (!string.IsNullOrEmpty(title))
            {
                sb.AppendLine("<Row>");
                sb.AppendLine($"<Cell ss:StyleID=\"Title\" ss:MergeAcross=\"1\"><Data ss:Type=\"String\">{HttpUtility.HtmlEncode(title)}</Data></Cell>");
                sb.AppendLine("</Row>");
            }
            
            // Data rows
            foreach (var row in data)
            {
                sb.AppendLine("<Row>");
                foreach (var kvp in row)
                {
                    string cellValue = kvp.Value?.ToString() ?? "";
                    string dataType = "String";
                    string styleId = "Cell";
                    
                    if (kvp.Value is int || kvp.Value is long || kvp.Value is decimal || kvp.Value is double || kvp.Value is float)
                    {
                        dataType = "Number";
                    }
                    else if (kvp.Value is DateTime)
                    {
                        dataType = "DateTime";
                        cellValue = ((DateTime)kvp.Value).ToString("yyyy-MM-ddTHH:mm:ss");
                    }
                    else if (kvp.Value is bool)
                    {
                        dataType = "Boolean";
                        cellValue = ((bool)kvp.Value) ? "1" : "0";
                    }
                    
                    // Check if this is a header row (all keys are the same as values or empty)
                    if (string.IsNullOrEmpty(cellValue) && !string.IsNullOrEmpty(kvp.Key))
                    {
                        styleId = "Header";
                    }
                    
                    sb.AppendLine($"<Cell ss:StyleID=\"{styleId}\"><Data ss:Type=\"{dataType}\">{HttpUtility.HtmlEncode(cellValue)}</Data></Cell>");
                }
                sb.AppendLine("</Row>");
            }
            
            sb.AppendLine("</Table>");
            sb.AppendLine("<WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">");
            sb.AppendLine("<PageSetup>");
            sb.AppendLine("<Header x:Margin=\"0.3\"/>");
            sb.AppendLine("<Footer x:Margin=\"0.3\"/>");
            sb.AppendLine("<PageMargins x:Bottom=\"0.75\" x:Left=\"0.7\" x:Right=\"0.7\" x:Top=\"0.75\"/>");
            sb.AppendLine("</PageSetup>");
            sb.AppendLine("</WorksheetOptions>");
            sb.AppendLine("</Worksheet>");
            sb.AppendLine("</Workbook>");
            
            // Add BOM for UTF-8
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var result = new byte[bom.Length + bytes.Length];
            Array.Copy(bom, 0, result, 0, bom.Length);
            Array.Copy(bytes, 0, result, bom.Length, bytes.Length);
            
            return result;
        }
    }
}

