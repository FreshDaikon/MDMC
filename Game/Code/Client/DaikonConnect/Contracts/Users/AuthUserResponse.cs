using System;

namespace Daikon.Contracts.Users;

public record AuthUserResponse
{
    public Guid SessionToken { get; set; }
};