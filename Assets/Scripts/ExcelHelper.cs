using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OfficeOpenXml;
using System.IO;
using UnityEditor;

public static class ExcelHelper {

    static ExcelPackage xls = null;

    public struct DataColumn {
        public string _sheet;
        public string _col;
        public DataColumn(string workSheet, string column) {
            _sheet = workSheet;
            _col = column;
        }
    }

    public static readonly string MAPSHEET = "Maps";
    public static readonly string PLATFORMSHEET = "Platforms";
    public static readonly string CHUNKSHEET = "Chunks";

    #region Excel Chunk Data
    public static readonly DataColumn CHUNK_TEST_NUMBER = new DataColumn(CHUNKSHEET, "A");
    public static readonly DataColumn ANGLE = new DataColumn(CHUNKSHEET, "B");
    public static readonly DataColumn ANGLE_TYPE = new DataColumn(CHUNKSHEET, "C");
    public static readonly DataColumn SMALL_DISCONTINUITIES = new DataColumn(CHUNKSHEET, "D");
    public static readonly DataColumn LARGE_DISCONTINUITIES = new DataColumn(CHUNKSHEET, "E");
    #endregion

    #region Excel Platform Data
    public static readonly DataColumn PLATFORM_TEST_NUMBER = new DataColumn(PLATFORMSHEET, "A");
    public static readonly DataColumn PLATFORM_TILECOUNT = new DataColumn(PLATFORMSHEET, "B");
    public static readonly DataColumn ROCKSIZE = new DataColumn(PLATFORMSHEET, "C");
    public static readonly DataColumn ROCKDISTANCE = new DataColumn(PLATFORMSHEET, "D");
    public static readonly DataColumn ROCKDENSITY = new DataColumn(PLATFORMSHEET, "E");
    #endregion

    #region Excel Map Data
    public static readonly DataColumn MAP_TEST_NUMBER = new DataColumn(MAPSHEET, "A");
    public static readonly DataColumn PLATFORMCOUNT = new DataColumn(MAPSHEET, "B");
    public static readonly DataColumn ROOMCOUNT = new DataColumn(MAPSHEET, "C");
    #endregion

    #region  Excel CA Data
    public static readonly DataColumn CHUNK_PCG_TYPE = new DataColumn(CHUNKSHEET, "F");
    public static readonly DataColumn CHUNK_DIMENSIONS = new DataColumn(CHUNKSHEET, "G");
    public static readonly DataColumn CHUNK_SEED = new DataColumn(CHUNKSHEET, "H");
    public static readonly DataColumn CHUNK_FILLAMOUNT = new DataColumn(CHUNKSHEET, "I");
    public static readonly DataColumn CHUNK_BIRTHLIMIT = new DataColumn(CHUNKSHEET, "J");
    public static readonly DataColumn CHUNK_DEATHLIMIT = new DataColumn(CHUNKSHEET, "K");
    public static readonly DataColumn CHUNK_SMOOTHITERATIONS = new DataColumn(CHUNKSHEET, "L");
    public static readonly DataColumn CHUNK_BLENDLAYERS = new DataColumn(CHUNKSHEET, "M");

    public static readonly DataColumn PLATFORM_PCG_TYPE = new DataColumn(PLATFORMSHEET, "F");
    public static readonly DataColumn PLATFORM_DIMENSIONS = new DataColumn(PLATFORMSHEET, "G");
    public static readonly DataColumn PLATFORM_SEED = new DataColumn(PLATFORMSHEET, "H");
    public static readonly DataColumn PLATFORM_FILLAMOUNT = new DataColumn(PLATFORMSHEET, "I");
    public static readonly DataColumn PLATFORM_BIRTHLIMIT = new DataColumn(PLATFORMSHEET, "J");
    public static readonly DataColumn PLATFORM_DEATHLIMIT = new DataColumn(PLATFORMSHEET, "K");
    public static readonly DataColumn PLATFORM_SMOOTHITERATIONS = new DataColumn(PLATFORMSHEET, "L");
    public static readonly DataColumn PLATFORM_BLENDLAYERS = new DataColumn(PLATFORMSHEET, "M");

    public static readonly DataColumn MAP_PCG_TYPE = new DataColumn(MAPSHEET, "D");
    public static readonly DataColumn MAP_DIMENSIONS = new DataColumn(MAPSHEET, "E");
    public static readonly DataColumn MAP_SEED = new DataColumn(MAPSHEET, "F");
    public static readonly DataColumn MAP_FILLAMOUNT = new DataColumn(MAPSHEET, "G");
    public static readonly DataColumn MAP_BIRTHLIMIT = new DataColumn(MAPSHEET, "H");
    public static readonly DataColumn MAP_DEATHLIMIT = new DataColumn(MAPSHEET, "I");
    public static readonly DataColumn MAP_SMOOTHITERATIONS = new DataColumn(MAPSHEET, "J");
    public static readonly DataColumn MAP_BLENDLAYERS = new DataColumn(MAPSHEET, "K");
    #endregion

    public static bool Init() {
        try {
            FileInfo fileInfo = new FileInfo(Directory.GetCurrentDirectory() + "/results.xlsx");
            xls = new ExcelPackage(fileInfo);
            return true;
        }
        catch {
            EditorUtility.DisplayDialog("Excel running", "Close the Excel Spreadsheet before running this script", "Ok");
            return false;
        }
    }

    public static void WriteData(DataColumn column, string data) {
        int row = GetEmptyRow(column);
        WriteData(column, row, data);
    }

    public static int GetEmptyRow(DataColumn column) {
        ExcelWorksheet sheet = xls.Workbook.Worksheets[column._sheet];
        int row = 0;
        int stepper = 100;
        while (true) {
            string address = column._col + row;
            if (sheet.Cells[address].Value == null) {
                if (stepper != 1) {
                    row -= stepper;
                    stepper /= 10;
                }
                else {
                    return row;
                }
            }
            row += stepper;
        }
    }

    public static int GetTestNumber() {
        int emptyRow = GetEmptyRow(MAP_TEST_NUMBER);
        int row = emptyRow - 1;
        ExcelWorksheet sheet = xls.Workbook.Worksheets[MAP_TEST_NUMBER._sheet];

        string address = MAP_TEST_NUMBER._col + row;
        if (sheet.Cells[address].Value == null) return 1;
        string value = sheet.Cells[address].GetValue<string>();
        if (int.TryParse(value, out int result)) {
            return result + 1;
        }
        return 1;
    }

    public static int GetEmptyRow(string sheetIndex) {
        ExcelWorksheet sheet = xls.Workbook.Worksheets[sheetIndex];
        int row = 0;
        int stepper = 100;
        while (true) {
            string address = "A" + row;
            if (sheet.Cells[address].Value == null) {
                if (stepper != 1) {
                    row -= stepper;
                    stepper /= 10;
                }
                else {
                    return row;
                }
            }
            row += stepper;
        }
    }

    public static void WriteData(DataColumn column, int row, string data) {
        ExcelWorksheet sheet = xls.Workbook.Worksheets[column._sheet];
        string address = column._col + row;
        sheet.Cells[address].Value = data;
    }


    public static void Save() {
        xls.Save();
    }

    public static void StartExcel() {
        System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "/results.xlsx");
    }
}
