using Godot;
using PlayFab;
using PlayFab.ClientModels;
using Daikon.Helpers;

namespace Daikon.Client.Playfab;

public partial class PlayfabClient : Node
{
	public static PlayfabClient instance;
	private string titleId = "5FDDE";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(instance != null)
		{
			Free();
			return;
		}
		instance = this;
		PlayFabSettings.staticSettings.TitleId = titleId;
		LoginPlayer("rs", "rs");
	}

	public async void LoginPlayer(string username, string password) 
	{
		GD.Print("Trying to login player...");
		// TODO : change to use steam...
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
			PlayFab.ClientModels.EntityKey playerEntity = apiResult.EntityToken.Entity;
		}
	}

}
