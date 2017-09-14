Imports System.Collections.Generic
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Text
Imports System.Xml
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Control
Imports log4net.Core

Partial Public Class QueryBuilderOffline
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(sender As Object, e As EventArgs)

        If Not Page.IsPostBack Then
            QueryBuilderControl1.QueryBuilder.SQL = "SELECT SUM(o.Payment) AS Total, c.Name, c.Address" & vbCr & vbLf & "FROM MyDB.MySchema.Orders o" & vbCr & vbLf & vbTab & "INNER JOIN MyDB.MySchema.Customers c ON o.CustomerID = c.CustomerID" & vbCr & vbLf & vbTab & vbTab & "LEFT JOIN MyDB.MySchema.OrderDetails od ON od.OrderID = o.OrderID" & vbCr & vbLf & "WHERE" & vbCr & vbLf & vbTab & "o.Special = 1" & vbCr & vbLf & vbTab & "OR" & vbCr & vbLf & vbTab & vbTab & "o.CustomerID = c.CustomerID" & vbCr & vbLf & vbTab & vbTab & "AND" & vbCr & vbLf & vbTab & vbTab & "o.OrderID > 0" & vbCr & vbLf & "GROUP BY o.CustomerID, c.Name, c.Address" & vbCr & vbLf & "HAVING SUM(o.Payment) > 1000" & vbCr & vbLf & "ORDER BY Total DESC, c.Name"
            textBox1.Text = QueryBuilderControl1.PlainTextSQLBuilder.SQL
        Else
            Try
                QueryBuilderControl1.QueryBuilder.SQL = textBox1.Text
            Catch ex As System.Exception
                tbResult.Text = ex.Message
                Return
            End Try
        End If
    End Sub

    Public Sub SleepModeChanged(sender As Object, e As EventArgs)
        Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
    End Sub

    Public Sub QueryBuilderControl1_Init(sender As Object, e As EventArgs)
        ' Get instance of QueryBuilder
        Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
        If queryBuilder.SyntaxProvider Is Nothing Then
            ' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
            queryBuilder.BehaviorOptions.AllowSleepMode = True
            queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()
        End If
        queryBuilder.OfflineMode = True
        queryBuilder.MetadataContainer.ImportFromXML(Page.Server.MapPath("Northwind.xml"))
        queryBuilder.MetadataStructure.Refresh()
    End Sub

    Protected Sub QueryBuilderControl1_SQLUpdated(sender As Object, e As EventArgs, clientdata As ClientData)
    End Sub

    Protected Sub buttonAnalyzeQuery_Click(sender As Object, e As EventArgs)
        Dim queryBuilder = QueryBuilderControl1.QueryBuilder
        Try
            queryBuilder.SQL = textBox1.Text
        Catch ex As System.Exception
            tbResult.Text = ex.Message
            Return
        End Try
        AnalyzeQuery()
    End Sub

    Protected Sub buttonQueryStatistics_Click(sender As Object, e As EventArgs)
        Dim queryBuilder = QueryBuilderControl1.QueryBuilder
        Try
            queryBuilder.SQL = textBox1.Text
        Catch ex As System.Exception
            tbResult.Text = ex.Message
            Return
        End Try
        QueryStatistics()
    End Sub

    Protected Sub buttonAnalyzeMetadataContainer_Click(sender As Object, e As EventArgs)
        Dim queryBuilder = QueryBuilderControl1.QueryBuilder
        Try
            queryBuilder.SQL = textBox1.Text
        Catch ex As System.Exception
            tbResult.Text = ex.Message
            Return
        End Try
        AnalyzeMetadataContainerContent()
    End Sub

    Protected Sub buttonAnalyzeWhereClause_Click(sender As Object, e As EventArgs)

        Dim queryBuilder = QueryBuilderControl1.QueryBuilder
        Try
            queryBuilder.SQL = textBox1.Text
        Catch ex As System.Exception
            tbResult.Text = ex.Message
            Return
        End Try

        AnalyzeWhereClause()

    End Sub

    Public Sub AnalyzeWhereClause()
        Dim queryBuilder = QueryBuilderControl1.QueryBuilder


        ' find a union-subquery to analyze
        Dim [select] As SQLSubQuerySelectExpression = FindFirstSelect(queryBuilder.ResultQueryAST)

        If [select].Where IsNot Nothing Then


            tbResult.Text = LoadCondition(0, [select].Where)
        End If
    End Sub

    Private Function LoadCondition(level As Integer, condition As SQLExpressionItem) As String
        Dim result As String = ""
        Dim s As [String]

        If condition Is Nothing Then
            ' NULL reference protection
            Append(result, level, "--null--")
        ElseIf TypeOf condition Is SQLExpressionBrackets Then
            ' The condition is actually the "Brackets" structural node.
            ' Create the "brackets" tree node and load the brackets content as children tree nodes.
            Append(result, level, "( )")
            Append(result, LoadCondition(level + 1, DirectCast(condition, SQLExpressionBrackets).LExpression))
        ElseIf TypeOf condition Is SQLExpressionOr Then
            ' The condition is actually the "OR Collection" structural node.
            ' Create the "OR" tree node and load all items of the collection as children tree nodes.
            Append(result, level, "OR")

            For i As Integer = 0 To DirectCast(condition, SQLExpressionOr).Count - 1
                Append(result, LoadCondition(level + 1, DirectCast(condition, SQLExpressionOr)(i)))
            Next
        ElseIf TypeOf condition Is SQLExpressionAnd Then
            ' The condition is actually the "AND Collection" structural node.
            ' Create the "AND" tree node and load all items of the collection as children tree nodes.
            Append(result, level, "AND")

            For i As Integer = 0 To DirectCast(condition, SQLExpressionAnd).Count - 1
                Append(result, LoadCondition(level + 1, DirectCast(condition, SQLExpressionAnd)(i)))
            Next
        ElseIf TypeOf condition Is SQLExpressionNot Then
            ' The condition is actually the "NOT" structural node.
            ' Create the "NOT" tree node and load content of the "NOT" operator as children tree nodes.
            Append(result, level, "NOT")
            Append(result, LoadCondition(level + 1, DirectCast(condition, SQLExpressionNot).LExpression))
        ElseIf TypeOf condition Is SQLExpressionOperatorBinary Then
            ' The condition is actually the "Binary Operator" structural node.
            ' Create a tree node containing the operator value and two leaf nodes with the operator arguments.
            s = DirectCast(condition, SQLExpressionOperatorBinary).OperatorObj.OperatorName
            Append(result, level, s)
            ' left argument of the binary operator
            Append(result, LoadCondition(level + 1, DirectCast(condition, SQLExpressionOperatorBinary).LExpression))
            ' right argument of the binary operator
            Append(result, LoadCondition(level + 1, DirectCast(condition, SQLExpressionOperatorBinary).RExpression))
        Else
            ' Other types of structureal nodes.
            ' Just show them as a text.
            s = condition.GetSQL(condition.SQLContext.SQLBuilderExpression)
            Append(result, level, s)
        End If

        Return result
    End Function

    Private Function FindFirstSelect(queryGroup As SQLSubQueryExpressions) As SQLSubQuerySelectExpression
        Dim result As SQLSubQuerySelectExpression = Nothing

        For i As Integer = 0 To queryGroup.Count - 1
            If TypeOf queryGroup(i) Is SQLSubQuerySelectExpression Then
                Return DirectCast(queryGroup(i), SQLSubQuerySelectExpression)
            ElseIf TypeOf queryGroup(i) Is SQLSubQueryExpressions Then
                result = FindFirstSelect(DirectCast(queryGroup(i), SQLSubQueryExpressions))

                If result IsNot Nothing Then
                    Return result
                End If
            End If
        Next

        Return result
    End Function

    Public Sub AnalyzeMetadataContainerContent()

        Dim queryBuilder = QueryBuilderControl1.QueryBuilder

        Dim s As StringBuilder
        Dim tables As New StringBuilder("Tables:" & vbCr & vbLf)
        Dim views As New StringBuilder("Views:" & vbCr & vbLf)

        Dim metadataObjects As IList(Of MetadataObject) = queryBuilder.MetadataContainer.FindItems(Of MetadataObject)(MetadataType.Objects)

        For Each mo As MetadataObject In metadataObjects
            If mo.Type = MetadataType.Table Then
                s = tables
            ElseIf mo.Type = MetadataType.View Then
                s = views
            Else
                ' process only tables and views
                s = Nothing
            End If

            If s IsNot Nothing Then
                ' append object name
                s.Append(vbTab & mo.NameFull & vbCr & vbLf)

                ' append fields
                For Each referencingField As MetadataField In mo.Items.Fields
                    ' append field name
                    s.Append(vbTab & vbTab & referencingField.Name)

                    ' search relations for this field
                    For Each fk As MetadataForeignKey In mo.Items.ForeignKeys
                        For i As Integer = 0 To fk.Fields.Count - 1
                            If fk.Fields(i) = referencingField.Name Then
                                s.Append(" -- > " & fk.ReferencedObject.NameFull & "." & fk.ReferencedFields(i))
                            End If
                        Next
                    Next

                    s.Append(vbCr & vbLf)
                Next
            End If
        Next

        tbResult.Text = tables.ToString() & vbCr & vbLf & views.ToString()
    End Sub

    Public Sub QueryStatistics()
        Dim queryBuilder = QueryBuilderControl1.QueryBuilder

        Dim stats As String = ""

        Dim qs As QueryStatistics = queryBuilder.QueryStatistics

        stats = "Used Objects (" & qs.UsedDatabaseObjects.Count & "):" & vbCr & vbLf
        For i As Integer = 0 To qs.UsedDatabaseObjects.Count - 1
            stats += vbCr & vbLf & qs.UsedDatabaseObjects(i).ObjectName.QualifiedName
        Next

        stats += vbCr & vbLf & vbCr & vbLf & "Used Columns (" & qs.UsedDatabaseObjectFields.Count & "):" & vbCr & vbLf
        For i As Integer = 0 To qs.UsedDatabaseObjectFields.Count - 1
            stats += vbCr & vbLf & qs.UsedDatabaseObjectFields(i).FullName.QualifiedName
        Next

        stats += vbCr & vbLf & vbCr & vbLf & "Output Expressions (" & qs.OutputColumns.Count & "):" & vbCr & vbLf
        For i As Integer = 0 To qs.OutputColumns.Count - 1
            stats += vbCr & vbLf & qs.OutputColumns(i).Expression
        Next

        tbResult.Text = stats
    End Sub

    Public Sub AnalyzeQuery()
        Dim queryBuilder = QueryBuilderControl1.QueryBuilder

        ' if you want to hide default database and schema from object names, uncomment lines below
        'queryBuilder.MetadataContainer.DefaultDatabaseNameStr = "MyDB";
        'queryBuilder.MetadataContainer.DefaultSchemaNamesStr = "MySchema";

        ' fill the tree with query structure

        ' Note: if you don't need to analyze the whole query structure
        ' with all sub-queries and unions, you may simply use the
        ' queryBuilder.Query.FirstSelect to get access to the main query.
        ' You may uncomment the next line and comment the line aftet that
        ' to load information about main query only.

        'LoadUnionSubQuery(f.treeView1.Nodes, queryBuilder.Query.FirstSelect);

        Dim result = LoadQuery(0, queryBuilder.Query)

        tbResult.Text = result

        ' show the XML structure of the query in the browser control

        Dim tempFile As String = System.IO.Path.GetTempPath() & "temp.xml"

        Dim writer As New System.Xml.XmlTextWriter(tempFile, Nothing)
        writer.Formatting = System.Xml.Formatting.Indented

        Dim doc As New System.Xml.XmlDocument()
        doc.LoadXml(queryBuilder.Query.StructureXml)
        doc.WriteTo(writer)
        writer.Flush()
        writer.Close()


        File.Delete(tempFile)
    End Sub

    Private Sub Append(ByRef src As String, str As String)
        Append(src, 0, str)
    End Sub
    Private Sub Append(ByRef src As String, level As Integer, str As String)
        For i As Integer = 0 To level - 1
            src += "  "
        Next
        src += str & vbLf
    End Sub

    Private Function LoadQuery(level As Integer, q As SubQuery) As String
        Dim result = ""
        Append(result, level, q.Caption)

        ' union parts
        If q.Count = 1 AndAlso TypeOf q(0) Is UnionSubQuery Then
            Append(result, LoadUnionSubQuery(level + 1, DirectCast(q(0), UnionSubQuery)))
        Else
            Append(result, LoadUnionGroup(level + 1, q, "Unions"))
        End If

        ' subqueries
        For i As Integer = 0 To q.SubQueryCount - 1
            Append(result, LoadQuery(level + 1, DirectCast(q.SubQueries(i), SubQuery)))
        Next

        Return result
    End Function

    Private Function LoadUnionGroup(level As Integer, unionGroup As UnionGroup, caption As [String]) As String
        Dim unionNode As String = ""

        Append(unionNode, level, caption)

        For i As Integer = 0 To unionGroup.Count - 1
            If TypeOf unionGroup(i) Is UnionGroup Then
                Append(unionNode, LoadUnionGroup(level + 1, DirectCast(unionGroup(i), UnionGroup), "( )"))
            ElseIf TypeOf unionGroup(i) Is UnionSubQuery Then
                Dim unionSubQuery As UnionSubQuery = DirectCast(unionGroup(i), UnionSubQuery)
                Append(unionNode, level, unionSubQuery.GetResultSQL(unionSubQuery.SQLContext.SQLBuilderExpressionForServer))
                Append(unionNode, LoadUnionSubQuery(level + 1, unionSubQuery))
            End If
        Next
        Return unionNode
    End Function

    Private Function LoadUnionSubQuery(level As Integer, unionSubQuery As UnionSubQuery) As String
        ' Notice: At this stage you can retrieve whole sub-query or it's different parts in string form:
        ' whole sub-query:
        '			String unionSubqueryString = unionSubQuery.GetResultSQL();
        ' parts:
        ' 			String selectListString = unionSubQuery.SelectListString;
        ' 			String fromString = unionSubQuery.FromClauseString;
        ' 			String whereString = unionSubQuery.WhereClauseString;
        ' 			String groupByString = unionSubQuery.GroupByClauseString;
        ' 			String havingString = unionSubQuery.HavingClauseString;
        ' 			String orderByString = unionSubQuery.OrderByClauseString;


        Dim listNode = ""
        Append(listNode, level, "Fields")

        ' Get structure of union subquery components:

        ' SELECT list

        Dim QueryColumnList As QueryColumnList = unionSubQuery.QueryColumnList

        For i As Integer = 0 To QueryColumnList.Count - 1
            Dim QueryColumnListItem As QueryColumnListItem = QueryColumnList(i)

            If QueryColumnListItem.[Select] Then
                Dim s As String = QueryColumnListItem.ExpressionString

                If QueryColumnListItem.Aggregate IsNot Nothing Then
                    s = QueryColumnListItem.AggregateString & "(" & s & ")"
                End If

                If Not [String].IsNullOrEmpty(QueryColumnListItem.AliasString) Then
                    s += " " & QueryColumnListItem.AliasString
                End If

                Append(listNode, level + 1, s)
            End If
        Next

        ' FROM clause

        If unionSubQuery.FromClause.Count > 0 Then
            Append(listNode, level, "FROM")

            Append(listNode, LoadFromGroup(level + 1, unionSubQuery.FromClause))
        End If

        ' WHERE clause

        ' compute ORs count
        Dim orsCount As Integer = 0

        For i As Integer = 0 To QueryColumnList.Count - 1
            orsCount = Math.Max(QueryColumnList(i).ConditionCount, orsCount)
        Next

        ' collect conditions
        If orsCount > 0 Then
            Append(listNode, level, "WHERE")

            For [or] As Integer = 0 To orsCount - 1
                ' new OR item
                Append(listNode, level + 1, "OR")

                For i As Integer = 0 To QueryColumnList.Count - 1
                    Dim QueryColumnListItem As QueryColumnListItem = QueryColumnList(i)

                    If QueryColumnListItem.ConditionType = ConditionType.Where AndAlso Not [String].IsNullOrEmpty(QueryColumnListItem.ConditionStrings([or])) Then
                        ' add AND item
                        Append(listNode, level + 2, "AND " & QueryColumnListItem.ExpressionString & " " & QueryColumnListItem.ConditionStrings([or]))
                    End If
                Next
            Next
        End If

        ' GROUP BY clause

        ' compute GROUPBYs count
        Dim groupbyCount As Integer = 0

        For i As Integer = 0 To QueryColumnList.Count - 1
            If QueryColumnList(i).Grouping Then
                groupbyCount += 1
            End If
        Next

        If groupbyCount > 0 Then
            Append(listNode, level, "GROUP BY")

            For i As Integer = 0 To QueryColumnList.Count - 1
                If QueryColumnList(i).Grouping Then
                    Append(listNode, level + 1, QueryColumnList(i).ExpressionString)
                End If
            Next
        End If

        ' HAVING clause
        Dim havingsCount As Integer = 0

        For [or] As Integer = 0 To orsCount - 1
            For i As Integer = 0 To QueryColumnList.Count - 1
                Dim QueryColumnListItem As QueryColumnListItem = QueryColumnList(i)

                If QueryColumnListItem.ConditionType = ConditionType.Having AndAlso Not [String].IsNullOrEmpty(QueryColumnListItem.ConditionStrings([or])) Then
                    havingsCount += 1
                End If
            Next
        Next

        If havingsCount > 0 Then
            Append(listNode, level, "HAVING")

            For [or] As Integer = 0 To orsCount - 1
                ' new OR item
                Append(listNode, level + 1, "OR")

                For i As Integer = 0 To QueryColumnList.Count - 1
                    Dim QueryColumnListItem As QueryColumnListItem = QueryColumnList(i)

                    If QueryColumnListItem.ConditionType = ConditionType.Having AndAlso Not [String].IsNullOrEmpty(QueryColumnListItem.ConditionStrings([or])) Then
                        ' add AND item
                        Append(listNode, level + 2, "AND " & QueryColumnListItem.ExpressionString & " " & QueryColumnListItem.ConditionStrings([or]))
                    End If
                Next
            Next
        End If

        ' ORDER BY clause

        ' collect ORDERBYs items

        Dim orderbyList As New List(Of QueryColumnListItem)()

        For i As Integer = 0 To QueryColumnList.Count - 1
            If Not [String].IsNullOrEmpty(QueryColumnList(i).SortOrderString) Then
                orderbyList.Add(QueryColumnList(i))
            End If
        Next

        If orderbyList.Count > 0 Then
            ' sort ORDER BY items by theirs index
            orderbyList.Sort(AddressOf CompareOrderByItems)

            Append(listNode, level, "ORDER BY")

            For i As Integer = 0 To orderbyList.Count - 1
                Dim s As String = orderbyList(i).ExpressionString & " " & orderbyList(i).SortTypeString
                Append(listNode, level + 1, s)
            Next
        End If
        Return listNode
    End Function

    Private Shared Function CompareOrderByItems(x As QueryColumnListItem, y As QueryColumnListItem) As Integer
        If x.SortOrder > y.SortOrder Then
            Return 1
        End If

        If x.SortOrder < y.SortOrder Then
            Return -1
        End If

        Return 0
    End Function

    Private Function LoadFromGroup(level As Integer, f As DataSourceGroup) As String
        Dim result As String = ""
        For i As Integer = 0 To f.Count - 1
            If TypeOf f(i) Is DataSourceGroup Then
                LoadFromGroup(level, DirectCast(f(i), DataSourceGroup))
            Else
                Append(result, level, f(i).GetResultSQL(f(i).SQLContext.SQLBuilderExpressionForServer))
            End If
        Next

        If TypeOf f Is DataSourceGroup Then
            ' FROM clause
            If f.LinkCount > 0 Then
                Append(result, level, "LINKS")

                For i As Integer = 0 To f.LinkCount - 1
                    Dim join As String

                    If f.Links(i).LeftType = LinkSideType.Inner Then
                        If f.Links(i).RightType = LinkSideType.Inner Then
                            join = "INNER JOIN"
                        Else
                            join = "RIGHT JOIN"
                        End If
                    ElseIf f.Links(i).RightType = LinkSideType.Inner Then
                        join = "LEFT JOIN"
                    Else
                        join = "FULL JOIN"
                    End If

                    join += " " & f.Links(i).LinkExpression.GetSQL(f.SQLContext.SQLBuilderExpressionForServer)

                    Append(result, level + 1, join)
                Next
            End If
        End If
        Return result
    End Function
End Class
