using System;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text;


public class CfgExportor
{
    private static int Rgb2Int(ushort r, ushort g, ushort b)
    {
        return r << 16 | g << 8 | b;
    }
    public enum FieldRule { RULE_ERROR = 0, RULE_COMMON, RULE_SERVER, RULE_CLIENT, RULE_IGNORE, RULE_FINISH, RULE_CONTENT }
    private static Dictionary<int, FieldRule> color_rule = new Dictionary<int, FieldRule>()
        {
            {Rgb2Int(  0, 128,   0), FieldRule.RULE_COMMON},
            {Rgb2Int(255, 204,   0), FieldRule.RULE_SERVER},
            {Rgb2Int(  0, 204, 255), FieldRule.RULE_CLIENT},
            {Rgb2Int(150, 150, 150), FieldRule.RULE_IGNORE},
            {Rgb2Int(  0,  51, 102), FieldRule.RULE_FINISH},
            {Rgb2Int(  0,   0,   0), FieldRule.RULE_CONTENT},
        };
    public FieldRule GetColorRule(int color)
    {
        try
        {
            return color_rule[color];
        }
        catch (Exception e)
        {
            return FieldRule.RULE_ERROR;
        }
    }
    public FieldRule GetColorRule(byte[] rgb)
    {
        if (rgb.Length < 3)
            return FieldRule.RULE_ERROR;
        return this.GetColorRule(Rgb2Int(rgb[0], rgb[1], rgb[2]));
    }
    struct HeadeCol
    {
        public int index;
        public FieldRule rule;
        public string name;
        public string type;
        //public HeadeCol() { index = 0; rule = FieldRule.RULE_ERROR; name = ""; }
    }
    struct RecordCol
    {
        public int index;
        public object val;
        public CellType type;
        //public RecordCol() { index = 0; val = null; type = CellType.Error; }
    }

    private string filename;
    private string exportname;
    private IWorkbook workbook;
    private ISheet sheet;
    private List<HeadeCol> header;
    private List<Dictionary<string, RecordCol>> records;

    public CfgExportor(string filename)
    {
        this.header = new List<HeadeCol>();
        this.records = new List<Dictionary<string, RecordCol>>();
        this.filename = filename;
        this.exportname = Path.GetFileNameWithoutExtension(filename);
    }

    private string RemoveBlank(string str)
    {
        return str.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
    }
    private bool IsTable(string str)
    {
        str = RemoveBlank(str);
        if (!str.StartsWith("{") || !str.EndsWith("}"))
            return false;
        return true;
    }

    private string PreprocessTable(string str)
    {
        int lbrace_cnt = Regex.Matches(str, @"{").Count;
        int rbrace_cnt = Regex.Matches(str, @"}").Count;

        if (lbrace_cnt != rbrace_cnt)
        {
            Console.WriteLine("大括号数目不匹配。\r\n" + str);
            throw new Exception("大括号数目不匹配。\r\n" + str);
        }
        str = RemoveBlank(str).Replace(",}", "}");
        if (!str.StartsWith("{") || !str.EndsWith("}"))
        {
            Console.WriteLine("Error!!! Lua table 必须使用{} <<<\r\n" + str);
            throw new Exception("Error!!! Lua table 必须使用{} <<<\r\n" + str);
        }
        if (lbrace_cnt == 1)
            return str.Substring(1, str.Length - 2);
        return null;
    }

    private string Table2String(string str)
    {
        string tmp_str = PreprocessTable(str);
        if (tmp_str != null)
            return tmp_str.Replace(",", ";");

        tmp_str = str.Substring(1, str.Length - 2);
        string[] str_arr = Regex.Split(tmp_str, @"},{");
        if (str_arr.Length == 1)
        {
            string s = str_arr[0].Substring(1, str_arr[0].Length - 2).Replace("{", "").Replace("}", "").Replace(",", "#");
            return s;
        }
        else
        {
            string final_s = "";
            foreach (string s in str_arr)
            {
                final_s += s.Replace("{", "").Replace("}", "").Replace(",", "#") + ';';
            }
            return final_s.Substring(0, final_s.Length - 1);
        }
    }
    private string QuoteStr(string str)
    {
        if (str.Length == 0)
            return "\"\"";
        double res;
        if (double.TryParse(str, out res))
            return str;
        if (!str.StartsWith("\""))
            str = '"' + str;
        if (!str.EndsWith("\""))
            str = str + '"';
        return str;
    }

