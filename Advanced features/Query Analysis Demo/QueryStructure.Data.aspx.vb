Imports System.Collections
Imports System.Configuration
Imports System.Data
Imports System.Text
Imports ActiveDatabaseSoftware.ActiveQueryBuilder


Partial Public Class QueryStructure
	Friend Class UnionSubQueryExchangeClass
		Public SelectedExpressions As String
		Public DataSources As String
		Public Links As String
		Public Where As String
	End Class

	Private Class ExchangeClass
		Public Statistics As String
		Public SubQueries As String
		Public QueryStructure As String
		Public UnionSubQuery As UnionSubQueryExchangeClass
	End Class


	Private Function GetQueryStatistic(qs As QueryStatistics) As String
		Dim stats As String = ""

		stats = "<b>Used Objects (" & qs.UsedDatabaseObjects.Count & "):</b><br/>"
		For i As Integer = 0 To qs.UsedDatabaseObjects.Count - 1
			stats += "<br />" & qs.UsedDatabaseObjects(i).ObjectName.QualifiedName
		Next

		stats += "<br /><br />" & "<b>Used Columns (" & qs.UsedDatabaseObjectFields.Count & "):</b><br />"
		For i As Integer = 0 To qs.UsedDatabaseObjectFields.Count - 1
			stats += "<br />" & qs.UsedDatabaseObjectFields(i).FullName.QualifiedName
		Next

		stats += "<br /><br />" & "<b>Output Expressions (" & qs.OutputColumns.Count & "):</b><br />"
		For i As Integer = 0 To qs.OutputColumns.Count - 1
			stats += "<br />" & qs.OutputColumns(i).Expression
		Next
		Return stats
	End Function

	Private Function DumpQueryStructureInfo(subQuery As SubQuery) As String
		Dim stringBuilder = New StringBuilder()
		DumpUnionGroupInfo(stringBuilder, "", subQuery)
		Return stringBuilder.ToString()
	End Function

	Private Sub DumpUnionGroupInfo(stringBuilder As StringBuilder, indent As String, unionGroup As UnionGroup)
		Dim children As QueryBase() = GetUnionChildren(unionGroup)

		For Each child As QueryBase In children
			If stringBuilder.Length > 0 Then
				stringBuilder.AppendLine("<br />")
			End If

			If TypeOf child Is UnionSubQuery Then
				' UnionSubQuery is a leaf node of query structure.
				' It represent a single SELECT statement in the tree of unions
				DumpUnionSubQueryInfo(stringBuilder, indent, DirectCast(child, UnionSubQuery))
			Else
				' UnionGroup is a tree node.
				' It contains one or more leafs of other tree nodes.
				' It represent a root of the subquery of the union tree or a
				' parentheses in the union tree.
				unionGroup = DirectCast(child, UnionGroup)

				stringBuilder.AppendLine(indent & unionGroup.UnionOperatorFull & "group: [")
				DumpUnionGroupInfo(stringBuilder, indent & "&nbsp;&nbsp;&nbsp;&nbsp;", unionGroup)
				stringBuilder.AppendLine(indent & "]<br />")
			End If
		Next
	End Sub

	Private Sub DumpUnionSubQueryInfo(stringBuilder As StringBuilder, indent As String, unionSubQuery As UnionSubQuery)
		Dim sql As String = unionSubQuery.GetResultSQL()

		stringBuilder.AppendLine(indent & unionSubQuery.UnionOperatorFull & ": " & sql & "<br />")
	End Sub

	Private Function GetUnionChildren(unionGroup As UnionGroup) As QueryBase()
		Dim result As New ArrayList()

		For i As Integer = 0 To unionGroup.Count - 1
			result.Add(unionGroup(i))
		Next

		Return DirectCast(result.ToArray(GetType(QueryBase)), QueryBase())
	End Function

	Public Function DumpSelectedExpressionsInfoFromUnionSubQuery(unionSubQuery As UnionSubQuery) As String
		Dim stringBuilder = New StringBuilder()
		' get list of CriteriaItems
		Dim criteriaList As QueryColumnList = unionSubQuery.QueryColumnList

		' dump all items
		For i As Integer = 0 To criteriaList.Count - 1
			Dim criteriaItem As QueryColumnListItem = criteriaList(i)

			' only items have .Select property set to True goes to SELECT list
			If Not criteriaItem.[Select] Then
				Continue For
			End If

			' separator
			If stringBuilder.Length > 0 Then
				stringBuilder.AppendLine("<br />")
			End If

			DumpSelectedExpressionInfo(stringBuilder, criteriaItem)
		Next
		Return stringBuilder.ToString()
	End Function

	Private Sub DumpSelectedExpressionInfo(stringBuilder As StringBuilder, selectedExpression As QueryColumnListItem)
		' write full sql fragment of selected expression
		stringBuilder.AppendLine(selectedExpression.ExpressionString & "<br />")

		' write alias
		If Not [String].IsNullOrEmpty(selectedExpression.AliasString) Then
			stringBuilder.AppendLine("&nbsp;&nbsp;alias: " & selectedExpression.AliasString & "<br />")
		End If

		' write datasource reference (if any)
		If selectedExpression.ExpressionDatasource IsNot Nothing Then
			stringBuilder.AppendLine("&nbsp;&nbsp;datasource: " & selectedExpression.ExpressionDatasource.GetResultSQL() & "<br />")
		End If

		' write metadata information (if any)
		If selectedExpression.ExpressionField IsNot Nothing Then
			Dim field As MetadataField = selectedExpression.ExpressionField
			stringBuilder.AppendLine("&nbsp;&nbsp;field name: " & field.Name & "<br />")

			Dim s As String = [Enum].GetName(GetType(DbType), field.FieldType)
			stringBuilder.AppendLine("&nbsp;&nbsp;field type: " & s & "<br />")
		End If
	End Sub

	Private Sub DumpDataSourcesInfo(stringBuilder As StringBuilder, dataSources As ArrayList)
		For i As Integer = 0 To dataSources.Count - 1
			If stringBuilder.Length > 0 Then
				stringBuilder.AppendLine("<br />")
			End If

			DumpDataSourceInfo(stringBuilder, DirectCast(dataSources(i), DataSource))
		Next
	End Sub

	Public Function DumpDataSourcesInfoFromUnionSubQuery(unionSubQuery As UnionSubQuery) As String
		Dim stringBuilder As New StringBuilder()
		DumpDataSourcesInfo(stringBuilder, GetDataSourceList(unionSubQuery))
		Return stringBuilder.ToString()
	End Function

	Private Function GetDataSourceList(unionSubQuery As UnionSubQuery) As ArrayList
		Dim list As New ArrayList()

		unionSubQuery.FromClause.GetDatasourceByClass(GetType(DataSource), list)

		Return list
	End Function

	Private Sub DumpDataSourceInfo(stringBuilder As StringBuilder, dataSource As DataSource)
		' write full sql fragment
		stringBuilder.AppendLine("<b>" & dataSource.GetResultSQL() & "</b><br />")

		' write alias
		stringBuilder.AppendLine("&nbsp;&nbsp;alias: " & dataSource.[Alias] & "<br />")

		' write referenced MetadataObject (if any)
		If dataSource.MetadataObject IsNot Nothing Then
			stringBuilder.AppendLine("&nbsp;&nbsp;ref: " & dataSource.MetadataObject.Name & "<br />")
		End If

		' write subquery (if datasource is actually a derived table)
		If TypeOf dataSource Is DataSourceQuery Then
			stringBuilder.AppendLine("&nbsp;&nbsp;subquery sql: " & DirectCast(dataSource, DataSourceQuery).Query.GetResultSQL() & "<br />")
		End If

		' write fields
		Dim fields As String = [String].Empty

		For i As Integer = 0 To dataSource.Metadata.Count - 1
			If fields.Length > 0 Then
				fields += ", "
			End If

			fields += dataSource.Metadata(i).Name
		Next

		stringBuilder.AppendLine("&nbsp;&nbsp;fields (" & dataSource.Metadata.Count.ToString() & "): " & fields & "<br />")
	End Sub

	Private Sub DumpLinkInfo(stringBuilder As StringBuilder, link As Link)
		' write full sql fragment of link expression
		stringBuilder.AppendLine(link.LinkExpression.GetSQL(link.SQLContext.SQLBuilderExpression) & "<br />")

		' write information about left side of link
		stringBuilder.AppendLine("&nbsp;&nbsp;left datasource: " & link.LeftDatasource.GetResultSQL() & "<br />")

		If link.LeftType = LinkSideType.Inner Then
			stringBuilder.AppendLine("&nbsp;&nbsp;left type: Inner" & "<br />")
		Else
			stringBuilder.AppendLine("&nbsp;&nbsp;left type: Outer" & "<br />")
		End If

		' write information about right side of link
		stringBuilder.AppendLine("&nbsp;&nbsp;right datasource: " & link.RightDatasource.GetResultSQL() & "<br />")

		If link.RightType = LinkSideType.Inner Then
			stringBuilder.AppendLine("&nbsp;&nbsp;lerightft type: Inner" & "<br />")
		Else
			stringBuilder.AppendLine("&nbsp;&nbsp;right type: Outer" & "<br />")
		End If
	End Sub

	Private Sub DumpLinksInfo(stringBuilder As StringBuilder, links As ArrayList)
		For i As Integer = 0 To links.Count - 1
			If stringBuilder.Length > 0 Then
				stringBuilder.AppendLine("<br />")
			End If

			DumpLinkInfo(stringBuilder, DirectCast(links(i), Link))
		Next
	End Sub

	Private Function GetLinkList(unionSubQuery As UnionSubQuery) As ArrayList
		Dim links As New ArrayList()

		unionSubQuery.FromClause.GetLinksRecursive(links)

		Return links
	End Function

	Public Function DumpLinksInfoFromUnionSubQuery(unionSubQuery As UnionSubQuery) As String
		Dim stringBuilder = New StringBuilder()
		DumpLinksInfo(stringBuilder, GetLinkList(unionSubQuery))
		Return stringBuilder.ToString()
	End Function

	Public Sub DumpWhereInfo(stringBuilder As StringBuilder, where As SQLExpressionItem)
		DumpExpression(stringBuilder, "", where)
	End Sub

	Private Sub DumpExpression(stringBuilder As StringBuilder, indent As String, expression As SQLExpressionItem)
		Const cIndentInc As String = "&nbsp;&nbsp;&nbsp;&nbsp;"

		Dim newIndent As String = indent & cIndentInc

		If expression Is Nothing Then
			' NULL reference protection
			stringBuilder.AppendLine(indent & "--nil--" & "<br />")
		ElseIf TypeOf expression Is SQLExpressionBrackets Then
			' Expression is actually the brackets query structure node.
			' Create the "brackets" tree node and load content of
			' the brackets as children of the node.
			stringBuilder.AppendLine(indent & "()" & "<br />")
			DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionBrackets).LExpression)
		ElseIf TypeOf expression Is SQLExpressionOr Then
			' Expression is actually the "OR" query structure node.
			' Create the "OR" tree node and load all items of
			' the "OR" collection as children of the tree node.
			stringBuilder.AppendLine(indent & "OR" & "<br />")

			For i As Integer = 0 To DirectCast(expression, SQLExpressionOr).Count - 1
				DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionOr)(i))
			Next
		ElseIf TypeOf expression Is SQLExpressionAnd Then
			' Expression is actually the "AND" query structure node.
			' Create the "AND" tree node and load all items of
			' the "AND" collection as children of the tree node.
			stringBuilder.AppendLine(indent & "AND" & "<br />")

			For i As Integer = 0 To DirectCast(expression, SQLExpressionAnd).Count - 1
				DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionAnd)(i))
			Next
		ElseIf TypeOf expression Is SQLExpressionNot Then
			' Expression is actually the "NOT" query structure node.
			' Create the "NOT" tree node and load content of
			' the "NOT" operator as children of the tree node.
			stringBuilder.AppendLine(indent & "NOT" & "<br />")
			DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionNot).LExpression)
		ElseIf TypeOf expression Is SQLExpressionOperatorBinary Then
			' Expression is actually the "BINARY OPERATOR" query structure node.
			' Create a tree node containing the operator value and
			' two leaf nodes with the operator arguments.
			Dim s As String = DirectCast(expression, SQLExpressionOperatorBinary).OperatorObj.OperatorName
			stringBuilder.AppendLine(indent & s & "<br />")
			' left argument of the binary operator
			DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionOperatorBinary).LExpression)
			' right argument of the binary operator
			DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionOperatorBinary).RExpression)
		Else
			' other type of AST nodes - out as a text
			Dim s As String = expression.GetSQL(expression.SQLContext.SQLBuilderExpression)
			stringBuilder.AppendLine(indent & s & "<br />")
		End If
	End Sub

	Private Function GetWhereInfo(unionSubQuery As UnionSubQuery) As String
		Dim stringBuilder As New StringBuilder()

		Dim unionSubQueryAst As SQLSubQuerySelectExpression = unionSubQuery.ResultQueryAST

		Try
			If unionSubQueryAst.Where IsNot Nothing Then
				DumpWhereInfo(stringBuilder, unionSubQueryAst.Where)
			End If
		Finally
			unionSubQueryAst.Dispose()
		End Try

		Return stringBuilder.ToString()
	End Function

	Public Function DumpSubQueries(queryBuilder As QueryBuilder) As String
		Dim stringBuilder As New StringBuilder()
		DumpSubQueriesInfo(stringBuilder, queryBuilder)
		Return stringBuilder.ToString()
	End Function

	Private Sub DumpSubQueryInfo(stringBuilder As StringBuilder, index As Integer, subQuery As SubQuery)
		Dim sql As String = subQuery.GetResultSQL()

		stringBuilder.AppendLine(index.ToString() & ": " & sql & "<br />")
	End Sub

	Public Sub DumpSubQueriesInfo(stringBuilder As StringBuilder, queryBuilder As QueryBuilder)
		For i As Integer = 0 To queryBuilder.GetSubQueryList().Count - 1
			If stringBuilder.Length > 0 Then
				stringBuilder.AppendLine("<br />")
			End If

			DumpSubQueryInfo(stringBuilder, i, queryBuilder.SubQueries(i))
		Next
	End Sub

End Class