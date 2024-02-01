public enum OrchMessageType
{
    ConnectionError = 1,
    ConnectionEstablished = 2,
    RequestingGame = 3,    
    GameCreated = 4,
    GameFound = 5,
    GameRequestFailed = 6,
    JoinRequestFailed = 7,
    RequstingJoin = 8
}

public enum ClientMessageType
{
    RequestGame = 1,
    JoinGame = 2
}