    private string ConvertArray(string str, bool is_lua)
    {
        string tmp = "";
        string curstr = "";
        bool in_str = false;
        bool next_escape = false;
        for (int i = 0; i < str.Length; i++)
        {
            char ch = str[i];
            if (ch == '"')
            {   //# 只能是转义或某一个元素的最开始及结尾
                if (!next_escape)
                {
                    if (!(i == 0 || str[i - 1] == ',' || ((i == str.Length - 1 || str[i + 1] == ',') && in_str)))
                    {
                        Console.WriteLine("Convert array error! {0} {1} {2} {3} {4}", str, i, ch, next_escape, in_str);
                        return tmp;
                    }
                    else
                        in_str = !in_str;
                }
                curstr += ch;
            }
            else if (ch == ',')
            {
                if (in_str == true)
                    curstr += ch;
                else if (is_lua && !(curstr.StartsWith("\"") && curstr.EndsWith("\"")))
                    tmp += QuoteStr(curstr) + ',';
                else
                    tmp += curstr + ',';
                curstr = "";
                in_str = false;
                next_escape = false;
            }
            else
                curstr += ch;
            if (ch == '\\')
                /*if not in_str:  #转义必须在双引号包含的字符串内
                #    print("Convert array error!!! %s %d %c" % (str, i, ch))
                #    return string
                 */
                next_escape = !next_escape;
            else
                next_escape = false;

            if (i == str.Length - 1)
            {
                if (in_str)
                {
                    Console.WriteLine("Convert array error!! {0} {1} {2} {3} {4}", str, i, ch, next_escape, in_str);
                    return tmp;
                }
                else if (is_lua && !(curstr.Length > 0 && curstr.StartsWith("\"") && curstr.EndsWith("\"")))
                    tmp += QuoteStr(curstr) + ',';
                else
                    tmp += curstr + ',';
            }
        }
        if (tmp.Length < 1)
            return "{}";
        return "{" + tmp.Substring(0, tmp.Length - 1) + "}";
    }

    private string ConvertLuaTable(string str)
    {
        string tmp_str = PreprocessTable(str);
        if (tmp_str != null)
            return ConvertArray(tmp_str, true);

        tmp_str = str.Substring(1, str.Length - 2);
        string[] str_arr = Regex.Split(tmp_str, @"},{");
        if (str_arr.Length == 1)
        {
            string s = str_arr[0].Substring(1, str_arr[0].Length - 2).Replace("{", "").Replace("}", "");
            return "{" + ConvertArray(s, true) + "}";
        }
        else
        {
            string final_s = "";
            foreach (string s in str_arr)
            {
                string tmp = s.Replace("{", "").Replace("}", "");
                final_s += ConvertArray(tmp, true) + ',';
            }
            //#return '"' + final_s[:-1] + '"'
            return "{" + final_s.Substring(0, final_s.Length - 1) + "}";
        }
    }

    private string Float2String(double val)
    {
        double diff = val - (int)val;
        int tmp = (int)(diff * 1000);
        if (tmp > 0)
            return val.ToString();
        else
            return ((int)val).ToString();
    }

    private bool IsSkipRow(IRow row)
    {
        if (row == null)
            return true;
        ICell cell = row.GetCell(0);
        CellType type;
        object obj = GetValueType(cell, out type);
        if (obj == null)
            return true;
        string str = obj.ToString();
        if (str == "" || str.StartsWith("//"))
            return true;
        IColor color = cell.CellStyle.FillForegroundColorColor;
        if (color == null)
            return false;
        FieldRule rule = GetColorRule(color.RGB);

        if (rule == FieldRule.RULE_ERROR)
            return true;
        return false;
    }
    private bool IsFinishRow(IRow row)
    {
        if (row == null)
            return false;
        ICell cell = row.GetCell(0);
        if (cell == null)
            return false;
        IColor color = cell.CellStyle.FillForegroundColorColor;
        if (color == null)
            return false;
        return GetColorRule(color.RGB) == FieldRule.RULE_FINISH;
    }

