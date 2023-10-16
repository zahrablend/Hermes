using HermesChat.Data.Context;
using HermesChat.Data.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace HermesChat.Web
{
    public class ChatHub : Hub
    {
        private HermesChatContext dbContext;
        public ChatHub(HermesChatContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private static readonly ConcurrentDictionary<string, string> connectedUsers =
            new ConcurrentDictionary<string, string>(); //Dict<username><connectionId>

        private static readonly Dictionary<string, HashSet<string>> activeGroups =
            new Dictionary<string, HashSet<string>>(); //Dict<group>{connections}

        private static readonly Dictionary<string, HashSet<string>> activeInvitations =
            new Dictionary<string, HashSet<string>>(); //Dict<recipient>{senders}


        //adding the connected user to the Users dictionary
        public override async Task OnConnectedAsync()
        {
            try
            {
                string userName = this.Context.User.Identity.Name;
                User user = dbContext.Users.Single(u => u.UserName == userName);

                string connectionId = Context.ConnectionId;

                connectedUsers.TryAdd(user.Name, connectionId);
                await Console.Out.WriteLineAsync($"OnConnectedAsync(): connected {userName}");
                await base.OnConnectedAsync();
                await Clients.Caller.SendAsync("setUserNameGroup", user.Name);
                await Clients.Caller.SendAsync("setUserNamePrivate", user.Name);
                await Clients.All.SendAsync("updateUserList", connectedUsers.Keys.ToList());
                await Clients.Caller.SendAsync("updateGroupList", activeGroups.Keys.ToList());
                if (activeInvitations.ContainsKey(user.Name))  //load invitation list if exists
                {
                    await Clients.Caller.SendAsync("updateInvitations", activeInvitations[user.Name]);
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

        //removing the connected user to the Users dictionary
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                string userName = this.Context.User.Identity.Name;
                User user = dbContext.Users.Single(u => u.UserName == userName);

                string removedConnectionId;
                connectedUsers.Remove(user.Name, out removedConnectionId);

                await RemoveFromAllGroups();
                await Console.Out.WriteLineAsync($"OnDisconnectedAsync(): {userName}");
                await base.OnDisconnectedAsync(exception);
                await Clients.All.SendAsync("updateUserList", connectedUsers.Keys.ToList());
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

        //adding user to the group + updating active group list
        public async Task AddToGroup(string groupName)
        {
            try
            {
                string userName = this.Context.User.Identity.Name;
                User user = dbContext.Users.Single(u => u.UserName == userName);
                await RemoveFromAllGroups();
                await RemoveSendInvitation();
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                await Clients.Group(groupName).SendAsync("enteredOrLeft", $"{user.Name} has joined the group \"{groupName}\"");
                await Console.Out.WriteLineAsync($"AddToGroup() {groupName}: {userName}");
                await Clients.Caller.SendAsync("setGroupName", groupName);
                if (!activeGroups.ContainsKey(groupName)) //if group is new, start collecting connections
                {
                    activeGroups[groupName] = new HashSet<string>();
                }
                activeGroups[groupName].Add(Context.ConnectionId); //add connection to HashSet
                await Clients.All.SendAsync("updateGroupList", activeGroups.Keys.ToList());
                await Clients.Caller.SendAsync("updateUserList", connectedUsers.Keys.ToList()); //to rerender Start private chat buttons if user had pending invitations and started group chat
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

        //removing user from the group + updating active group list
        public async Task RemoveFromGroup(string groupName)
        {
            try
            {
                string userName = Context.User?.Identity?.Name;
                User user = dbContext.Users.Single(u => u.UserName == userName);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                bool removed = activeGroups[groupName].Remove(Context.ConnectionId);
                if (removed)
                {
                    await Clients.Group(groupName).SendAsync("enteredOrLeft", $"{user.Name} has left the group \"{groupName}\"");
                    await Console.Out.WriteLineAsync($"RemoveFromGroup() {groupName}: {userName}");
                }
                if (activeGroups[groupName].Count == 0)
                {
                    activeGroups.Remove(groupName);
                    await Clients.All.SendAsync("updateGroupList", activeGroups.Keys.ToList());
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

        //removing user from connections, so that she can leave the group on disconection
        public async Task RemoveFromAllGroups()
        {
            foreach (var groupName in activeGroups.Keys.ToList())
            {
                await RemoveFromGroup(groupName);
            };
        }

        //send notification to user that she has new private chat inquiry
        //adding invitations to active invitations list. 
        public async Task InviteToPrivate(string recipient)
        {
            try
            {
                var recipientId = dbContext.Users.Single(u => u.UserName == recipient).Id;
                string sender = this.Context.User.Identity.Name;
                if (!activeInvitations.ContainsKey(recipient))
                {
                    activeInvitations[recipient] = new HashSet<string>();
                }
                activeInvitations[recipient].Add(sender); //add sender to recipients HashSet
                await Clients.User(recipientId).SendAsync("updateInvitations", activeInvitations[recipient]); //send hashset of the current recipientId
                Console.WriteLine($"{sender} has invited user {recipient}");
                foreach (var item in activeInvitations[recipient])
                {
                    await Console.Out.WriteLineAsync($"sender: {item}");
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

        //user accepted private chat inquiry
        public async Task AcceptToPrivate(string toUser)
        {
            try
            {
                var toUserId = dbContext.Users.Single(u => u.UserName == toUser).Id;
                string accepter = this.Context.User.Identity.Name;
                Console.WriteLine($"{accepter} has accepted {toUser}");
                activeInvitations[accepter].Remove(toUser); //remove sender from acceptors invitations 
                await Clients.User(toUserId).SendAsync("userAccepted", accepter);
                await Clients.Caller.SendAsync("updateInvitations", activeInvitations[accepter]);
                await Clients.Caller.SendAsync("updateUserList", connectedUsers.Keys.ToList()); //to rerender userList so it can hide buttons
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

        //user declined private chat inquiry
        public async Task DeclinePrivate(string toUser)
        {
            try
            {
                var toUserId = dbContext.Users.Single(u => u.UserName == toUser).Id;
                string accepter = this.Context.User.Identity.Name;
                Console.WriteLine($"{accepter} has declined {toUser}");
                activeInvitations[accepter].Remove(toUser); //remove sender from acceptors invitations 
                await Clients.User(toUserId).SendAsync("userDeclined", accepter);
                await Clients.Caller.SendAsync("updateInvitations", activeInvitations[accepter]);
                await Clients.Caller.SendAsync("updateUserList", connectedUsers.Keys.ToList()); //to rerender userList so it can hide buttons
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

        public async Task SendMessagePrivate(string toUser, string message)
        {
            try
            {
                var toUserId = dbContext.Users.Single(u => u.UserName == toUser).Id;
                string sender = this.Context.User.Identity.Name; //fromUser
                User user = dbContext.Users.Single(u => u.UserName == sender);
                await Console.Out.WriteLineAsync($"sender: {user.Name}");
                await Console.Out.WriteLineAsync($"recipient: {toUser}");
                await Clients.Caller.SendAsync("receiveMessagePrivate", user.Name, message);
                await Clients.User(toUserId).SendAsync("receiveMessagePrivate", user.Name, message);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

        public async Task SendMessageGroup(string groupName, string message)
        {
            try
            {
                string userName = this.Context.User.Identity.Name;

                User user = dbContext.Users.Single(u => u.UserName == userName);
                await Clients.Group(groupName).SendAsync("receiveMessageGroup", user.Name, message);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

        //called to restore Start chat buttons after leaving private chat
        public async Task RequestUserListUpdate(string toUser)
        {
            try
            {
                var toUserId = dbContext.Users.Single(u => u.UserName == toUser).Id;
                await Clients.Caller.SendAsync("updateUserList", connectedUsers.Keys.ToList()); //to rerender userList so it can hide buttons
                await Clients.User(toUserId).SendAsync("updateUserList", connectedUsers.Keys.ToList()); //to rerender userList so it can hide buttons
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

        //called to restore Join group buttons after leaving group chat
        public async Task RequestGroupListUpdate()
        {
            try
            {
                await Clients.Caller.SendAsync("updateGroupList", activeGroups.Keys.ToList()); //to rerender userList so it can hide buttons
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

        //removing previous invitation send by user when it is still pending.
        //To be called after invitation sender initiates next invitation or joining group chat
        public async Task RemoveSendInvitation()
        {
            try
            {
                string sender = this.Context.User.Identity.Name;
                await Console.Out.WriteLineAsync("RemoveSendInvitations invoked " + sender);

                foreach (var keyValuePair in activeInvitations)
                {
                    var removed = keyValuePair.Value.Remove(sender); //remove sender value from the dict pair
                    if (removed)
                    {
                        var recipient = keyValuePair.Key;
                        var recipientId = dbContext.Users.Single(u => u.UserName == recipient).Id;
                        await Console.Out.WriteLineAsync("invitation removed " + recipient + " " + sender);
                        await Clients.User(recipientId).SendAsync("updateInvitations", activeInvitations[recipient]);
                    }
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw;
            }
        }

    }
}
