using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace VisionImageTool
{
    /// <summary>
    /// Một ô trong bảng xuất Excel.
    ///   - Link != null      : ô là hyperlink (tới file / thư mục).
    ///   - ImageJpeg != null : chèn ảnh (JPEG) vừa khít ô, căn giữa; khi đó Link (nếu có) gắn lên ảnh -
    ///                         bấm vào ảnh để mở file - và Text chỉ dùng làm mô tả ảnh.
    /// </summary>
    internal sealed class XlsxCell
    {
        public string Text { get; set; }
        public string Link { get; set; }
        public bool Center { get; set; }

        public byte[] ImageJpeg { get; set; }
        public int ImageWidthPx { get; set; }
        public int ImageHeightPx { get; set; }

        /// <summary>Định dạng của ImageJpeg: "jpeg" (mặc định) hoặc "png".</summary>
        public string ImageExtension { get; set; }
    }

    /// <summary>
    /// Nhóm cột trên hàng tiêu đề thứ nhất (gộp ô), ví dụ "DK1" phủ 4 cột Raw | Mask | UnitX | Evaluation.
    /// Cột không thuộc nhóm nào thì tiêu đề của nó gộp dọc qua cả hai hàng tiêu đề.
    /// </summary>
    internal sealed class XlsxHeaderGroup
    {
        public string Text { get; set; }
        public int FirstColumn { get; set; }
        public int ColumnCount { get; set; }
    }

    /// <summary>
    /// Ghi file .xlsx (Office Open XML) tối giản, KHÔNG cần thư viện ngoài: một sheet, dòng tiêu đề
    /// in đậm + cố định + có bộ lọc, ô hyperlink thật (quan hệ External trong sheet1.xml.rels) nên
    /// bấm vào là mở ảnh / thư mục ngay trong Excel, và ảnh nhúng (xl/media + DrawingML).
    ///
    /// Dự án chưa có EPPlus/ClosedXML/NPOI; tự ghi vài file XML nén ZIP gọn hơn nhiều so với kéo thêm
    /// một gói NuGet chỉ để xuất một bảng.
    /// </summary>
    internal static class XlsxWriter
    {
        // Chỉ số style trong cellXfs của styles.xml bên dưới.
        private const int StyleDefault = 0;
        private const int StyleHeader = 1;
        private const int StyleLink = 2;
        private const int StyleCenter = 3;
        private const int StyleLabelO = 4;
        private const int StyleLabelDelta = 5;
        private const int StyleLabelX = 6;
        private const int StyleGroupHeader = 7;

        /// <summary>Ô căn giữa: nhãn O / Δ / X được tô màu (xanh / cam / đỏ) giống bảng trong phần mềm.</summary>
        private static int LabelStyle(string text)
        {
            switch (text)
            {
                case "O": return StyleLabelO;
                case "Δ":
                case "▲": return StyleLabelDelta;
                case "X": return StyleLabelX;
                default: return StyleCenter;
            }
        }

        /// <summary>1 pixel = 9525 EMU (đơn vị toạ độ của DrawingML).</summary>
        private const long EmuPerPixel = 9525;

        /// <summary>
        /// Độ rộng cột Excel (đơn vị "ký tự" của font Calibri 11) để cột rộng đúng <paramref name="px"/>
        /// pixel: Excel hiển thị cột rộng = width * 7 + 5 px.
        /// </summary>
        public static double ColumnWidthForPixels(int px)
        {
            return Math.Max(1.0, Math.Round((px - 5) / 7.0, 2));
        }

        private static int PixelsForColumnWidth(double width)
        {
            return (int)Math.Round(width * 7 + 5);
        }

        /// <param name="columnWidths">Độ rộng từng cột (đơn vị ký tự - xem ColumnWidthForPixels).</param>
        /// <param name="dataRowHeightPx">Chiều cao các hàng dữ liệu (px), 0 = mặc định của Excel.</param>
        /// <param name="headerGroups">Nhóm cột (hàng tiêu đề thứ nhất); null/rỗng = tiêu đề một hàng.</param>
        /// <param name="freezeColumns">Số cột bên trái cố định khi cuộn ngang.</param>
        public static void Write(string path, string sheetName, IList<string> headers,
            IList<XlsxCell[]> rows, IList<double> columnWidths, int dataRowHeightPx = 0,
            bool doNotCompressImages = false, IList<XlsxHeaderGroup> headerGroups = null, int freezeColumns = 0)
        {
            bool grouped = headerGroups != null && headerGroups.Count > 0;
            int headerRows = grouped ? 2 : 1;
            var groupOf = new XlsxHeaderGroup[headers.Count];
            if (grouped)
            {
                foreach (var group in headerGroups)
                {
                    for (int c = group.FirstColumn; c < group.FirstColumn + group.ColumnCount && c < headers.Count; c++)
                    {
                        groupOf[c] = group;
                    }
                }
            }

            var links = new List<KeyValuePair<string, string>>();    // cellRef -> target (hyperlink ô)
            var pictures = new List<Picture>();

            // Hyperlink tính TƯƠNG ĐỐI theo thư mục chứa file Excel (xem ToLinkTarget).
            string linkBase = Path.GetDirectoryName(Path.GetFullPath(path));

            var sheet = new StringBuilder();
            sheet.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sheet.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" ")
                 .Append("xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">");

            sheet.Append("<sheetViews><sheetView workbookViewId=\"0\"><pane ");
            if (freezeColumns > 0)
            {
                sheet.Append("xSplit=\"").Append(freezeColumns).Append("\" ");
            }
            sheet.Append("ySplit=\"").Append(headerRows).Append("\" topLeftCell=\"")
                 .Append(CellRef(Math.Max(0, freezeColumns), headerRows + 1))
                 .Append("\" activePane=\"").Append(freezeColumns > 0 ? "bottomRight" : "bottomLeft")
                 .Append("\" state=\"frozen\"/></sheetView></sheetViews>");

            if (columnWidths != null && columnWidths.Count > 0)
            {
                sheet.Append("<cols>");
                for (int c = 0; c < columnWidths.Count; c++)
                {
                    sheet.AppendFormat(CultureInfo.InvariantCulture,
                        "<col min=\"{0}\" max=\"{0}\" width=\"{1}\" customWidth=\"1\"/>", c + 1, columnWidths[c]);
                }
                sheet.Append("</cols>");
            }

            sheet.Append("<sheetData>");

            var merges = new List<string>();

            sheet.Append("<row r=\"1\">");
            for (int c = 0; c < headers.Count; c++)
            {
                var group = groupOf[c];
                if (group == null)
                {
                    AppendCell(sheet, CellRef(c, 1), headers[c], StyleHeader);
                    if (grouped)
                    {
                        merges.Add(CellRef(c, 1) + ":" + CellRef(c, 2));
                    }
                }
                else
                {
                    // Ô bị gộp vẫn ghi (rỗng) để có viền + nền giống ô đầu nhóm.
                    AppendCell(sheet, CellRef(c, 1), c == group.FirstColumn ? group.Text : string.Empty, StyleGroupHeader);
                    if (c == group.FirstColumn && group.ColumnCount > 1)
                    {
                        merges.Add(CellRef(c, 1) + ":" + CellRef(c + group.ColumnCount - 1, 1));
                    }
                }
            }
            sheet.Append("</row>");

            if (grouped)
            {
                sheet.Append("<row r=\"2\">");
                for (int c = 0; c < headers.Count; c++)
                {
                    AppendCell(sheet, CellRef(c, 2), groupOf[c] == null ? string.Empty : headers[c], StyleHeader);
                }
                sheet.Append("</row>");
            }

            for (int r = 0; r < rows.Count; r++)
            {
                int rowNumber = r + headerRows + 1;
                sheet.Append("<row r=\"").Append(rowNumber).Append("\"");
                if (dataRowHeightPx > 0)
                {
                    // Chiều cao hàng tính bằng point: 1 px = 0.75 pt ở 96 DPI.
                    sheet.AppendFormat(CultureInfo.InvariantCulture, " ht=\"{0}\" customHeight=\"1\"", dataRowHeightPx * 0.75);
                }
                sheet.Append(">");

                var cells = rows[r];
                for (int c = 0; c < cells.Length; c++)
                {
                    var cell = cells[c];
                    if (cell == null)
                    {
                        continue;
                    }

                    string cellRef = CellRef(c, rowNumber);

                    if (cell.ImageJpeg != null && cell.ImageJpeg.Length > 0)
                    {
                        int cellWidthPx = columnWidths != null && c < columnWidths.Count
                            ? PixelsForColumnWidth(columnWidths[c])
                            : cell.ImageWidthPx;
                        int cellHeightPx = dataRowHeightPx > 0 ? dataRowHeightPx : cell.ImageHeightPx;

                        pictures.Add(new Picture
                        {
                            Column = c,
                            Row = rowNumber - 1,
                            Jpeg = cell.ImageJpeg,
                            Extension = cell.ImageExtension == "png" ? "png" : "jpeg",
                            Width = cell.ImageWidthPx,
                            Height = cell.ImageHeightPx,
                            OffsetX = Math.Max(0, (cellWidthPx - cell.ImageWidthPx) / 2),
                            OffsetY = Math.Max(0, (cellHeightPx - cell.ImageHeightPx) / 2),
                            Link = string.IsNullOrEmpty(cell.Link) ? null : ToLinkTarget(cell.Link, linkBase),
                            Name = cell.Text
                        });
                        continue;
                    }

                    if (string.IsNullOrEmpty(cell.Text))
                    {
                        continue;
                    }

                    int style = StyleDefault;

                    if (!string.IsNullOrEmpty(cell.Link))
                    {
                        style = StyleLink;
                        links.Add(new KeyValuePair<string, string>(cellRef, ToLinkTarget(cell.Link, linkBase)));
                    }
                    else if (cell.Center)
                    {
                        style = LabelStyle(cell.Text);
                    }

                    AppendCell(sheet, cellRef, cell.Text, style);
                }

                sheet.Append("</row>");
            }

            sheet.Append("</sheetData>");

            string lastCell = CellRef(Math.Max(0, headers.Count - 1), rows.Count + headerRows);
            sheet.Append("<autoFilter ref=\"A").Append(headerRows).Append(":").Append(lastCell).Append("\"/>");

            if (merges.Count > 0)
            {
                sheet.Append("<mergeCells count=\"").Append(merges.Count).Append("\">");
                foreach (var merge in merges)
                {
                    sheet.Append("<mergeCell ref=\"").Append(merge).Append("\"/>");
                }
                sheet.Append("</mergeCells>");
            }

            if (links.Count > 0)
            {
                sheet.Append("<hyperlinks>");
                for (int i = 0; i < links.Count; i++)
                {
                    sheet.Append("<hyperlink ref=\"").Append(links[i].Key).Append("\" r:id=\"rId")
                         .Append(i + 1).Append("\"/>");
                }
                sheet.Append("</hyperlinks>");
            }

            sheet.Append("<pageMargins left=\"0.7\" right=\"0.7\" top=\"0.75\" bottom=\"0.75\" header=\"0.3\" footer=\"0.3\"/>");

            // Quan hệ tới drawing đứng SAU các hyperlink (rId1..rIdN) trong sheet1.xml.rels.
            string drawingRelId = "rId" + (links.Count + 1);
            if (pictures.Count > 0)
            {
                sheet.Append("<drawing r:id=\"").Append(drawingRelId).Append("\"/>");
            }

            sheet.Append("</worksheet>");

            var sheetRels = new StringBuilder();
            sheetRels.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sheetRels.Append("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
            for (int i = 0; i < links.Count; i++)
            {
                sheetRels.Append("<Relationship Id=\"rId").Append(i + 1)
                         .Append("\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink\" Target=\"")
                         .Append(Escape(links[i].Value)).Append("\" TargetMode=\"External\"/>");
            }
            if (pictures.Count > 0)
            {
                sheetRels.Append("<Relationship Id=\"").Append(drawingRelId)
                         .Append("\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing\" Target=\"../drawings/drawing1.xml\"/>");
            }
            sheetRels.Append("</Relationships>");

            // Ghi ra file tạm rồi mới thay file đích: Excel đang mở file cũ thì báo lỗi rõ ràng thay
            // vì để lại một file hỏng dở dang.
            string temp = path + ".tmp";
            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                AddEntry(zip, "[Content_Types].xml", pictures.Count > 0 ? ContentTypesWithDrawing : ContentTypes);
                AddEntry(zip, "_rels/.rels", RootRels);
                AddEntry(zip, "xl/workbook.xml", Workbook(sheetName, doNotCompressImages));
                AddEntry(zip, "xl/_rels/workbook.xml.rels", WorkbookRels);
                AddEntry(zip, "xl/styles.xml", Styles);
                AddEntry(zip, "xl/worksheets/sheet1.xml", sheet.ToString());
                AddEntry(zip, "xl/worksheets/_rels/sheet1.xml.rels", sheetRels.ToString());

                if (pictures.Count > 0)
                {
                    WriteDrawing(zip, pictures);
                }
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Move(temp, path);
        }

        private sealed class Picture
        {
            public int Column;
            public int Row;          // 0-based (hàng tiêu đề = 0)
            public byte[] Jpeg;
            public string Extension;
            public int Width;
            public int Height;
            public int OffsetX;
            public int OffsetY;
            public string Link;
            public string Name;
        }

        /// <summary>
        /// xl/drawings/drawing1.xml + quan hệ + xl/media/imageN.jpeg. Mỗi ảnh là một twoCellAnchor nằm
        /// gọn trong MỘT ô (from/to cùng ô) nên ảnh đi theo ô và bị ẩn cùng hàng khi dùng bộ lọc.
        /// </summary>
        private static void WriteDrawing(ZipArchive zip, IList<Picture> pictures)
        {
            var drawing = new StringBuilder();
            var rels = new StringBuilder();

            drawing.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            drawing.Append("<xdr:wsDr xmlns:xdr=\"http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing\" ")
                   .Append("xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" ")
                   .Append("xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">");

            rels.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            rels.Append("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");

            int relId = 0;

            for (int i = 0; i < pictures.Count; i++)
            {
                var p = pictures[i];
                string imageRel = "rId" + (++relId);
                string mediaName = "image" + (i + 1) + "." + p.Extension;

                rels.Append("<Relationship Id=\"").Append(imageRel)
                    .Append("\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/image\" Target=\"../media/")
                    .Append(mediaName).Append("\"/>");

                string linkRel = null;
                if (p.Link != null)
                {
                    linkRel = "rId" + (++relId);
                    rels.Append("<Relationship Id=\"").Append(linkRel)
                        .Append("\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink\" Target=\"")
                        .Append(Escape(p.Link)).Append("\" TargetMode=\"External\"/>");
                }

                long x = p.OffsetX * EmuPerPixel;
                long y = p.OffsetY * EmuPerPixel;
                long cx = p.Width * EmuPerPixel;
                long cy = p.Height * EmuPerPixel;

                drawing.Append("<xdr:twoCellAnchor editAs=\"twoCell\">");
                drawing.Append("<xdr:from><xdr:col>").Append(p.Column).Append("</xdr:col><xdr:colOff>").Append(x)
                       .Append("</xdr:colOff><xdr:row>").Append(p.Row).Append("</xdr:row><xdr:rowOff>").Append(y)
                       .Append("</xdr:rowOff></xdr:from>");
                drawing.Append("<xdr:to><xdr:col>").Append(p.Column).Append("</xdr:col><xdr:colOff>").Append(x + cx)
                       .Append("</xdr:colOff><xdr:row>").Append(p.Row).Append("</xdr:row><xdr:rowOff>").Append(y + cy)
                       .Append("</xdr:rowOff></xdr:to>");
                drawing.Append("<xdr:pic><xdr:nvPicPr><xdr:cNvPr id=\"").Append(i + 2).Append("\" name=\"Picture ").Append(i + 1)
                       .Append("\" descr=\"").Append(Escape(p.Name ?? string.Empty)).Append("\"");
                if (linkRel != null)
                {
                    drawing.Append("><a:hlinkClick r:id=\"").Append(linkRel).Append("\"/></xdr:cNvPr>");
                }
                else
                {
                    drawing.Append("/>");
                }
                drawing.Append("<xdr:cNvPicPr><a:picLocks noChangeAspect=\"1\"/></xdr:cNvPicPr></xdr:nvPicPr>");
                drawing.Append("<xdr:blipFill><a:blip r:embed=\"").Append(imageRel).Append("\"/><a:stretch><a:fillRect/></a:stretch></xdr:blipFill>");
                drawing.Append("<xdr:spPr><a:xfrm><a:off x=\"0\" y=\"0\"/><a:ext cx=\"").Append(cx).Append("\" cy=\"").Append(cy)
                       .Append("\"/></a:xfrm><a:prstGeom prst=\"rect\"><a:avLst/></a:prstGeom></xdr:spPr>");
                drawing.Append("</xdr:pic><xdr:clientData/></xdr:twoCellAnchor>");

                // JPEG đã nén sẵn - nén thêm lần nữa chỉ tốn thời gian.
                var media = zip.CreateEntry("xl/media/" + mediaName, CompressionLevel.NoCompression);
                using (var s = media.Open())
                {
                    s.Write(p.Jpeg, 0, p.Jpeg.Length);
                }
            }

            drawing.Append("</xdr:wsDr>");
            rels.Append("</Relationships>");

            AddEntry(zip, "xl/drawings/drawing1.xml", drawing.ToString());
            AddEntry(zip, "xl/drawings/_rels/drawing1.xml.rels", rels.ToString());
        }

        private static void AppendCell(StringBuilder sb, string cellRef, string text, int style)
        {
            sb.Append("<c r=\"").Append(cellRef).Append("\" t=\"inlineStr\"");
            if (style != StyleDefault)
            {
                sb.Append(" s=\"").Append(style).Append("\"");
            }
            sb.Append("><is><t xml:space=\"preserve\">").Append(Escape(text)).Append("</t></is></c>");
        }

        /// <summary>Chỉ số cột (0-based) + số dòng -> "A1", "AB12"...</summary>
        private static string CellRef(int column, int row)
        {
            string letters = string.Empty;
            int n = column + 1;
            while (n > 0)
            {
                int rem = (n - 1) % 26;
                letters = (char)('A' + rem) + letters;
                n = (n - 1) / 26;
            }

            return letters + row;
        }

        /// <summary>
        /// Đích của hyperlink: đường dẫn TƯƠNG ĐỐI tính từ thư mục chứa file Excel, vd
        /// "raw/MODEL1_DK1_VITRI1_OK.jpg" hay "../mask/a%20b.jpg". Excel giải link tương đối theo vị trí
        /// của chính file .xlsx, nên copy / đổi tên / đổi ổ cả thư mục dataset (kèm file Excel) sang máy
        /// khác thì link vẫn mở được. Link tuyệt đối "file:///E:/..." chỉ đúng trên máy đã xuất.
        /// Ảnh khác ổ đĩa với file Excel thì không có đường tương đối - đành dùng link tuyệt đối.
        /// </summary>
        public static string ToLinkTarget(string file, string baseDirectory)
        {
            try
            {
                string fullFile = Path.GetFullPath(file);
                var target = new Uri(fullFile);

                // Khác ổ đĩa (hoặc khác máy chia sẻ UNC) thì không có đường tương đối. KHÔNG dựa vào
                // MakeRelativeUri cho trường hợp này: nó trả về "E:/TEST/..." thiếu "file:///" - Excel
                // không mở được.
                if (!string.IsNullOrEmpty(baseDirectory)
                    && string.Equals(Path.GetPathRoot(fullFile), Path.GetPathRoot(Path.GetFullPath(baseDirectory)),
                        StringComparison.OrdinalIgnoreCase))
                {
                    string dir = Path.GetFullPath(baseDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    var relative = new Uri(dir).MakeRelativeUri(target);

                    if (!relative.IsAbsoluteUri)
                    {
                        return EncodeLink(Uri.UnescapeDataString(relative.OriginalString));
                    }
                }

                return "file:///" + EncodeLink(fullFile.Replace('\\', '/'));
            }
            catch
            {
                return file;
            }
        }

        /// <summary>
        /// Mã hoá TỐI THIỂU cho đích hyperlink: chỉ dấu cách, %, # (giống Excel tự ghi). KHÔNG mã hoá ký tự
        /// Unicode (tiếng Việt): Excel giải "%E1%BA%B7" theo bảng mã ANSI chứ không phải UTF-8 -> sai tên
        /// file, link không mở được (lỗi 2026-09-30 với tên file có dấu).
        /// </summary>
        private static string EncodeLink(string path)
        {
            return path.Replace("%", "%25").Replace(" ", "%20").Replace("#", "%23");
        }

        /// <summary>Escape XML + bỏ ký tự điều khiển không hợp lệ trong XML 1.0 (Excel từ chối cả file).</summary>
        private static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                switch (c)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '"': sb.Append("&quot;"); break;
                    default:
                        if (c >= 0x20 || c == '\t' || c == '\n' || c == '\r')
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }

            return sb.ToString();
        }

        private static void AddEntry(ZipArchive zip, string name, string content)
        {
            var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
            using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        /// <param name="doNotCompressImages">
        /// Bật "Do not compress images in file" (File > Options > Advanced) = workbookPr autoCompressPictures="0":
        /// Excel giữ nguyên độ phân giải ảnh nhúng khi lưu lại file, không tự nén xuống 220 ppi.
        /// </param>
        private static string Workbook(string sheetName, bool doNotCompressImages)
        {
            string name = string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName;
            foreach (char c in new[] { '\\', '/', '?', '*', '[', ']', ':' })
            {
                name = name.Replace(c, '_');
            }

            if (name.Length > 31)
            {
                name = name.Substring(0, 31);
            }

            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                + "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" "
                + "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">"
                // Ghi RÕ cả hai trường hợp: "0" = bật "Do not compress images in file" (ảnh gốc),
                // "1" = tắt - Excel được phép nén ảnh (chế độ thumbnail hyperlink, dataset lớn).
                + "<workbookPr autoCompressPictures=\"" + (doNotCompressImages ? "0" : "1") + "\"/>"
                + "<sheets><sheet name=\"" + Escape(name) + "\" sheetId=\"1\" r:id=\"rId1\"/></sheets>"
                + "</workbook>";
        }

        private const string ContentTypes =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
            + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
            + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
            + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
            + "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
            + "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>"
            + "</Types>";

        private const string ContentTypesWithDrawing =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
            + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
            + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
            + "<Default Extension=\"jpeg\" ContentType=\"image/jpeg\"/>"
            + "<Default Extension=\"png\" ContentType=\"image/png\"/>"
            + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
            + "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
            + "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>"
            + "<Override PartName=\"/xl/drawings/drawing1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.drawing+xml\"/>"
            + "</Types>";

        private const string RootRels =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
            + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>"
            + "</Relationships>";

        private const string WorkbookRels =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
            + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>"
            + "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>"
            + "</Relationships>";

        // fonts: 0 thường, 1 đậm, 2 xanh gạch chân (hyperlink), 3/4/5 đậm xanh lá / cam / đỏ (O / Δ / X)
        // fills: 0 none, 1 gray125 (bắt buộc), 2 nền xám nhạt cho tiêu đề, 3/4/5 nền nhạt O / Δ / X, 6 xanh nhạt nhóm DK
        // cellXfs: 0 mặc định, 1 tiêu đề, 2 hyperlink, 3 căn giữa, 4/5/6 nhãn O / Δ / X, 7 tiêu đề nhóm (DK)
        private const string Styles =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            + "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
            + "<fonts count=\"6\">"
            + "<font><sz val=\"11\"/><name val=\"Calibri\"/><family val=\"2\"/></font>"
            + "<font><b/><sz val=\"11\"/><name val=\"Calibri\"/><family val=\"2\"/></font>"
            + "<font><u/><sz val=\"11\"/><color rgb=\"FF0563C1\"/><name val=\"Calibri\"/><family val=\"2\"/></font>"
            + "<font><b/><sz val=\"11\"/><color rgb=\"FF15803D\"/><name val=\"Calibri\"/><family val=\"2\"/></font>"
            + "<font><b/><sz val=\"11\"/><color rgb=\"FFB45309\"/><name val=\"Calibri\"/><family val=\"2\"/></font>"
            + "<font><b/><sz val=\"11\"/><color rgb=\"FFB91C1C\"/><name val=\"Calibri\"/><family val=\"2\"/></font>"
            + "</fonts>"
            + "<fills count=\"7\">"
            + "<fill><patternFill patternType=\"none\"/></fill>"
            + "<fill><patternFill patternType=\"gray125\"/></fill>"
            + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFE5E7EB\"/><bgColor indexed=\"64\"/></patternFill></fill>"
            + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFDCFCE7\"/><bgColor indexed=\"64\"/></patternFill></fill>"
            + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFFEF3C7\"/><bgColor indexed=\"64\"/></patternFill></fill>"
            + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFFEE2E2\"/><bgColor indexed=\"64\"/></patternFill></fill>"
            + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFDBEAFE\"/><bgColor indexed=\"64\"/></patternFill></fill>"
            + "</fills>"
            + "<borders count=\"2\">"
            + "<border><left/><right/><top/><bottom/><diagonal/></border>"
            + "<border><left style=\"thin\"><color rgb=\"FFBFC5CE\"/></left><right style=\"thin\"><color rgb=\"FFBFC5CE\"/></right>"
            + "<top style=\"thin\"><color rgb=\"FFBFC5CE\"/></top><bottom style=\"thin\"><color rgb=\"FFBFC5CE\"/></bottom><diagonal/></border>"
            + "</borders>"
            + "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>"
            + "<cellXfs count=\"8\">"
            + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"1\" xfId=\"0\" applyBorder=\"1\" applyAlignment=\"1\"><alignment vertical=\"center\"/></xf>"
            + "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>"
            + "<xf numFmtId=\"0\" fontId=\"2\" fillId=\"0\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment vertical=\"center\"/></xf>"
            + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"1\" xfId=\"0\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>"
            + "<xf numFmtId=\"0\" fontId=\"3\" fillId=\"3\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>"
            + "<xf numFmtId=\"0\" fontId=\"4\" fillId=\"4\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>"
            + "<xf numFmtId=\"0\" fontId=\"5\" fillId=\"5\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>"
            + "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"6\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>"
            + "</cellXfs>"
            + "<cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles>"
            + "</styleSheet>";
    }
}