    private void PrintColor()
    {
        for (int i = sheet.FirstRowNum; i < sheet.LastRowNum; i++)
        {
            IRow row = sheet.GetRow(i);
            if (row == null)
                continue;
            for (int j = row.FirstCellNum; j < row.LastCellNum; ++j)
            {
                ICell cell = row.GetCell(j);
                if (cell == null)
                    continue;
                Console.Write(i + "," + j + ":");
                HSSFColor color = (HSSFColor)cell.CellStyle.FillForegroundColorColor;
                Console.Write("(" + color.RGB[0] + "," + color.RGB[1] + "," + color.RGB[2] + ") ");
            }
            Console.WriteLine();
        }
    }

    public bool LoadFile()
    {
        using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            this.workbook = WorkbookFactory.Create(fs);
            this.sheet = workbook.GetSheetAt(0);
            fs.Close();
        }
        int i = sheet.FirstRowNum;
        while (i <= sheet.LastRowNum)
        {
            IRow row = sheet.GetRow(i++);
            if (IsSkipRow(row))
                continue;
            if (IsFinishRow(row))
                return false;
            ProcessHead(row);
            break;
        }
        if (this.header.Count < 1)
            return false;
        while (i <= sheet.LastRowNum)
        {
            IRow row = sheet.GetRow(i++);
            if (IsFinishRow(row))
                return true;
            if (IsSkipRow(row))
                continue;
            ProcessRecord(row);
        }
        return true;
    }

    /// <summary>
    /// 获取单元格类型
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private object GetValueType(ICell cell, out CellType type)
    {
        type = CellType.Error;
        if (cell == null)
            return null;
        type = cell.CellType;
        switch (type)
        {
            case CellType.Boolean: // Boolean
                return cell.BooleanCellValue;
            case CellType.Numeric: // Numeric
                //if (DateUtil.IsCellDateFormatted(cell))
                //    return cell.DateCellValue;
                return cell.NumericCellValue;
            case CellType.String: // String
                string str = cell.StringCellValue;
                if (string.IsNullOrEmpty(str))
                    return null;
                return str.ToString();    // cell.StringCellValue;
            case CellType.Error:    // Error
                return cell.ErrorCellValue;
            case CellType.Formula:  // Formula
                type = cell.CachedFormulaResultType;
                switch (type)
                {
                    case CellType.Boolean: // Boolean
                        return cell.BooleanCellValue;
                    case CellType.Numeric: // Numeric
                        //if (DateUtil.IsCellDateFormatted(cell))
                        //    return cell.DateCellValue;
                        return cell.NumericCellValue;
                    case CellType.String: // String
                        string strval = cell.StringCellValue;
                        if (string.IsNullOrEmpty(strval))
                            return null;
                        return strval.ToString();    // cell.StringCellValue;
                    case CellType.Error:    // Error
                        return cell.ErrorCellValue;
                    case CellType.Unknown:
                    case CellType.Blank: // Blank
                    default:
                        return null;    //return "=" + cell.CellFormula;
                }
            case CellType.Unknown:
            case CellType.Blank: // Blank
            default:
                return null;    //return "=" + cell.CellFormula;
        }
    }

    private void ProcessHead(IRow head)
    {
        this.header.Clear();
        for (int i = head.FirstCellNum; i < head.LastCellNum; i++)
        {
            ICell cell = head.GetCell(i);
            CellType type;
            object obj = GetValueType(cell, out type);
            if (obj != null)
            {
                IColor color = cell.CellStyle.FillForegroundColorColor;
                FieldRule rule = GetColorRule(color.RGB);
                string name = obj.ToString();
                if (name != "" && color != null && rule != FieldRule.RULE_IGNORE && rule != FieldRule.RULE_ERROR)
                {
                    HeadeCol col = new HeadeCol();
                    col.index = i;
                    col.rule = rule;
                    col.name = name;
                    this.header.Add(col);
                }
            }
        }
    }

    private void ProcessRecord(IRow row)
    {
        Dictionary<string, RecordCol> record = new Dictionary<string, RecordCol>();
        for (int i = 0; i < this.header.Count; i++)
        {
            int index = this.header[i].index;
            ICell cell = row.GetCell(index);
            CellType type;
            object obj = GetValueType(cell, out type);
            RecordCol col = new RecordCol();
            col.index = index;
            col.val = obj;
            col.type = type;
            record.Add(this.header[i].name, col);
        }
        this.records.Add(record);
    }

    public bool ExportLuaFile(string path)
    {
        string filename = path + this.exportname.ToLower() + ".config";
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("return {");
        foreach (Dictionary<string, RecordCol> record in this.records)
        {
            if (this.header.Count < 1)
                break;
            string field = this.header[0].name;
            string key = record[field].val.ToString();
            if (record[field].type != CellType.Numeric)
                key = QuoteStr(key);
            sb.Append("\t[" + key + "] = {");
            for (int i = 0; i < this.header.Count; i++)
            {
                FieldRule rule = this.header[i].rule;
                field = this.header[i].name;
                if (rule == FieldRule.RULE_COMMON || rule == FieldRule.RULE_SERVER)
                {
                    object val = record[field].val;
                    string str = "nil";
                    if (val != null)
                        if (IsTable(val.ToString()))
                            str = ConvertLuaTable(val.ToString());
                        else if (record[field].type == CellType.Boolean)
                            str = val.ToString().ToLower();
                        else
                            str = QuoteStr(val.ToString());
                    /// float int
                    sb.Append(" " + field + " = " + str + ",");
                }
            }
            sb.AppendLine(" },");
        }
        sb.AppendLine("}");
        File.WriteAllBytes(filename, Encoding.UTF8.GetBytes(sb.ToString()));
        return true;
    }

    public bool ExportCsvFile(string path)
    {
        bool result = false;
        bool isDelete = true;
        string filename = path + this.exportname + ".csv";
        using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
        {
            bool isFirstCol = true;
            for (int i = 0; i < this.header.Count; i++)
            {
                FieldRule rule = this.header[i].rule;
                if (rule == FieldRule.RULE_CLIENT || rule == FieldRule.RULE_COMMON)
                {
                    if (!isFirstCol)
                        writer.Write("\t");
                    isFirstCol = false;
                    writer.Write(this.header[i].name);
                }
            }
            writer.WriteLine();

            foreach (Dictionary<string, RecordCol> record in this.records)
            {
                isFirstCol = true;
                for (int i = 0; i < this.header.Count; i++)
                {
                    FieldRule rule = this.header[i].rule;
                    string field = this.header[i].name;
                    if (rule == FieldRule.RULE_COMMON || rule == FieldRule.RULE_CLIENT)
                    {
                        object val = record[field].val;
                        string str = "";
                        if (val != null)
                            str = val.ToString();
                        //                            if (IsTable(val.ToString()))
                        //                                str = Table2String(val.ToString());
                        //                            else
                        //                                str = val.ToString();
                        if (!isFirstCol)
                            writer.Write("\t");
                        isFirstCol = false;

                        isDelete = false;

                        writer.Write(str);
                    }
                }
                writer.WriteLine();
            }

            if (isDelete == false)
                Console.WriteLine("前端配置 {0}.csv 导出成功~", exportname);
            else
                Console.WriteLine("前端配置 {0}.csv 不需要导出", exportname);

            result = true;
        }

        if (isDelete && File.Exists(filename))
            File.Delete(filename);

        return result;
    }

    public bool ExportServerCsvFile(string path)
    {
        bool result = false;
        bool isDelete = true;

        string filename = path + this.exportname.ToLower() + ".csv";
        using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
        {
            bool isFirstCol = true;
            for (int i = 0; i < this.header.Count; i++)
            {
                FieldRule rule = this.header[i].rule;
                if (rule == FieldRule.RULE_SERVER || rule == FieldRule.RULE_COMMON)
                {
                    if (!isFirstCol)
                        writer.Write("\t");
                    isFirstCol = false;
                    writer.Write(this.header[i].name);
                }
            }
            writer.WriteLine();

            int index = 0;

            foreach (Dictionary<string, RecordCol> record in this.records)
            {
                index++;
                if (index <= 2)
                    continue;

                isFirstCol = true;
                for (int i = 0; i < this.header.Count; i++)
                {
                    FieldRule rule = this.header[i].rule;
                    string field = this.header[i].name;
                    if (rule == FieldRule.RULE_COMMON || rule == FieldRule.RULE_SERVER)
                    {
                        object val = record[field].val;
                        string str = "";
                        if (val != null)
                            str = val.ToString();
                        //                            if (IsTable(val.ToString()))
                        //                                str = Table2String(val.ToString());
                        //                            else
                        //                                str = val.ToString();
                        if (!isFirstCol)
                            writer.Write("\t");
                        isFirstCol = false;
                        isDelete = false;
                        writer.Write(str);
                    }
                }
                writer.WriteLine();
            }

            if (isDelete == false)
                Console.WriteLine("后端配置 {0}.csv 导出成功~", exportname.ToLower());
            else
                Console.WriteLine("后端配置 {0}.csv 不需要导出", exportname.ToLower());

            result = true;

        }

        if (isDelete && File.Exists(filename))
            File.Delete(filename);

        return result;
    }

    public void ImportCsv(string path)
    {
        StreamReader sr = null;
        FileStream fs = null;
        try
        {
            #region 以Unicode编码打开csv文件

            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                sr = new StreamReader(fs, Encoding.Unicode);
            }
            catch (IOException ex)
            {
                throw new IOException("读取文件出错：" + path + "\r\n" + ex.StackTrace);
            }

            #endregion

            #region 解析列表名称和数据类型，忽略描述

            string[] fieldNames = ReadLine(sr);
            //            string sum = "";
            //            for (int i = 0; i < fieldNames.Length; i++)
            //            {
            //                sum += fieldNames[i] + "    ";
            //            }
            //            sum += "\r\n";
            //            Console.WriteLine(sum);

            //列类型
            string[] types = ReadLine(sr);

            //            sum = "";
            //            for (int i = 0; i < types.Length; i++)
            //            {
            //                sum += types[i] + "    ";
            //            }
            //            sum += "\r\n";
            //            Console.WriteLine(sum);

            //列描述，可忽略
            string[] decs = ReadLine(sr);

            //            sum = "";
            //            for (int i = 0; i < decs.Length; i++)
            //            {
            //                sum += decs[i] + "    ";
            //            }
            //            sum += "\r\n";
            //            Console.WriteLine(sum);


            #endregion

            #region 解析所有行数据

            //数据
            string[] sArray = null;
            //            sum = "";
            List<string[]> aList = new List<string[]>();
            while ((sArray = ReadLine(sr)) != null)
            {
                //                for (int i = 0; i < sArray.Length; i++)
                //                {
                //                    sum += sArray[i] + "    ";
                //                }
                //                sum += "\r\n";
                aList.Add(sArray);
            }
            //            Console.WriteLine(sum);
            #endregion

            LoadFile1(path, decs, fieldNames, types, aList);
        }
        finally
        {
            try
            {
                fs.Close();
                sr.Close();
            }
            catch
            {
            }
        }
    }


    /// <summary>
    /// 增加对Excel的“Unicode 文本”格式中换行和双引号的解析支持
    /// </summary>
    /// <param name="sr"></param>
    /// <returns></returns>
    private string[] ReadLine(StreamReader sr)
    {
        string line = sr.ReadLine();

        if (!string.IsNullOrEmpty(line))
        {
            bool end = false;
            while (end == false && line != null)
            {
                if (end && line == "")
                    line += "\r\n";
                else
                {
                    if (IsFullLine(line))
                        end = true;
                    else
                    {
                        string temp = sr.ReadLine();
                        line += "\n" + (temp ?? "");
                    }
                }
            }
            string[] columns = line.Split('\t');
            for (int i = 0; i < columns.Length; i++)
            {
                string temp = columns[i];
                //                if (temp.StartsWith("\""))
                //                {
                //                    temp = temp.Substring(1, temp.Length - 2);
                //                    temp = temp.Replace("\"\"", "\"");
                //                }

                //                if (temp.Contains("\r\n"))
                //                {
                //                    Console.WriteLine(temp);
                //                }
                //                if (temp.EndsWith("\r\n"))
                //                {
                //                    temp = temp.Remove(temp.Length - 2, 2); //删除最后的换行
                //                }
                columns[i] = temp;
            }
            return columns;
        }
        return null;
    }

    /// <summary>
    /// 判断一行是否还有后续行
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private bool IsFullLine(string line)
    {
        int index = line.LastIndexOf('\t') + 1;
        char[] chars = line.ToCharArray();
        if (index >= chars.Length || chars[index] != '\"')
            return true;
        else
        {
            int i = index + 1;
            while (i < chars.Length)
            {
                if (chars[i] == '\"')
                {
                    if (i + 1 < chars.Length && chars[i + 1] == '\"')
                        i++;
                    else
                        return true;
                }
                i++;
            }
            return false;
        }
    }

    public void LoadFile1(string path, string[] decs, string[] names, string[] types, List<string[]> allrows)
    {
        using (FileStream fs = File.Open("template.xls", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            this.workbook = WorkbookFactory.Create(fs);
            this.sheet = workbook.GetSheetAt(0);
        }

        HSSFRow nameRow = sheet.CreateRow(1) as HSSFRow;
        for (int i = 0; i < names.Length; i++)
        {
            HSSFCell newcell = nameRow.CreateCell(i) as HSSFCell;
            newcell.SetCellValue(names[i]);

            HSSFCellStyle style = workbook.CreateCellStyle() as HSSFCellStyle;
            style.FillForegroundColor = HSSFColor.SkyBlue.Index;
            style.FillPattern = FillPattern.SolidForeground;
            newcell.CellStyle = style;
        }

        HSSFRow typeRow = sheet.CreateRow(2) as HSSFRow;

        for (int i = 0; i < types.Length; i++)
        {
            HSSFCell newcell = typeRow.CreateCell(i) as HSSFCell;
            newcell.SetCellValue(types[i]);
        }

        HSSFRow decRow = sheet.CreateRow(3) as HSSFRow;

        for (int i = 0; i < decs.Length; i++)
        {
            HSSFCell newcell = decRow.CreateCell(i) as HSSFCell;
            newcell.SetCellValue(decs[i]);
        }

        for (int i = 0; i < allrows.Count; i++)
        {
            HSSFRow row = sheet.CreateRow(4 + i) as HSSFRow;

            string[] values = allrows[i];

            for (int j = 0; j < values.Length; j++)
            {
                HSSFCell newcell = row.CreateCell(j) as HSSFCell;
                string type = types[j];
                string value = values[j];
                if (string.IsNullOrEmpty(value))
                {
                    newcell.SetCellValue(value);
                    continue;
                }
                switch (type)
                {
                    case "int":
                        int intv = 0;
                        int.TryParse(value, out intv);
                        newcell.SetCellValue(intv);
                        break;
                    case "string":
                        int result;
                        if (IsMumberric(value, out result))
                        {
                            //Console.WriteLine(result);
                            int.TryParse(value, out result);
                            newcell.SetCellValue(result);
                        }
                        else
                        {
                            newcell.SetCellValue(value);
                        }
                        break;
                    default:
                        newcell.SetCellValue(value);
                        break;
                }
            }
        }

        HSSFRow finishRow = sheet.CreateRow(sheet.LastRowNum + 1) as HSSFRow;
        for (int i = 0; i < names.Length; i++)
        {
            HSSFCell newcell = finishRow.CreateCell(i) as HSSFCell;

            HSSFCellStyle style = workbook.CreateCellStyle() as HSSFCellStyle;
            style.FillForegroundColor = HSSFColor.DarkTeal.Index;
            style.FillPattern = FillPattern.SolidForeground;
            newcell.CellStyle = style;
        }

        path = path.Replace(@"Client\config", "ExcelConfig");

        path = path.Remove(path.LastIndexOf('.')) + ".xls";
        FileStream fs2 = File.Create(path);
        workbook.Write(fs2);
        fs2.Close();
    }

    bool IsMumberric(string msg, out int result)
    {
        Regex rex = new Regex("^\\d+$");
        //Regex rex = new Regex(@"^[-]?d+[.]?d*$");
        result = 0;
        if (rex.IsMatch(msg))
        {
            result = int.Parse(msg);
            return true;
        }
        return false;
    }

    private void ChangeXlsName(string oldName, string newName)
    {
        Console.WriteLine("workbook" + workbook.NumberOfSheets);
        int i = 0;
        bool isNeedDell = false;
        while (i <= sheet.LastRowNum)
        {
            IRow row = sheet.GetRow(i++);
            if (row == null)
                continue;
            foreach (var VARIABLE in row.Cells)
            {
                CellType type;
                object obj = GetValueType(VARIABLE, out type);
                if (obj != null)
                {
                    string item = obj.ToString();
                    if (item.Contains(oldName))
                    {
                        item = item.Replace(oldName, newName);
                        isNeedDell = true;
                        VARIABLE.SetCellValue(item);
                    }
                }
            }
        }
        if (isNeedDell)
        {
            FileStream file = new FileStream(currentFile, FileMode.OpenOrCreate);
            workbook.Write(file);
            file.Flush();
            file.Close();
        }

    }

    private static string currentFile;
    /// <summary>
    /// 第三个参数表示要操作的方式
    /// </summary>
    /// <param name="args">type==3表示xls修改文字 参数1为原文字参数2为现在的文字</param>
    static void Main(string[] args)
    {

        //        CfgExportor export1 = new CfgExportor("111");
        //
        //        export1.ImportCsv("BountyState.csv");
        //        Console.ReadKey();
        //        return;
        //
        //        string[] files1 = Directory.GetFiles(@"E:\ssj_MobileGame\modifyBranch\Client\config", "*.csv", SearchOption.TopDirectoryOnly);
        //
        //        for (int i = 0; i < files1.Length; i++)
        //        {
        //            export1.ImportCsv(files1[i]);
        //        }
        //
        //        Console.WriteLine("导出完成 *^_^*");
        //       
        //        Console.ReadKey();
        //
        //        return;

        if (args.Length < 4)
        {
            Console.WriteLine("Usage: {0} csv_filepath lua_filepath type filename", Process.GetCurrentProcess().ProcessName);
            Console.ReadLine();
            return;
        }
        string[] files = null;
        string type = args[2];
        if (type == "1")
        {
            files = new string[1] { args[3] };
        }
        else if (type == "2")
        {
            files = Directory.GetFiles(args[3], "*.xls", SearchOption.TopDirectoryOnly);
        }
        else if (type == "3")
        {
            files = Directory.GetFiles(args[3], "*.xls", SearchOption.TopDirectoryOnly);
        }

        for (int i = 0; i < files.Length; i++)
        {
            currentFile = files[i];
            string file = files[i];
            if (file.Contains("template"))
                continue;

            CfgExportor export = new CfgExportor(file);
            if (!export.LoadFile())
                Console.WriteLine("加载文件:" + file + " 失败！");
            else
            {
                if (type == "3")
                {
                    export.ChangeXlsName(args[0], args[1]);
                }
                else
                {
                    if (!export.ExportCsvFile(args[0]))
                        Console.WriteLine("导出 CSV {0} 文件失败！", file);

 //                   if (!export.ExportServerCsvFile(args[1]))
 //                       Console.WriteLine("导出 Server CSV {0} 文件失败！", file);

                    Console.WriteLine("导出完成 *^_^*");
                }
            }
        }

        // Console.ReadKey();
    }
}
