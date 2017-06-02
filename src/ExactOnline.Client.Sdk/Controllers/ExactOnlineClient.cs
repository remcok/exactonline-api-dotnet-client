﻿using System;
using System.Linq;
using ExactOnline.Client.Models.Current;
using ExactOnline.Client.Sdk.Delegates;
using ExactOnline.Client.Sdk.Helpers;

namespace ExactOnline.Client.Sdk.Controllers
{
	/// <summary>
	/// Front Controller for working with Exact Online Entities
	/// </summary>
	public class ExactOnlineClient
	{
		private readonly ApiConnector _apiConnector;

		// https://start.exactonline.nl/api/v1
		private readonly string _exactOnlineApiUrl;

		private readonly ControllerList _controllers;
		private int _division;

		#region Constructors

		/// <summary>
		/// Create instance of ExactClient
		/// </summary>
		/// <param name="exactOnlineUrl">The Exact Online URL for your country</param>
		/// <param name="division">Division number</param>
		/// <param name="accesstokenDelegate">Delegate that will be executed the access token is expired</param>
		public ExactOnlineClient(string exactOnlineUrl, int division, AccessTokenManagerDelegate accesstokenDelegate)
		{
			// Set culture for correct deserializing of API Response (comma and points)
			_apiConnector = new ApiConnector(accesstokenDelegate);
			//Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

			if (!exactOnlineUrl.EndsWith("/")) exactOnlineUrl += "/";
			_exactOnlineApiUrl = exactOnlineUrl + "api/v1/";

			_division = (division > 0) ? division : GetDivision();
			string serviceRoot = _exactOnlineApiUrl + _division + "/";

			_controllers = new ControllerList(_apiConnector, serviceRoot);
		}

		/// <summary>
		/// Create instance of ExactClient
		/// </summary>
		/// <param name="exactOnlineUrl">{URI}/</param>
		/// <param name="accesstokenDelegate">Valid oAuth AccessToken</param>
		public ExactOnlineClient(string exactOnlineUrl, AccessTokenManagerDelegate accesstokenDelegate)
			: this(exactOnlineUrl, 0, accesstokenDelegate)
		{
		}

		#endregion

		#region Public methods

		/// <summary>
		/// return the division number of the current user
		/// </summary>
		/// <returns>Division number</returns>
		public int GetDivision()
		{
			if (_division > 0)
			{
				return _division;
			}

			var conn = new ApiConnection(_apiConnector, _exactOnlineApiUrl + "current/Me");
			string response = conn.Get("$select=CurrentDivision");
			response = ApiResponseCleaner.GetJsonArray(response);
			var converter = new EntityConverter();
			var currentMe = converter.ConvertJsonArrayToObjectList<Me>(response).FirstOrDefault(); ;

			if (currentMe != null)
			{
				_division = currentMe.CurrentDivision;
				return _division;
			}

			throw new Exception("Cannot get division. Please specify division explicitly via the constructor.");
		}

		/// <summary>
		/// Returns instance of ExactOnlineQuery that can be used to manipulate data in Exact Online
		/// </summary>
		public ExactOnlineQuery<T> For<T>() where T : class
		{
			var controller = _controllers.GetController<T>();
			return new ExactOnlineQuery<T>(controller);
		}

		#endregion
	}
}
