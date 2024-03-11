using Godot;
using PlayFab;
using PlayFab.ClientModels;

namespace Mdmc.Code.Client.PlayFab;

public partial class PlayfabManager : Node
{
	// Public Static Access:
	public static PlayfabManager Instance;
	
	// Internal:
	private string _titleId = "5FDDE";

	public override void _Ready()
	{
		if(Instance != null)
		{
			Free();
			return;
		}
		Instance = this;
		PlayFabSettings.staticSettings.TitleId = _titleId;
		LoginPlayer("rs", "rs");
	}

	public async void LoginPlayer(string username, string password) 
	{
		GD.Print("Trying to login player...");
		LoginWithCustomIDRequest request = new LoginWithCustomIDRequest
		{
			CustomId = username,
			PlayerSecret = password,
			CreateAccount = true
		};

		var playerLogin = PlayFabClientAPI.LoginWithCustomIDAsync(request);		
		PlayFabResult<LoginResult> token = await playerLogin;	

		var apiError = token.Error;
		var apiResult = token.Result;
		
		if(apiError != null)
		{
			GD.Print("Login failed - let's have a look");
			GD.Print(PlayFabUtil.GenerateErrorReport(apiError));
		}
		else if(apiResult != null)
		{
			GD.Print("Login sucess!");
			var auth = apiResult.AuthenticationContext.ClientSessionTicket;			
			var wasCreated = apiResult.NewlyCreated;		
			
			//figure out the whole client thing :
			EntityKey playerEntity = apiResult.EntityToken.Entity;
		}
	}

}
