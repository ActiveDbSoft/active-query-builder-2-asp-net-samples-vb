Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Xml
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Control

Partial Public Class QueryBuilderOffline
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(sender As Object, e As EventArgs)

    End Sub

    Public Sub SleepModeChanged(sender As Object, e As EventArgs)
        Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
        If queryBuilder.SleepMode Then StatusBar1.Message.Error("Unsupported SQL statement.")
    End Sub

    Public Sub QueryBuilderControl1_Init(sender As Object, e As EventArgs)
        ' Get instance of QueryBuilder
        Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
        If queryBuilder.SyntaxProvider Is Nothing Then
            ' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
            queryBuilder.BehaviorOptions.AllowSleepMode = True
            queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()
        End If
    End Sub

    Protected Sub btn1_Click(sender As Object, e As EventArgs)
        Dim queryBuilder1 = QueryBuilderControl1.QueryBuilder

        ' prevent QueryBuilder to request metadata
        queryBuilder1.OfflineMode = True

        queryBuilder1.MetadataProvider = Nothing

        Dim metadataContainer As MetadataContainer = queryBuilder1.MetadataContainer
        metadataContainer.BeginUpdate()

        Try
            metadataContainer.Items.Clear()

            Dim schemaDbo As MetadataNamespace = metadataContainer.AddSchema("dbo")

            ' prepare metadata for table "Orders"
            Dim orders As MetadataObject = schemaDbo.AddTable("Orders")
            ' fields
            orders.AddField("OrderId")
            orders.AddField("CustomerId")

            ' prepare metadata for table "Order Details"
            Dim orderDetails As MetadataObject = schemaDbo.AddTable("Order Details")
            ' fields
            orderDetails.AddField("OrderId")
            orderDetails.AddField("ProductId")
            ' foreign keys
            Dim foreignKey As MetadataForeignKey = orderDetails.AddForeignKey("OrderDetailsToOrders")

            Using referencedName As New MetadataQualifiedName()
                referencedName.Add("Orders")
                referencedName.Add("dbo")

                foreignKey.ReferencedObjectName = referencedName
            End Using

            foreignKey.Fields.Add("OrderId")
            foreignKey.ReferencedFields.Add("OrderId")
        Finally
            metadataContainer.EndUpdate()
        End Try

        queryBuilder1.MetadataStructure.Refresh()
        StatusBar1.Message.Information("Metadata loaded")
    End Sub

    Protected Sub btn2_Click(sender As Object, e As EventArgs)
        Dim queryBuilder1 As ActiveDatabaseSoftware.ActiveQueryBuilder.QueryBuilder = QueryBuilderControl1.QueryBuilder

        ' allow QueryBuilder to request metadata
        queryBuilder1.OfflineMode = False

        queryBuilder1.MetadataProvider = Nothing
        queryBuilder1.MetadataContainer.Items.Clear()
        AddHandler queryBuilder1.MetadataContainer.ItemMetadataLoading, AddressOf way2ItemMetadataLoading
        queryBuilder1.MetadataStructure.Refresh()
        StatusBar1.Message.Information("Metadata loaded")
    End Sub

    Private Sub way2ItemMetadataLoading(sender As Object, item As MetadataItem, types As MetadataType)
        Select Case item.Type
            Case MetadataType.Root
                If (types And MetadataType.Schema) > 0 Then
                    item.AddSchema("dbo")
                End If
                Exit Select

            Case MetadataType.Schema
                If (item.Name = "dbo") AndAlso (types And MetadataType.Table) > 0 Then
                    item.AddTable("Orders")
                    item.AddTable("Order Details")
                End If
                Exit Select

            Case MetadataType.Table
                If item.Name = "Orders" Then
                    If (types And MetadataType.Field) > 0 Then
                        item.AddField("OrderId")
                        item.AddField("CustomerId")
                    End If
                ElseIf item.Name = "Order Details" Then
                    If (types And MetadataType.Field) > 0 Then
                        item.AddField("OrderId")
                        item.AddField("ProductId")
                    End If

                    If (types And MetadataType.ForeignKey) > 0 Then
                        Dim foreignKey As MetadataForeignKey = item.AddForeignKey("OrderDetailsToOrder")
                        foreignKey.Fields.Add("OrderId")
                        foreignKey.ReferencedFields.Add("OrderId")
                        Using name As New MetadataQualifiedName()
                            name.Add("Orders")
                            name.Add("dbo")

                            foreignKey.ReferencedObjectName = name
                        End Using
                    End If
                End If
                Exit Select
        End Select

        item.Items.SetLoaded(types, True)
    End Sub

    Private m_way1EventMetadataProvider As ActiveDatabaseSoftware.ActiveQueryBuilder.EventMetadataProvider
    Private ReadOnly Property Way1EventMetadataProvider() As ActiveDatabaseSoftware.ActiveQueryBuilder.EventMetadataProvider
        Get
            If m_way1EventMetadataProvider Is Nothing Then
                m_way1EventMetadataProvider = New EventMetadataProvider()
                AddHandler m_way1EventMetadataProvider.ExecSQL, New ActiveDatabaseSoftware.ActiveQueryBuilder.ExecSQLEventHandler(AddressOf Me.way3EventMetadataProvider_ExecSQL)
            End If
            Return m_way1EventMetadataProvider
        End Get
    End Property

    Private Sub way3EventMetadataProvider_ExecSQL(metadataProvider As BaseMetadataProvider, sql As String, schemaOnly As Boolean, ByRef dataReader As IDataReader)
        dataReader = Nothing

        If dbConnection IsNot Nothing Then
            Dim command As IDbCommand = dbConnection.CreateCommand()
            command.CommandText = sql
            dataReader = command.ExecuteReader()
        End If
    End Sub

    Private dbConnection As IDbConnection = New SqlConnection(ConfigurationManager.ConnectionStrings("AdventureWorks").ConnectionString)
    Protected Sub btn3_Click(sender As Object, e As EventArgs)
        Dim queryBuilder1 = QueryBuilderControl1.QueryBuilder

        If dbConnection IsNot Nothing Then
            Try
                dbConnection.Close()
                dbConnection.Open()

                ' allow QueryBuilder to request metadata
                queryBuilder1.OfflineMode = False

                ResetQueryBuilderMetadata()

                queryBuilder1.MetadataProvider = Way1EventMetadataProvider
                queryBuilder1.MetadataStructure.Refresh()
                StatusBar1.Message.Information("Metadata loaded")
            Catch ex As System.Exception
                StatusBar1.Message.[Error](ex.Message)
            End Try
        Else
            StatusBar1.Message.[Error]("Please setup a database connection by clicking on the ""Connect"" menu item before testing this method.")
        End If
    End Sub

    Private Sub ResetQueryBuilderMetadata()
        Dim queryBuilder1 As ActiveDatabaseSoftware.ActiveQueryBuilder.QueryBuilder = QueryBuilderControl1.QueryBuilder

        queryBuilder1.MetadataProvider = Nothing
        queryBuilder1.MetadataContainer.Items.Clear()
        RemoveHandler queryBuilder1.MetadataContainer.ItemMetadataLoading, AddressOf way2ItemMetadataLoading
    End Sub

    Protected Sub btn4_Click(sender As Object, e As EventArgs)
        Dim queryBuilder1 = QueryBuilderControl1.QueryBuilder
        queryBuilder1.MetadataProvider = Nothing
        queryBuilder1.OfflineMode = True
        ' prevent QueryBuilder to request metadata from connection
        Dim dataSet As New DataSet()

        ' Load sample dataset created in the Visual Studio with Dataset Designer
        ' and exported to XML using WriteXmlSchema() method.
        dataSet.ReadXmlSchema(Path.Combine(Server.MapPath("~/"), "StoredDataSetSchema.xml"))

        queryBuilder1.MetadataContainer.BeginUpdate()

        Try
            queryBuilder1.MetadataContainer.Items.Clear()

            ' add tables
            For Each table As DataTable In dataSet.Tables
                ' add new metadata table
                Dim metadataTable As MetadataObject = queryBuilder1.MetadataContainer.AddTable(table.TableName)

                ' add metadata fields (columns)
                For Each column As DataColumn In table.Columns
                    ' create new field
                    Dim metadataField As MetadataField = metadataTable.AddField(column.ColumnName)
                    ' setup field
                    metadataField.FieldType = TypeToDbType(column.DataType)
                    metadataField.Nullable = column.AllowDBNull
                    metadataField.[ReadOnly] = column.[ReadOnly]

                    If column.MaxLength <> -1 Then
                        metadataField.Size = column.MaxLength
                    End If

                    ' detect the field is primary key
                    For Each pkColumn As DataColumn In table.PrimaryKey
                        If column Is pkColumn Then
                            metadataField.PrimaryKey = True
                        End If
                    Next
                Next

                ' add relations
                For Each relation As DataRelation In table.ParentRelations
                    ' create new relation on the parent table
                    Dim metadataRelation As MetadataForeignKey = metadataTable.AddForeignKey(relation.RelationName)

                    ' set referenced table
                    Using referencedName As New MetadataQualifiedName()
                        referencedName.Add(relation.ParentTable.TableName)

                        metadataRelation.ReferencedObjectName = referencedName
                    End Using

                    ' set referenced fields
                    For Each parentColumn As DataColumn In relation.ParentColumns
                        metadataRelation.ReferencedFields.Add(parentColumn.ColumnName)
                    Next

                    ' set fields
                    For Each childColumn As DataColumn In relation.ChildColumns
                        metadataRelation.Fields.Add(childColumn.ColumnName)
                    Next
                Next
            Next
        Finally
            queryBuilder1.MetadataContainer.EndUpdate()
        End Try

        queryBuilder1.MetadataStructure.Refresh()
        StatusBar1.Message.Information("Metadata loaded")
    End Sub

    Private Shared Function TypeToDbType(type As Type) As DbType
        If type Is GetType(System.String) Then
            Return DbType.[String]
        ElseIf type Is GetType(System.Int16) Then
            Return DbType.Int16
        ElseIf type Is GetType(System.Int32) Then
            Return DbType.Int32
        ElseIf type Is GetType(System.Int64) Then
            Return DbType.Int64
        ElseIf type Is GetType(System.UInt16) Then
            Return DbType.UInt16
        ElseIf type Is GetType(System.UInt32) Then
            Return DbType.UInt32
        ElseIf type Is GetType(System.UInt64) Then
            Return DbType.UInt64
        ElseIf type Is GetType(System.Boolean) Then
            Return DbType.[Boolean]
        ElseIf type Is GetType(System.Single) Then
            Return DbType.[Single]
        ElseIf type Is GetType(System.Double) Then
            Return DbType.[Double]
        ElseIf type Is GetType(System.Decimal) Then
            Return DbType.[Decimal]
        ElseIf type Is GetType(System.DateTime) Then
            Return DbType.DateTime
        ElseIf type Is GetType(System.TimeSpan) Then
            Return DbType.Time
        ElseIf type Is GetType(System.Byte) Then
            Return DbType.[Byte]
        ElseIf type Is GetType(System.SByte) Then
            Return DbType.[SByte]
        ElseIf type Is GetType(System.Char) Then
            Return DbType.[String]
        ElseIf type Is GetType(System.Byte()) Then
            Return DbType.Binary
        ElseIf type Is GetType(System.Guid) Then
            Return DbType.Guid
        Else
            Return DbType.[Object]
        End If
    End Function
End Class
