using SqlSugar;
using System;

namespace Poxiao.Lab.Helpers;

public static class IntermediateDataFormulaSchemaHelper
{
    public const string AdvancedMode = "advanced";
    public const string RangeMode = "range";

    private const string TableName = "LAB_INTERMEDIATE_DATA_FORMULA";
    private const string EditorModeColumnName = "F_EDITOR_MODE";
    private static readonly object SyncLock = new();
    private static bool editorModeColumnEnsured;

    public static void EnsureEditorModeColumn(ISqlSugarClient db)
    {
        if (editorModeColumnEnsured || db == null)
        {
            return;
        }

        lock (SyncLock)
        {
            if (editorModeColumnEnsured)
            {
                return;
            }

            if (!db.DbMaintenance.IsAnyTable(TableName, false))
            {
                editorModeColumnEnsured = true;
                return;
            }

            if (!db.DbMaintenance.IsAnyColumn(TableName, EditorModeColumnName, false))
            {
                db.DbMaintenance.AddColumn(
                    TableName,
                    new DbColumnInfo
                    {
                        DbColumnName = EditorModeColumnName,
                        DataType = "varchar",
                        Length = 20,
                        IsNullable = true,
                        DefaultValue = AdvancedMode,
                        ColumnDescription = "formula editor mode"
                    });
            }

            db.Ado.ExecuteCommand(
                $"UPDATE {TableName} SET {EditorModeColumnName} = @EditorMode WHERE {EditorModeColumnName} IS NULL",
                new SugarParameter("@EditorMode", AdvancedMode));

            editorModeColumnEnsured = true;
        }
    }

    public static string NormalizeEditorMode(string editorMode)
    {
        return string.Equals(editorMode, RangeMode, StringComparison.OrdinalIgnoreCase)
            ? RangeMode
            : AdvancedMode;
    }
}
