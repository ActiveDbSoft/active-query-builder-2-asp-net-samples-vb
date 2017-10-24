Imports System.Configuration
Imports System.IO
Imports System.Xml
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server

Namespace Samples
	Public Partial Class VirtualObjectsAndFields
		Inherits System.Web.UI.UserControl
		Private Class ExchangeClass
			Public SQL As String
			Public AlternateSQL As String
		End Class

		Private plainTextSQLBuilder As PlainTextSQLBuilder
		Private plainTextSQLBuilder2 As PlainTextSQLBuilder

	Public Sub Page_Load(sender As Object, e As EventArgs)
		If Not Page.IsPostBack Then
			SQLEditor1.SQL = "SELECT dummy_alias._qry_OrderId_plus_1, dummy_alias._qry_CustomerName FROM Orders_qry dummy_alias"
		End If
	End Sub


Public Sub QueryBuilderControl1_OnSQLUpdated(sender As Object, e As EventArgs, clientdata AS ClientData)
            SessionStore.Current.Exchange.Data = New ExchangeClass() With { _
             .SQL = plainTextSQLBuilder.SQL, _
             .AlternateSQL = plainTextSQLBuilder2.SQL _
            }
        End Sub


        Public Sub SleepModeChanged(sender As Object, e As EventArgs)
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		If queryBuilder.SleepMode Then StatusBar1.Message.Error("Unsupported SQL statement.")
	End Sub

Public Sub QueryBuilderControl1_Init(ByVal sender As Object, ByVal e As EventArgs)
            ' Get instance of QueryBuilder
            Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
            ' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
		queryBuilder.BehaviorOptions.AllowSleepMode = True
		queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

            plainTextSQLBuilder = New PlainTextSQLBuilder() With { _
             .QueryBuilder = queryBuilder, _
             .KeywordFormat = KeywordFormat.UpperCase _
            }
			plainTextSQLBuilder2 = New PlainTextSQLBuilder() With { _
			 .QueryBuilder = queryBuilder, _
			 .KeywordFormat = KeywordFormat.UpperCase, _
			 .UseAltNames = False, _
			 .ExpandVirtualObjects = True _
			}

            queryBuilder.OfflineMode = True
            ' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/XmlMetaData] key
            ' Pass loaded XML to QueryBuilder component
            Try
                queryBuilder.MetadataContainer.ImportFromXML(Server.MapPath(ConfigurationManager.AppSettings("XmlMetaData")))
                queryBuilder.MetadataStructure.Refresh()
            StatusBar1.Message.Information("Metadata loaded")
            Catch ex As Exception
                Dim message As String = 
                "Error loading metadata from the database." +
                "Check the 'configuration\\connectionStrings' key in the [web.config] file."
			Logger.[Error](message, ex)
			StatusBar1.Message.Error(message & " Check log.txt for details.")
            End Try

            Dim o As MetadataObject
            Dim f As MetadataField

            ' Virtual fields for real object
            ' ===========================================================================
			o = queryBuilder.MetadataContainer.FindItem(Of MetadataObject)("Orders")

			' first test field - simple expression
			f = o.AddField("_OrderId_plus_1")
			f.Expression = "orders.OrderId + 1"

			' second test field - correlated sub-query
			f = o.AddField("_CustomerName")
			f.Expression = "(select c.CompanyName from Customers c where c.CustomerId = orders.CustomerId)"

			' Virtual object (table) with virtual fields
			' ===========================================================================

			o = queryBuilder.MetadataContainer.AddTable("Orders_tbl")
			o.Expression = "Orders"

			' first test field - simple expression
			f = o.AddField("_tbl_OrderId_plus_1")
			f.Expression = "Orders_tbl.OrderId + 1"

			' second test field - correlated sub-query
			f = o.AddField("_tbl_CustomerName")
			f.Expression = "(select c.CompanyName from Customers c where c.CustomerId = Orders_tbl.CustomerId)"

			' Virtual object (sub-query) with virtual fields
			' ===========================================================================

			o = queryBuilder.MetadataContainer.AddTable("Orders_qry")
			o.Expression = "(select OrderId, CustomerId, OrderDate from Orders) as dummy_alias"

			' first test field - simple expression
			f = o.AddField("_qry_OrderId_plus_1")
			f.Expression = "Orders_qry.OrderId + 1"

			' second test field - correlated sub-query
			f = o.AddField("_qry_CustomerName")
			f.Expression = "(select c.CompanyName from Customers c where c.CustomerId = Orders_qry.CustomerId)"

			' kick queryBuilder to initialize its metadata tree
			queryBuilder.InitializeDatabaseSchemaTree()

			queryBuilder.SQL = "SELECT dummy_alias._qry_OrderId_plus_1, dummy_alias._qry_CustomerName FROM Orders_qry dummy_alias"
        End Sub

        Private Function LoadXml(ByVal xmlFileName As String) As XmlDocument
            Dim doc As New XmlDocument()
            Try
                doc.Load(xmlFileName)
            Catch ex As Exception
                Dim message As String
                If TypeOf ex Is FileNotFoundException Then
                    message = String.Format("Can't find XML file '{0}'.", xmlFileName)
                    Logger.[Error](message, ex)
                    StatusBar1.Message.Error(message & " Check log.txt for details.")
                ElseIf TypeOf ex Is IOException Then
                    message = String.Format("Can't read XML file '{0}'.", xmlFileName)
                    Logger.[Error](message, ex)
                    StatusBar1.Message.Error(message & " Check log.txt for details.")
                ElseIf TypeOf ex Is XmlException Then
                    message = String.Format("Can't parse XML file '{0}'.", xmlFileName)
                    Logger.[Error](message, ex)
                    StatusBar1.Message.Error(message & " Check log.txt for details.")
                Else
                    message = String.Format("Unknown error during load XML file '{0}'", xmlFileName)
                    Logger.[Error](message, ex)
                    StatusBar1.Message.Error(message & " Check log.txt for details.")
                End If
                doc = Nothing
            End Try
            Return doc
        End Function
	End Class
End Namespace
