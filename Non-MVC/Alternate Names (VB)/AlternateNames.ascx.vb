Imports System.Configuration
Imports System.IO
Imports System.Xml
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Control
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server

Partial Public Class AlternateNames
    Inherits System.Web.UI.UserControl
    Private Class ExchangeClass
        Public SQL As String
        Public AlternateSQL As String
    End Class

    Private plainTextSQLBuilder As PlainTextSQLBuilder
    Private plainTextSQLBuilderWithAltNames As PlainTextSQLBuilder

    Public Sub Page_Load(sender As Object, e As EventArgs)
        If Not Page.IsPostBack Then
            SQLEditor1.SQL = "Select Employees.""Employee ID"", Employees.""First Name"", Employees.""Last Name"", ""Employee Photos"".""Photo Image"", ""Employee Resumes"".Resume From ""Employee Photos"" Inner Join Employees On ""Employee Photos"".""Employee ID"" = Employees.""Employee ID"" Inner Join ""Employee Resumes"" On ""Employee Resumes"".""Employee ID"" = Employees.""Employee ID"""
        End If
    End Sub

    Public Sub SleepModeChanged(sender As Object, e As EventArgs)
        Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
        If queryBuilder.SleepMode Then StatusBar1.Message.Error("Unsupported SQL statement.")
    End Sub

    Public Sub QueryBuilderControl1_OnSQLUpdated(sender As Object, e As EventArgs, clientdata As ClientData)
        SessionStore.Current.Exchange.Data = New ExchangeClass() With {
         .SQL = plainTextSQLBuilder.SQL,
         .AlternateSQL = plainTextSQLBuilderWithAltNames.SQL
        }
    End Sub

    Public Sub QueryBuilderControl1_Init(sender As Object, e As EventArgs)
        ' Get instance of QueryBuilder
        Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder

        queryBuilder.BehaviorOptions.UseAltNames = True
        plainTextSQLBuilder = New PlainTextSQLBuilder() With {
         .QueryBuilder = queryBuilder,
         .KeywordFormat = KeywordFormat.UpperCase
        }
        plainTextSQLBuilderWithAltNames = New PlainTextSQLBuilder() With {
         .QueryBuilder = queryBuilder,
         .KeywordFormat = KeywordFormat.UpperCase,
         .UseAltNames = False
        }

        ' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
        queryBuilder.BehaviorOptions.AllowSleepMode = True
        queryBuilder.SyntaxProvider = New DB2SyntaxProvider()

        queryBuilder.OfflineMode = True
        ' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/XmlMetaData] key
        ' Pass loaded XML to QueryBuilder component
        Try
            queryBuilder.MetadataContainer.ImportFromXML(Page.Server.MapPath(ConfigurationManager.AppSettings("XmlMetaData")))
            queryBuilder.MetadataStructure.Refresh()
            StatusBar1.Message.Information("Metadata loaded")
        Catch ex As Exception
            Dim message As String =
                "Error loading metadata from the database." +
                "Check the 'configuration\\connectionStrings' key in the [web.config] file."
            Logger.[Error](message, ex)
            StatusBar1.Message.Error(message & " Check log.txt for details.")
        End Try

    End Sub


End Class
