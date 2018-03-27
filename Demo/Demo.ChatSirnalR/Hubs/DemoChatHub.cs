using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Demo.ChatSirnalR.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Demo.ChatSirnalR.Hubs
{
    [HubName("demoChatHub")]
    public class DemoChatHub : Hub
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        public void SendMessage(string id, string message)
        {
            var fromUser = _db.Users.FirstOrDefault(a => a.ConnectionId == Context.ConnectionId);
            if (fromUser == null)
                throw new Exception($"User#{Context.ConnectionId} is disconnected!");

            var toUser = _db.Users.Find(id);
            if (toUser == null)
                throw new Exception($"User#{id} is unavaliable!");

            var ent = new MessageHistory
            {
                Content = message,
                Date = DateTime.Now,
                FromUserId = fromUser.Id,
                ToUserId = toUser.Id
            };
            _db.MessageHistories.Add(ent);
            _db.Entry(ent).State = EntityState.Added;
            _db.SaveChanges();

            var connections = new List<string>
            {
                Context.ConnectionId,
                toUser.ConnectionId
            };
            var date = ent.Date.ToLongDateString() + " " + ent.Date.ToShortTimeString();

            Clients.Clients(connections).UpdateChat(id, date, message);

        }

        private void UpdateUsers()
        {
            try
            {
                var activeUsers = _db.Users.Select(a => new
                {
                    id = a.Id,
                    name = a.UserName,
                    online = a.IsOnline ? 1 : 0
                }).ToArray();

                Clients.All.UpdateUsers(new
                {
                    success = true,
                    result = activeUsers
                });
            }
            catch (Exception ex)
            {
                Clients.All.UpdateUsers(new
                {
                    success = false,
                    result = ex.Message
                });
            }
        }

        public dynamic ConnectUser(string id)
        {
            try
            {
                var user = _db.Users.Find(id);

                if (user == null)
                    throw new Exception($"User#{id} is null.");

                user.ConnectionId = Context.ConnectionId;
                user.IsOnline = true;

                _db.Entry(user).State = EntityState.Modified;
                _db.SaveChanges();

                UpdateUsers();

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

            var user = _db.Users.FirstOrDefault(a => a.ConnectionId == Context.ConnectionId);

            if (user == null)
                throw new Exception("An attempt to reconnect a non tracked connection id have been made.");

            user.IsOnline = true;

            _db.Entry(user).State = EntityState.Modified;
            _db.SaveChanges();

            UpdateUsers();

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var user = _db.Users.FirstOrDefault(a => a.ConnectionId == Context.ConnectionId);

            if (user == null)
                throw new Exception("An attempt to disconnect a non tracked connection id have been made.");

            user.IsOnline = false;

            _db.Entry(user).State = EntityState.Modified;
            _db.SaveChanges();

            UpdateUsers();

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