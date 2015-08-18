' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Text
Imports System.Data.OleDb
Imports System.ComponentModel
Imports System.Drawing
Imports TVA.Database.Common

Namespace Database

    ' Note: user must define primary key fields in the database or through code to be used for updates for each table
    ' in the table collection in order for the updates to occur, hence any tables which have no key fields defined
    ' yet appear in the table collection will not be updated...
    <ToolboxBitmap(GetType(DataUpdater), "DataUpdater.bmp"), DefaultProperty("FromConnectString"), DefaultEvent("OverallProgress")> _
    Public Class DataUpdater

        Inherits BulkDataOperationBase
        Implements IComponent

        ' Component Implementation
        Public Event Disposed(ByVal sender As Object, ByVal e As EventArgs) Implements IComponent.Disposed

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overridable Overloads Property Site() As ISite Implements IComponent.Site
            Get
                Return ComponentSite
            End Get
            Set(ByVal Value As ISite)
                ComponentSite = Value
            End Set
        End Property

        Public Overrides Sub Close()

            MyBase.Close()
            RaiseEvent Disposed(Me, EventArgs.Empty)

        End Sub

        Public Sub New()

            MyBase.New()

        End Sub

        Public Sub New(ByVal FromConnectString As String, ByVal ToConnectString As String)

            MyBase.New(FromConnectString, ToConnectString)

        End Sub

        Public Sub New(ByVal FromSchema As Schema, ByVal ToSchema As Schema)

            MyBase.New(FromSchema, ToSchema)

        End Sub

        Public Overrides Sub Execute()

            Dim lstTables As New ArrayList()
            Dim tblLookup As Table
            Dim tbl As Table
            Dim x As Integer

            If colTables Is Nothing Then Analyze()

            ' We copy the tables into an array list so we can sort and process them in priority order
            For Each tbl In colTables
                If tbl.Process Then lstTables.Add(tbl)
            Next

            lstTables.Sort()

            ' Begin updating data in destination tables
            For x = 0 To lstTables.Count - 1
                tbl = DirectCast(lstTables(x), Table)

                ' Lookup table name in destination datasource
                tblLookup = schTo.Tables.FindByMapName(tbl.MapName)
                If Not tblLookup Is Nothing Then
                    ' We can only do Sql updates where key fields are defined...
                    If tbl.RowCount > 0 And IIf(flgUseFromSchemaRI, tbl.PrimaryKeyFieldCount, tblLookup.PrimaryKeyFieldCount) > 0 Then
                        ' Inform clients of table update
                        RaiseEvent_TableProgress(tbl.Name, True, (x + 1), lstTables.Count)

                        ' Update destination table based on source table records
                        ExecuteUpdates(tbl, tblLookup)
                    Else
                        ' Inform clients of table skip
                        RaiseEvent_TableProgress(tbl.Name, False, (x + 1), lstTables.Count)

                        ' If we skipped rows because of lack of key fields, make sure and
                        ' synchronize overall progress event
                        If tbl.RowCount > 0 Then
                            intOverallProgress += tbl.RowCount
                            RaiseEvent_OverallProgress(intOverallProgress, intOverallTotal)
                        End If
                    End If
                Else
                    ' Inform clients of table skip
                    RaiseEvent_TableProgress(tbl.Name, False, (x + 1), lstTables.Count)
                End If
            Next

            ' Perform final update of progress information
            RaiseEvent_TableProgress("", False, lstTables.Count, lstTables.Count)

        End Sub

        Private Sub ExecuteUpdates(ByVal FromTable As Table, ByVal ToTable As Table)

            Dim colFields As Fields
            Dim fld As Field
            Dim fldLookup As Field
            Dim fldCommon As Field
            Dim strUpdateSqlStub As String
            Dim strUpdateSql As StringBuilder
            Dim strWhereSql As StringBuilder
            Dim strValue As String
            Dim flgIsPrimary As Boolean
            Dim flgAddedFirstUpdate As Boolean
            Dim intProgress As Integer
            Dim intTotal As Integer
            Dim tblSource As Table = IIf(flgUseFromSchemaRI, FromTable, ToTable)

            ' Create a field list of all of the common fields in both tables
            colFields = New Fields(tblSource)

            For Each fld In FromTable.Fields
                ' Lookup field name in destination table
                fldLookup = ToTable.Fields(fld.Name)
                If Not fldLookup Is Nothing Then
                    With fldLookup
                        ' We currently don't handle binary fields...
                        If _
                            Not (fld.Type = OleDbType.Binary Or fld.Type = OleDbType.LongVarBinary Or fld.Type = OleDbType.VarBinary) And _
                            Not (.Type = OleDbType.Binary Or .Type = OleDbType.LongVarBinary Or .Type = OleDbType.VarBinary) _
                        Then
                            ' Copy field information from destination field
                            If flgUseFromSchemaRI Then
                                fldCommon = New Field(colFields, fld.Name, fld.Type)
                                fldCommon.flgAutoInc = fld.AutoIncrement
                            Else
                                fldCommon = New Field(colFields, .Name, .Type)
                                fldCommon.flgAutoInc = .AutoIncrement
                            End If

                            colFields.Add(fldCommon)
                        End If
                    End With
                End If
            Next

            ' Exit if no common field names were found
            If colFields.Count = 0 Then
                intOverallProgress += FromTable.RowCount
                Exit Sub
            End If

            intTotal = FromTable.RowCount
            RaiseEvent_RowProgress(FromTable.Name, 0, intTotal)
            RaiseEvent_OverallProgress(intOverallProgress, intOverallTotal)

            ' Execute source query
            With ExecuteReader("SELECT " & colFields.GetList() & " FROM " & FromTable.FullName, FromTable.Connection, CommandBehavior.SequentialAccess, Timeout)
                ' Create Sql update stub
                strUpdateSqlStub = "UPDATE " & ToTable.FullName & " SET "

                ' Insert data for each row...
                While .Read()
                    strUpdateSql = New StringBuilder(strUpdateSqlStub)
                    strWhereSql = New StringBuilder
                    flgAddedFirstUpdate = False

                    ' Coerce all field data into proper Sql format
                    For Each fld In colFields
                        Try
                            fld.Value = .Item(fld.Name)
                        Catch ex As Exception
                            fld.Value = ""
                            RaiseEvent_SqlFailure("Failed to get field value for [" & tblSource.Name & "." & fld.Name & "]", ex)
                        End Try

                        ' Check to see if this is a key field
                        fldLookup = tblSource.Fields(fld.Name)
                        If Not fldLookup Is Nothing Then
                            flgIsPrimary = fldLookup.IsPrimaryKey
                        Else
                            flgIsPrimary = False
                        End If

                        strValue = fld.SqlEncodedValue
                        If StrComp(strValue, "NULL", CompareMethod.Text) <> 0 Then
                            If flgIsPrimary Then
                                If strWhereSql.Length = 0 Then
                                    strWhereSql.Append(" WHERE ")
                                Else
                                    strWhereSql.Append(" AND ")
                                End If

                                strWhereSql.Append("[")
                                strWhereSql.Append(fld.Name)
                                strWhereSql.Append("] = ")
                                strWhereSql.Append(strValue)
                            Else
                                If flgAddedFirstUpdate Then
                                    strUpdateSql.Append(", ")
                                Else
                                    flgAddedFirstUpdate = True
                                End If

                                strUpdateSql.Append("[")
                                strUpdateSql.Append(fld.Name)
                                strUpdateSql.Append("] = ")
                                strUpdateSql.Append(strValue)
                            End If
                        End If
                    Next

                    If strWhereSql.Length > 0 Then
                        ' Add where criteria to Sql update statement
                        strUpdateSql.Append(strWhereSql.ToString())

                        Try
                            ' Update record in destination table
                            If flgAddedFirstUpdate Then ExecuteNonQuery(strUpdateSql.ToString(), ToTable.Connection, Timeout)
                        Catch ex As Exception
                            RaiseEvent_SqlFailure(strUpdateSql.ToString(), ex)
                        End Try
                    Else
                        RaiseEvent_SqlFailure(strUpdateSql.ToString(), New DataException("ERROR: No ""WHERE"" criteria was generated for Sql update statement, primary key value missing?  Update not performed"))
                    End If

                    intProgress += 1
                    intOverallProgress += 1
                    If intProgress Mod RowReportInterval = 0 Then
                        RaiseEvent_RowProgress(FromTable.Name, intProgress, intTotal)
                        RaiseEvent_OverallProgress(intOverallProgress, intOverallTotal)
                    End If
                End While

                .Close()
            End With

            RaiseEvent_RowProgress(FromTable.Name, intTotal, intTotal)
            RaiseEvent_OverallProgress(intOverallProgress, intOverallTotal)

        End Sub

    End Class

End Namespace