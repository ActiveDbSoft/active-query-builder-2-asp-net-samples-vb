Imports Microsoft.Owin
Imports MVC5ChangeConnection
Imports Owin

<Assembly: OwinStartup(GetType(Startup))>
Public Partial Class Startup
	Public Sub Configuration(app As IAppBuilder)
		ConfigureAuth(app)
	End Sub
End Class

