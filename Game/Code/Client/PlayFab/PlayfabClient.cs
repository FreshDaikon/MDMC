using Godot;
using System;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System.Collections.Generic;
using SmartFormat.Core.Parsing;
using System.Reflection.Metadata;
using PlayFab.DataModels;
using PlayFab.AdminModels;



public partial class PlayfabClient : Node
{
	public static PlayfabClient instance;
	private string titleId = "5FDDE";
	private PlayFabResult<LoginResult> cachedToken;

	//Player Tokens:
	private PlayFab.MultiplayerModels.EntityKey entityKey;
	//Lobby Details:
	private string lobbyId;

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
	}

	public async void LoginPlayer(string username, string password) 
	{
		MD.Log("Trying to login player...");
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
			MD.Log("Login failed - let's have a look");
			MD.Log(PlayFabUtil.GenerateErrorReport(apiError));
		}
		else if(apiResult != null)
		{
			MD.Log("Login sucess!");
			var auth = apiResult.AuthenticationContext.ClientSessionTicket;			
			var wasCreated = apiResult.NewlyCreated;		
			
			//figure out the whole client thing :
			PlayFab.ClientModels.EntityKey playerEntity = apiResult.EntityToken.Entity;
			cachedToken = token;
		}
	}
	public async void CreateLobby()
	{
		if(cachedToken != null)
		{
			var tokenResult = cachedToken.Result;
			if(tokenResult != null)
			{
				PlayFab.MultiplayerModels.EntityKey multiplayerkey = new PlayFab.MultiplayerModels.EntityKey{
					Id = tokenResult.EntityToken.Entity.Id,
					Type = tokenResult.EntityToken.Entity.Type
				};
				List<Member> initialMembers = new List<Member>();
				Member member = new Member{
					MemberEntity = multiplayerkey
				};
				initialMembers.Add(member);
				CreateLobbyRequest request = new CreateLobbyRequest
				{
					AuthenticationContext = cachedToken.Result.AuthenticationContext,
					Owner = multiplayerkey,
					Members = initialMembers,
					MaxPlayers = 8,
				};
				var lobbyRequest = PlayFabMultiplayerAPI.CreateLobbyAsync(request);
				PlayFabResult<CreateLobbyResult> token = await lobbyRequest;	

				var lobbyError = token.Error;
				var lobbyResult = token.Result;

				if(lobbyError != null)
				{
					MD.Log("Requsting a Server failed - let's have a look");
					MD.Log(PlayFabUtil.GenerateErrorReport(lobbyError ));
				}
				else if(lobbyResult != null)
				{
					// Connection string :
					MD.Log("Succesfully created lobby with id : " + lobbyResult.LobbyId);
					MD.Log("The connection string is :  [ " + lobbyResult.ConnectionString + " ]");
					var lobbyThing = lobbyResult.ConnectionString;					
					lobbyId = lobbyResult.LobbyId;
					MD.Log(lobbyThing);
				}
			}
		}
	}
	public async void JoinLobby(string connectionString)
	{
		if(cachedToken != null)
		{
			var tokenResult = cachedToken.Result;
			if(tokenResult != null)
			{
				PlayFab.MultiplayerModels.EntityKey multiplayerkey = new PlayFab.MultiplayerModels.EntityKey{
					Id = tokenResult.EntityToken.Entity.Id,
					Type = tokenResult.EntityToken.Entity.Type
				};
				JoinLobbyRequest request = new JoinLobbyRequest
				{
					AuthenticationContext = cachedToken.Result.AuthenticationContext,
					MemberEntity = multiplayerkey,
					ConnectionString = connectionString,
				};
				var lobbyRequest = PlayFabMultiplayerAPI.JoinLobbyAsync(request);
				PlayFabResult<JoinLobbyResult> token = await lobbyRequest;	
				var lobbyError = token.Error;
				var lobbyResult = token.Result;

				if(lobbyError != null)
				{
					MD.Log("Lobby failed - let's have a look");
					MD.Log(PlayFabUtil.GenerateErrorReport(lobbyError ));
				}
				else if(lobbyResult != null)
				{
					MD.Log("Succesfully joined lobby with id : " + lobbyResult.LobbyId);
					MD.Log(lobbyResult.LobbyId);
					
					
				}
			}
		}		
	}
	//
	public async void RequstServer(string arena)
	{
		if(cachedToken != null)
		{
			RequestMultiplayerServerRequest request = new RequestMultiplayerServerRequest
			{
				SessionId = cachedToken.Result.SessionTicket,
				SessionCookie = new string(""),
				CustomTags = new System.Collections.Generic.Dictionary<string, string>
				{ 
					{ arena, "BasicArena"}
				}
			};

			var serverResult = PlayFabMultiplayerAPI.RequestMultiplayerServerAsync(request);
			PlayFabResult<RequestMultiplayerServerResponse> token = await serverResult;

			var apiError = token.Error;
			var apiResult = token.Result;

			if(apiError != null)
			{
				MD.Log("Requsting a Server failed - let's have a look");
				MD.Log(PlayFabUtil.GenerateErrorReport(apiError ));
			}
			else if(apiResult != null)
			{
				string ip = apiResult.IPV4Address;
				int port = apiResult.Ports[0].Num;
				ClientManager.Instance.StartAsClient(ip, port);
			}
			else
			{
				// eeh, something went really weird.
			}			
		}
	}
}
