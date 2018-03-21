using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Demo.ChatSirnalR.Models;
using Microsoft.AspNet.SignalR;

namespace Demo.ChatSirnalR.Hubs
{
    public class DemoChatHub : Hub
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        private string CurrentConnectionId => Context.ConnectionId;

        public void SendMessage(string userName, string message)
        {
            if (message.StartsWith("#"))
            {
                var pmUserName = message.Split(' ')[0].Substring(1);
                var pmConnection = _db.Users.FirstOrDefault(a => a.UserName.ToLower() == pmUserName && a.IsOnline);

                if (pmConnection != null)
                {
                    var connections = new List<string>
                    {
                        CurrentConnectionId,
                        pmConnection.ConnectionId
                    };

                    Clients.Clients(connections).UpdateChat(userName, message, true);

                    return;
                }
            }

            Clients.All.UpdateChat(userName, message);
        }

        private void UpdateUsersOnline()
        {
            try
            {
                var activeUsers = _db.Users.Where(a => a.IsOnline && a.ConnectionId != CurrentConnectionId).Select(a => a.UserName).ToArray();

                Clients.All.UpdateUsersOnline(new
                {
                    success = true,
                    result = activeUsers
                });
            }
            catch (Exception ex)
            {
                Clients.All.UpdateUsersOnline(new
                {
                    success = false,
                    result = ex.Message
                });
            }
        }

        public dynamic ConnectUser(string userName)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(a => a.UserName == userName);

                if (user == null)
                    throw new Exception($"User#{userName} is null.");

                user.ConnectionId = CurrentConnectionId;
                user.IsOnline = true;
                _db.SaveChanges();

                UpdateUsersOnline();

                return new
                {
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    result = ex.Message
                };
            }
        }

        public override Task OnReconnected()
        {

            var user = _db.Users.FirstOrDefault(a => a.ConnectionId == CurrentConnectionId);

            if (user == null)
                throw new Exception("An attempt to reconnect a non tracked connection id have been made.");

            user.IsOnline = true;
            _db.SaveChanges();

            UpdateUsersOnline();

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var user = _db.Users.FirstOrDefault(a => a.ConnectionId == CurrentConnectionId);

            if (user == null)
                throw new Exception("An attempt to disconnect a non tracked connection id have been made.");

            user.IsOnline = false;
            _db.SaveChanges();

            UpdateUsersOnline();

            return base.OnDisconnected(stopCalled);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}