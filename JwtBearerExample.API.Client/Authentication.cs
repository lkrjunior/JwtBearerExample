using System;
namespace JwtBearerExample.API.Client
{
	public class Authentication
	{
        public string AccessToken { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}

