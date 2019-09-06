using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;

namespace NPOIHelper
{
    public class NPOIExcel
    {
        private string _title;
        private string _sheetName;
        private string _filePath;

        /// <summary>
        /// 导出到Excel
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool ToExcel(DataTable table)
        {
            //if (File.Exists(_filePath))
            //{
            //    File.Delete(_filePath);
            //}
            //FileStream fs = new FileStream(this._filePath, FileMode.Open, FileAccess.Read);
            //fs.Position = 0;
            FileStream fs = File.OpenWrite(_filePath);
            IWorkbook workBook = null;
            //workBook = WorkbookFactory.Create(fs);
            if (_filePath.IndexOf(".xlsx") > 0) // 2007版本
                workBook = new XSSFWorkbook();
            else if (_filePath.IndexOf(".xls") > 0) // 2003版本
                workBook = new HSSFWorkbook();
            this._sheetName = this._sheetName == string.Empty ? "sheet1" : this._sheetName;
            ISheet sheet = workBook.CreateSheet(this._sheetName);

            IRow row;
            int start = 0;
            //处理表格标题
            if (_title.Length>1)
            {
                row = sheet.CreateRow(start);
                row.CreateCell(0).SetCellValue(this._title);
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, table.Columns.Count - 1));
                row.Height = 500;
                ICellStyle cellStyle = workBook.CreateCellStyle();
                IFont font = workBook.CreateFont();
                font.FontName = "微软雅黑";
                font.FontHeightInPoints = 17;
                cellStyle.SetFont(font);
                cellStyle.VerticalAlignment = VerticalAlignment.Center;
                cellStyle.Alignment = HorizontalAlignment.Center;
                row.Cells[0].CellStyle = cellStyle;
                start = 1;
            }

