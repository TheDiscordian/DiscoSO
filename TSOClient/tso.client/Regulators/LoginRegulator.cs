using FSO.Client.Utils;
using FSO.Common;
using FSO.Common.Domain.Shards;
using FSO.Server.Clients;
using FSO.Server.Clients.Framework;
using FSO.Server.Protocol.Authorization;
using FSO.Server.Protocol.CitySelector;
using System;
using System.Collections.Generic;

namespace FSO.Client.Regulators
{
    /// <summary>
    /// Handles authentication and city server network activity
    /// </summary>
    public class LoginRegulator : AbstractRegulator
    {
        public AuthResult AuthResult { get; internal set; }
        public List<AvatarData> Avatars { get; internal set; } = new List<AvatarData>();
        //public List<ShardStatusItem> Shards { get; internal set; } = new List<ShardStatusItem>();
        public IShardsDomain Shards;

        private AuthClient AuthClient;
        private CityClient CityClient;

        public LoginRegulator(AuthClient authClient, CityClient cityClient, IShardsDomain domain)
        {
            this.Shards = domain;
            this.AuthClient = authClient;
            this.CityClient = cityClient;
            
            AddState("NotLoggedIn")
                .Default()
                    .Transition()
                        .OnData(typeof(AuthRequest)).TransitionTo("AuthLogin");

            AddState("AuthLogin").OnlyTransitionFrom("NotLoggedIn");
            AddState("InitialConnect").OnlyTransitionFrom("AuthLogin");
            AddState("AvatarData").OnlyTransitionFrom("InitialConnect", "UpdateRequired", "LoggedIn");
            AddState("ShardStatus").OnlyTransitionFrom("AvatarData");
            AddState("LoggedIn").OnlyTransitionFrom("ShardStatus");

            AddState("UpdateRequired").OnlyTransitionFrom("InitialConnect");
        }

        protected override void OnAfterTransition(RegulatorState oldState, RegulatorState newState, object data)
        {
            switch (newState.Name)
            {
                case "AuthLogin":
                    var loginData = (AuthRequest)data;
                    var result = AuthClient.Authenticate(loginData);

                    if (result == null || !result.Valid)
                    {
                        if (result.ReasonText != null)
                        {
                            base.ThrowErrorAndReset(ErrorMessage.FromLiteral(result.ReasonText));
                        }
                        else if (result.ReasonCode != null)
                        {
                            base.ThrowErrorAndReset(ErrorMessage.FromLiteral(
                                (GameFacade.Strings.GetString("210", result.ReasonCode) ?? "Unknown Error")
                                .Replace("EA.com", AuthClient.BaseUrl.Substring(7).TrimEnd('/'))
                                ));
                        }
                        else
                        {
                            base.ThrowErrorAndReset(new Exception("Unknown error"));
                        }
                    }
                    else
                    {
                        this.AuthResult = result;
                        AsyncTransition("InitialConnect");
                    }
                    break;
                case "InitialConnect":
                    try {
                        var connectResult = CityClient.InitialConnectServlet(
                            new InitialConnectServletRequest {
                                Ticket = AuthResult.Ticket,
                                Version = "Version 1.1097.1.0"
                            });

                        if (connectResult.Status == InitialConnectServletResultType.Authorized)
                        {
                            var cdnurl = connectResult.UserAuthorized.FSOCDNUrl;
                            if (cdnurl != null)
                                ApiClient.CDNUrl = cdnurl;

                            if (RequireUpdate(connectResult.UserAuthorized) && !FSOEnvironment.SoftwareKeyboard)
                            {
                                AsyncTransition("UpdateRequired", connectResult.UserAuthorized);
                            }
                            else
                            {
                                AsyncTransition("AvatarData");
                            }
                        }
                        else if (connectResult.Status == InitialConnectServletResultType.Error)
                        {
                            base.ThrowErrorAndReset(ErrorMessage.FromLiteral(connectResult.Error.Code, connectResult.Error.Message));
                        }
                    }catch(Exception ex)
                    {
                        base.ThrowErrorAndReset(ex);
                    }
                    break;
                case "UpdateRequired":
                    break;
                case "AvatarData":
                    try {
                        Avatars = CityClient.AvatarDataServlet();
                        AsyncTransition("ShardStatus");
                    }
                    catch (Exception ex)
                    {
                        base.ThrowErrorAndReset(ex);
                    }
                    break;

                case "ShardStatus":
                    try {
                        ((ClientShards)Shards).All = CityClient.ShardStatus();
                        AsyncTransition("LoggedIn");
                    }
                    catch (Exception ex)
                    {
                        base.ThrowErrorAndReset(ex);
                    }
                    break;
                case "LoggedIn":
                    FSOFacade.Controller.ShowPersonSelection();
                    break;
            }
        }

        public bool RequireUpdate(UserAuthorized auth)
        {
            if (auth.FSOVersion == null) return false;

            var str = GlobalSettings.Default.ClientVersion;
            var authstr = auth.FSOBranch + "-" + auth.FSOVersion;
            if (str == authstr) return false;

            //the server's version is a minimum: only update when the client is older.
            //non-numeric branches (e.g. "beta") fall back to exact match.
            var client = ParseVersion(str);
            var server = ParseVersion(authstr);
            if (client == null || server == null) return true;

            int len = (client.Length > server.Length) ? client.Length : server.Length;
            for (int i = 0; i < len; i++)
            {
                int c = (i < client.Length) ? client[i] : 0;
                int s = (i < server.Length) ? server[i] : 0;
                if (c != s) return c < s;
            }
            return false;
        }

        private static int[] ParseVersion(string version)
        {
            var split = version.LastIndexOf('-');
            if (split == -1) return null;
            int update;
            if (!int.TryParse(version.Substring(split + 1), out update)) return null;
            var parts = version.Substring(0, split).Split('.');
            var result = new int[parts.Length + 1];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out result[i])) return null;
            }
            result[parts.Length] = update;
            return result;
        }

        protected override void OnBeforeTransition(RegulatorState oldState, RegulatorState newState, object data)
        {
        }

        public void Login(AuthRequest request){
            this.AsyncProcessMessage(request);
        }

        public void Logout()
        {
            this.AsyncTransition("NotLoggedIn");
        }
    }
}
