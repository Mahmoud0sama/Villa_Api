﻿using System.Net;

namespace Villa_API.Models
{
	public class APIResponse
	{
        public APIResponse()
        {
            ErrorMessages = new List<string>();
        }
        public HttpStatusCode StatusCode { get; set; }
		public bool IsSuccessfull { get; set; } = true;
		public List<string> ErrorMessages { get; set; }
		public object Result { get; set; }
	}
}