            //处理表格列头
            row = sheet.CreateRow(start);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                row.CreateCell(i).SetCellValue(table.Columns[i].ColumnName);
                row.Height = 350;
                sheet.AutoSizeColumn(i);
            }

            //处理数据内容
            for (int i = 0; i < table.Rows.Count; i++)
            {
                row = sheet.CreateRow(start+1 + i);
                row.Height = 250;
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(table.Rows[i][j].ToString());
                    sheet.SetColumnWidth(j, 256 * 15);
                }
            }

            //写入数据流
            workBook.Write(fs);
            //fs.Flush();
            fs.Close();

            return true;
        }

        /// <summary>
        /// 导出到Excel
        /// </summary>
        /// <param name="table"></param>
        /// <param name="title"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public bool ToExcel(DataTable table, string title, string sheetName, string filePath)
        {
            this._title = title;
            this._sheetName = sheetName;
            this._filePath = filePath;
            return ToExcel(table);
        }

        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(bool isFirstRowColumn, string sheetName, string fileName)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            int startRow = 0;
            //this._title = title;
            this._sheetName = sheetName;
            this._filePath = fileName;
            IWorkbook workbook = null;
            try
            {
                FileStream fs = new FileStream(this._filePath, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook(fs);
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(fs);

                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (sheet == null) //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {

                    IRow firstRow = sheet.GetRow(sheet.FirstRowNum);

                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }

                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        //for (int j = row.FirstCellNum; j < cellCount; ++j)
                        //{
                        //    if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                        //        dataRow[j] = row.GetCell(j).ToString();
                        //}
                        for (int e = 0; e < row.PhysicalNumberOfCells; e++)
                        {
                            dataRow[e] = row.Cells[e + 1].ToString().Replace("\t", "").Replace("\n", "");//从第2列开始 下标1
                        }
                        data.Rows.Add(dataRow);
                    }
                }
                fs.Close();
                fs.Dispose();
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }
        public DataTable ExcelToDataTable(string fileName)
        {


            ISheet sheet = null;
            DataTable data = new DataTable();
            data.Columns.Add("APPLIC_NO");
            data.Columns.Add("NAME");
            data.Columns.Add("DOB");
            data.Columns.Add("NATIONALITY");
            data.Columns.Add("PETITIONER");
            data.Columns.Add("NO_DEP");
            data.Columns.Add("VISA_TYPE");
            data.Columns.Add("VALIDITY");
            data.Columns.Add("HEARING_OFFICER");
            data.Columns.Add("FOLDER_NO");
            data.Columns.Add("FILENAME");
            data.Columns.Add("SHEETNAME");

            this._filePath = fileName;
            IWorkbook workbook = null;
            try
            {
                FileStream fs = new FileStream(this._filePath, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook(fs);
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(fs);
                for (int c = 0; c < workbook.NumberOfSheets; c++)
                {
                    sheet = workbook.GetSheetAt(c);
                    //从第一张表里面获取所有的列头
                    if (c==0)
                    {
                        for (int t_ = 0; t_ < 5; t_++)
                        {
                            IRow row = sheet.GetRow(t_);
                            bool b = row.Cells.Exists(x => x.StringCellValue.Contains("APPLIC_NO") && x.StringCellValue.Contains("NAME"));
                            if (b)
                            {
                                for (int t_c = 0; t_c < row.PhysicalNumberOfCells; t_c++)
                                {
                                    DataColumn col = new DataColumn(row.Cells[t_c].StringCellValue);
                                    data.Columns.Add(col);
                                }
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    int startrow = 0;
                    if (c == 0)
                        startrow = 3;

                    for (int d = startrow; d < sheet.PhysicalNumberOfRows; d++)
                    {
                        IRow row = sheet.GetRow(d);
                        DataRow dataRow = data.NewRow();
                        if (row.PhysicalNumberOfCells<11 || row.PhysicalNumberOfCells>12)
                        {
                            continue;
                        }
                        for (int e = 0; e < row.PhysicalNumberOfCells-2; e++)
                        {
                            dataRow[e] = row.Cells[e+1].ToString().Replace("\t", "").Replace("\n", "");//从第2列开始 下标1
                        }
                        dataRow[10] = fileName;
                        dataRow[11] = sheet.SheetName;
                        data.Rows.Add(dataRow);
                    }
                }

                fs.Close();
                fs.Dispose();
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }

        public DataTable ExcelToDataTable2(string fileName)
        {


            ISheet sheet = null;
            DataTable data = new DataTable();
            //data.Columns.Add("APPLIC_NO");
            //data.Columns.Add("NAME");
            //data.Columns.Add("DOB");
            //data.Columns.Add("NATIONALITY");
            //data.Columns.Add("PETITIONER");
            //data.Columns.Add("NO_DEP");
            //data.Columns.Add("VISA_TYPE");
            //data.Columns.Add("VALIDITY");
            //data.Columns.Add("HEARING_OFFICER");
            //data.Columns.Add("FOLDER_NO");
            //data.Columns.Add("FILENAME");
            //data.Columns.Add("SHEETNAME");

            //this._filePath = fileName;
            IWorkbook workbook = null;
            string docname = fileName.Substring(fileName.LastIndexOf("\\")+1);
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook(fs);
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(fs);

                sheet = workbook.GetSheetAt(0);
                //从第一张表里面获取所有的列头
                int tr_start = 0;
                for (int t_ = 0; t_ < 5; t_++)
                {
                    IRow row = sheet.GetRow(t_);

                    if (row.Cells.Exists(x => x.StringCellValue.Contains("APPLIC_NO")) && row.Cells.Exists(x => x.StringCellValue.Contains("NAME")))
                    {
                        for (int t_c = 0; t_c < row.PhysicalNumberOfCells; t_c++)
                        {
                            DataColumn col = new DataColumn(row.Cells[t_c].ToString().Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace(" ", "").Replace(".", "").Replace("_", "").Trim());
                            data.Columns.Add(col);
                            //row.Cells.ToArray()
                            //data.Columns.AddRange()
                        }
                        tr_start = t_;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                //加2列 记录信息
                data.Columns.Add("FILENAME");
                data.Columns.Add("SHEETNAME");
                //没有列
                if (!data.Columns.Contains("NODEP"))
                {
                    data.Columns.Add("NODEP");
                }

                for (int c = 0; c < workbook.NumberOfSheets; c++)
                {
                    sheet = workbook.GetSheetAt(c);

                    int startrow = 0;
                    if (c == 0)
                        startrow = tr_start+1;
                    bool bf = false;
                    for (int d = startrow; d < sheet.PhysicalNumberOfRows; d++)
                    {
                        IRow row = sheet.GetRow(d);
                        DataRow dataRow = data.NewRow();
                        //if (row.PhysicalNumberOfCells < 10 || row.PhysicalNumberOfCells > 11)
                        //{
                        //    continue;
                        //}
                        for (int e = 0; e < row.PhysicalNumberOfCells; e++)
                        {
                            if (row.Cells[e].IsMergedCell)
                            {
                                bf = true;
                                //continue;
                            }
                            dataRow[e] = row.Cells[e].ToString().Replace("\t", "").Replace("\n", "");

                        }
                        if (bf)
                        {
                            dataRow[row.PhysicalNumberOfCells-1] = docname;
                            dataRow[row.PhysicalNumberOfCells] = sheet.SheetName;
                        }
                        else
                        {
                            dataRow[row.PhysicalNumberOfCells] = docname;
                            dataRow[row.PhysicalNumberOfCells+1] = sheet.SheetName;
                        }

                        data.Rows.Add(dataRow);
                    }
                }

                fs.Close();
                fs.Dispose();
                return data;
            }
            catch (Exception ex)
            {
                string newfilepath = fileName.Replace("data", "data\\Err");
                File.Move(fileName,newfilepath);
                
                Console.WriteLine($"Exception:{ex.Message} {docname}-{sheet.SheetName}");

                return null;
            }
        }


        public DataTable ExcelToDataTable_hb(string fileName)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            IWorkbook workbook = null;
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook(fs);
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(fs);


                sheet = workbook.GetSheetAt(0);

                if (sheet != null)
                {
                    //组列
                    IRow firstRow = sheet.GetRow(sheet.FirstRowNum);

                    for (int e = 0; e < firstRow.PhysicalNumberOfCells; e++)
                    {
                        DataColumn column = new DataColumn();
                        data.Columns.Add(column);
                    }

                    //填充数据
                    for (int i = 0; i < sheet.PhysicalNumberOfRows; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        DataRow dataRow = data.NewRow();
                        for (int e = 0; e < row.PhysicalNumberOfCells; e++)
                        {
                            dataRow[e] = row.Cells[e].ToString().Replace("\t", "").Replace("\n", "");//从第2列开始 下标1
                        }
                        data.Rows.Add(dataRow);
                    }
                }
                fs.Close();
                fs.Dispose();
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }
    }
}
