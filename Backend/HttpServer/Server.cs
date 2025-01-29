using System.Data.Entity.Core.Objects;
using System.Text;
using System.Data.SQLite;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using WebSocketSharp.Server;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SQLiteCommand = System.Data.SQLite.SQLiteCommand;

namespace HttpServer;
public class Server
{
    private SQLiteConnection _connection;
    private WebSocketServer _server;
    private Dictionary<string,Echo> _echoInstances = new();
    private static Random _random = new Random();




    public void DbToServer()
    {
        string sqlCommandString = "SELECT room_name FROM rooms";
        var sqlCommand = new SQLiteCommand(sqlCommandString, _connection);
        var output = sqlCommand.ExecuteReader();
        while (output.Read())
        {
            string roomName = output["room_name"].ToString();
            List<string> roomUsers = new List<string>();
            sqlCommandString = $"SELECT u.user_id, u.username FROM users u JOIN room_users tu ON u.user_id = tu.user_id JOIN rooms r ON tu.room_id = r.room_id WHERE r.room_name = '{roomName}';";
            sqlCommand = new SQLiteCommand(sqlCommandString, _connection);
            var output2 = sqlCommand.ExecuteReader();
            while (output2.Read())
            {
                roomUsers.Add(output2["user_id"]+"_"+output2["username"]);
            }
            
            _server.AddWebSocketService($"/Chat/{roomName}",() => {
                var echo = new Echo(roomUsers);
    
                if (!_echoInstances.ContainsKey(roomName))
                {
                    _echoInstances.Add(roomName, echo);
                }
                else
                {
                    Console.WriteLine($"Room {roomName} already exists in echoInstances.");
                }
    
                return echo;            
            });

            Console.WriteLine("Successfully added "+roomName +" from DB.");

        }
    }

    public void CheckMain()
    {
        string sqlCommandString="SELECT COUNT(*) FROM rooms WHERE room_name = 'Main'";
        var sqlCommand = new SQLiteCommand(sqlCommandString, _connection);
        int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
        if(count==0)
        {
            Console.WriteLine("Main room doesn't exist; creating now");
            List<string> roomUsers = new List<string>();
            sqlCommandString = "SELECT u.user_id, u.username FROM users u";
            sqlCommand = new SQLiteCommand(sqlCommandString, _connection);
            var output2 = sqlCommand.ExecuteReader();
            while (output2.Read())
            {
                roomUsers.Add(output2["user_id"]+"_"+output2["username"]);

            }
            //Echo roomEcho = new Echo(roomUsers);
            _server.AddWebSocketService("/Chat/Main",()=> {
                var echo = new Echo(roomUsers);
    
                if (!_echoInstances.ContainsKey("Main"))
                {
                    _echoInstances.Add("Main", echo);
                }
                else
                {
                    Console.WriteLine($"Room Main already exists in echoInstances.");
                }
    
                return echo;
            });
            sqlCommandString = "INSERT INTO rooms (room_name) values ('Main')";
            sqlCommand = new SQLiteCommand(sqlCommandString, _connection);
            sqlCommand.ExecuteNonQuery();
        }
    }

    
    public void CreateRoom()
    {
        Console.WriteLine("Room Name: ");
        String inputRoomName = Console.ReadLine();
        string sqlCommandString =$"SELECT room_name FROM rooms WHERE room_name='{inputRoomName}';";
        SQLiteCommand command = new SQLiteCommand(sqlCommandString, _connection);
        var commandOutputReader = command.ExecuteReader();
        bool roomExists =false;
        while (commandOutputReader.Read())
        {
            String roomName = commandOutputReader["room_name"].ToString();
            if (roomName == inputRoomName)
            {
                roomExists = true;
                break;
            }
        }

        if (roomExists)
        {
            Console.WriteLine("Room already exists");
            Console.WriteLine("Back to menu");
            return;
        }
        else
        {
            Console.WriteLine("Room Name is available");
        }
        
        Console.WriteLine("Input Invited Users:");
        string inputInvitedUser = Console.ReadLine();
        List< (int,string)> inputInvitedUserList = new List<(int,string)>();
        while (inputInvitedUser!="")
        {
            sqlCommandString = $"SELECT user_id FROM users WHERE username='{inputInvitedUser}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            var commandOutputScalar = command.ExecuteScalar();
            if (commandOutputScalar == null)
            {
                Console.WriteLine("User not found");
            }
            else
            {
                Console.WriteLine("User found");
                inputInvitedUserList.Add((Int32.Parse(commandOutputScalar.ToString()),inputInvitedUser));
            }
            inputInvitedUser = Console.ReadLine();
        }
        
        //Creating new websocket
        List<string> roomUsers = new List<string>();
        foreach (var user in inputInvitedUserList)
        {
            roomUsers.Add(user.Item1+"_"+user.Item2);
        }
        _server.AddWebSocketService($"/Chat/{inputRoomName}",()=> {
            var echo = new Echo(roomUsers);
    
            if (!_echoInstances.ContainsKey(inputRoomName))
            {
                _echoInstances.Add(inputRoomName, echo);
            }
            else
            {
                Console.WriteLine($"Room {inputRoomName} already exists in echoInstances.");
            }
    
            return echo;
            
        });
        
        //Adding to room table
        sqlCommandString=$"INSERT INTO rooms (room_name) VALUES('{inputRoomName}');";
        command = new SQLiteCommand(sqlCommandString, _connection);
        command.ExecuteNonQuery();
        Console.WriteLine("Room Created");
        
        //Get room_id
        sqlCommandString =$"SELECT room_id FROM rooms WHERE room_name='{inputRoomName}';";
        command = new SQLiteCommand(sqlCommandString, _connection);
        var roomId = command.ExecuteScalar().ToString();
        Console.WriteLine($"Room ID: {roomId}");
        
        //Add  users to room_users table
        foreach (var user in inputInvitedUserList)
        {
            Console.WriteLine($"User {user.Item2} joined the room {inputRoomName}");
            sqlCommandString=$"INSERT INTO room_users VALUES('{roomId}', '{user.Item1}');";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            
        }
        Console.WriteLine("Operation Completed");
        Console.WriteLine("Back to menu");
        
    }

    public void DeleteRoom()
    {
        Console.WriteLine("Room Name");
        string roomName= Console.ReadLine();
        _server.RemoveWebSocketService("/Chat/" + roomName);
        Console.WriteLine("Room socket closed");
        
        string sqlCommandString = $"SELECT room_id FROM rooms WHERE room_name='{roomName}';";
        var command = new SQLiteCommand(sqlCommandString, _connection);
        var roomId = command.ExecuteScalar();
        if (roomId == null)
        {
            Console.WriteLine("Room not found");
            Console.WriteLine("Back to menu");
        }
        else
        {
            Console.WriteLine("Room found");
            sqlCommandString=$"DELETE FROM room_users WHERE room_id='{roomId}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            Console.WriteLine("Removed room from room_users table");
            sqlCommandString=$"DELETE FROM rooms WHERE room_name='{roomName}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            Console.WriteLine("Removed room from rooms table");
            Console.WriteLine("Operation Completed");
            Console.WriteLine("Back to menu");
            
        }
        
        
    }

    public void CreateUser()
    {
        Console.WriteLine("Enter Username: ");
        string inputUsername = Console.ReadLine();
        Console.WriteLine("Enter Password: ");
        string inputPassword = Console.ReadLine();
        string inputSalt = RandomStringGenerator(10);
        string finalHash = Argon2HashGenerator(inputPassword, inputSalt);
        string sqlCommandString =
            $"INSERT INTO users (username, password,salt) VALUES('{inputUsername}', '{finalHash}','{inputSalt}');";
        var command = new SQLiteCommand(sqlCommandString, _connection);
        command.ExecuteNonQuery();
        Console.WriteLine("User Created");
        sqlCommandString =$"SELECT user_id FROM users WHERE username='{inputUsername}';";
        command = new SQLiteCommand(sqlCommandString, _connection);
        var userId = command.ExecuteScalar().ToString();
        string tokken;
        for (int i = 0; i < 3; i++)
        {
            
            Console.WriteLine("Generating token "+ i);
            tokken = RandomStringGenerator(10);
            sqlCommandString = $"INSERT INTO auth_tokens (token) VALUES ('{tokken}');";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            sqlCommandString = $"select token_id from auth_tokens WHERE token='{tokken}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            var tokenId = command.ExecuteScalar().ToString();
            sqlCommandString = $"insert into user_tokens (user_id, token_id) VALUES ('{userId}', '{tokenId}');";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            Console.WriteLine("Token Created");
            
        }
        
        
        
        Console.WriteLine("Operation Completed");
        Console.WriteLine("Reset server to apply changes");
        Console.WriteLine("Back to menu");


    }

    public void DeleteUser()
    {
        Console.WriteLine("Enter UserId: ");
        string inputUserid = Console.ReadLine();
        string sqlCommandString  = $"SELECT username FROM users WHERE user_id='{inputUserid}';";
        var command = new SQLiteCommand(sqlCommandString, _connection);
        var username = command.ExecuteScalar();
        if (username == null)
        {
            Console.WriteLine("User Not Found");
            Console.WriteLine("Back to menu");
        }
        else
        {
            sqlCommandString = $"select token_id from user_tokens where user_id='{inputUserid}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            var output = command.ExecuteReader();
            while (output.Read())
            {
                
                string token = output["token_id"].ToString();
                sqlCommandString = $"DELETE FROM user_tokens WHERE token_id='{token}';";
                command = new SQLiteCommand(sqlCommandString, _connection);
                command.ExecuteNonQuery();
                sqlCommandString = $"DELETE FROM auth_tokens WHERE token_id='{token}';";
                command = new SQLiteCommand(sqlCommandString, _connection);
                command.ExecuteNonQuery();

                
            }
            Console.WriteLine("User found");
            sqlCommandString=$"DELETE FROM room_users WHERE user_id='{inputUserid}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            var modifiedRows= command.ExecuteNonQuery();
            Console.WriteLine($"{modifiedRows} rows deleted from room_users table");
            sqlCommandString=$"DELETE FROM users WHERE user_id='{inputUserid}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            Console.WriteLine("User Deleted from users table");
            Console.WriteLine("Operation Completed");
            Console.WriteLine("Reset server to apply changes");
            Console.WriteLine("Back to menu");
            
        }
    }

    public void AddUser()
    {
        Console.WriteLine("Input User Id: ");
        string inputId = Console.ReadLine();
        string sqlCommandString = $"SELECT user_id FROM users WHERE user_id='{inputId}';";
        var command = new SQLiteCommand(sqlCommandString, _connection);
        var userId = command.ExecuteScalar();
        if (userId == null)
        {
            Console.WriteLine("User Not Found");
            Console.WriteLine("Back to menu");
        }
        else
        {
            Console.WriteLine("Input Room Id");
            string inputRoomId = Console.ReadLine();
            sqlCommandString=$"SELECT room_name FROM rooms WHERE room_id='{inputRoomId}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            var roomName = command.ExecuteScalar();
            if (roomName == null)
            {
                Console.WriteLine("Room not found");
                Console.WriteLine("Back to menu");
            }
            else
            {
                sqlCommandString=$"INSERT INTO room_users (room_id,user_id) VALUES('{inputRoomId}','{inputId}');";
                command = new SQLiteCommand(sqlCommandString, _connection);
                command.ExecuteNonQuery();
                Console.WriteLine("User Added to room");
                Console.WriteLine("Operation Completed");
                Console.WriteLine("Restart server to apply changes");
                Console.WriteLine("Back to menu");
            }
        }
    }

    public void RemoveUser()
    {
        Console.WriteLine("Enter UserId: ");
        string inputUserid = Console.ReadLine();
        Console.WriteLine("Enter RoomId: ");
        string inputRoomId = Console.ReadLine();
        
        string sqlCommandString = $"SELECT room_id, user_id FROM room_users WHERE room_id='{inputRoomId}' AND user_id='{inputUserid}';";
        var command = new SQLiteCommand(sqlCommandString, _connection);
        var output = command.ExecuteScalar();
        if (output == null)
        {
            Console.WriteLine("User not found in room");
            Console.WriteLine("Back to menu");
        }
        else
        {
            sqlCommandString = $"DELETE FROM room_users WHERE user_id='{inputUserid}' AND room_id='{inputRoomId}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            Console.WriteLine("User Removed from room");
            Console.WriteLine("Operation Completed");
            Console.WriteLine("Restart server to apply changes");
            Console.WriteLine("Back to menu");
        }
    }

    public void ListRooms()
    {
        string sqlCommandString = $"SELECT * FROM rooms;";
        var command = new SQLiteCommand(sqlCommandString, _connection);
        var rooms = command.ExecuteReader();
        while (rooms.Read())
        {
            string roomName = rooms["room_name"].ToString();
            string roomId = rooms["room_id"].ToString();
            Console.WriteLine(roomName + " "+ roomId);

            string sqlCommandString2 =
                $"SELECT u.username, u.user_id FROM users u JOIN room_users ru ON u.user_id = ru.user_id JOIN rooms r ON ru.room_id = r.room_id WHERE r.room_id = '{roomId}';";
            var command2 = new SQLiteCommand(sqlCommandString2, _connection);
            var roomUsers = command2.ExecuteReader();
            while (roomUsers.Read())
            {
                Console.WriteLine($"   -{roomUsers["username"]}  {roomUsers["user_id"]}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("Operation Complete");
        Console.WriteLine("Back to menu");
    }
    
    public void ListUsers()
    {
        string sqlCommandString = "SELECT * FROM users;";
        var command = new SQLiteCommand(sqlCommandString, _connection);
        var users = command.ExecuteReader();

        while (users.Read())
        {
            // Get user information
            string username = users["username"].ToString();
            string userId = users["user_id"].ToString();

            Console.WriteLine($"{username} {userId}");

            // Query rooms for the current user
            string sqlCommandString2 = @" SELECT r.room_name, r.room_id  FROM rooms r  JOIN room_users ru ON r.room_id = ru.room_id  WHERE ru.user_id = @userId;";
            var command2 = new SQLiteCommand(sqlCommandString2, _connection);
            command2.Parameters.AddWithValue("@userId", userId);

            var userRooms = command2.ExecuteReader();
            while (userRooms.Read())
            {
                string roomId = userRooms["room_id"].ToString();
                string roomName = userRooms["room_name"].ToString();
                Console.WriteLine($"   -{roomName}  {roomId}");
            }
            userRooms.Close(); // Ensure reader is closed before moving to the next user

            Console.WriteLine(); // For better formatting
        }

        users.Close(); // Ensure reader is closed after the loop

        Console.WriteLine("Operation Complete");
        Console.WriteLine("Back to menu");
    }

public void RegenTokens()
{
    Console.WriteLine("Enter user Id: ");
    string inputUserId = Console.ReadLine();

    // Validate if the user exists
    string sqlCommandString = $"SELECT token_id FROM user_tokens WHERE user_id = '{inputUserId}';";
    using (var command = new SQLiteCommand(sqlCommandString, _connection))
    {
        var validateUserId = command.ExecuteScalar();
        if (validateUserId == null)
        {
            Console.WriteLine("User not found");
            Console.WriteLine("Back to menu");
            return;
        }
    }

    Queue<string> tokensToDelete = new Queue<string>();
    sqlCommandString = $"SELECT token_id FROM user_tokens WHERE user_id = '{inputUserId}';";
    using (var command = new SQLiteCommand(sqlCommandString, _connection))
    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            tokensToDelete.Enqueue(reader["token_id"].ToString());
        }
    }

    sqlCommandString = $"DELETE FROM user_tokens WHERE user_id = '{inputUserId}';";
    using (var command = new SQLiteCommand(sqlCommandString, _connection))
    {
        command.ExecuteNonQuery();
    }
    Console.WriteLine("Deleted tokens from user_tokens table");

    while (tokensToDelete.Count > 0)
    {
        string currentTokenId = tokensToDelete.Dequeue();
        sqlCommandString = $"DELETE FROM auth_tokens WHERE token_id = '{currentTokenId}';";
        using (var command = new SQLiteCommand(sqlCommandString, _connection))
        {
            command.ExecuteNonQuery();
        }
    }
    Console.WriteLine("Deleted tokens from auth_tokens table");

    for (int i = 0; i < 3; i++)
    {
        string newToken = RandomStringGenerator(10);

        sqlCommandString = $"INSERT INTO auth_tokens (token) VALUES ('{newToken}');";
        using (var command = new SQLiteCommand(sqlCommandString, _connection))
        {
            command.ExecuteNonQuery();
        }

        sqlCommandString = $"SELECT token_id FROM auth_tokens WHERE token = '{newToken}';";
        string newTokenId;
        using (var command = new SQLiteCommand(sqlCommandString, _connection))
        {
            newTokenId = command.ExecuteScalar().ToString();
        }

        sqlCommandString = $"INSERT INTO user_tokens (user_id, token_id) VALUES ('{inputUserId}', '{newTokenId}');";
        using (var command = new SQLiteCommand(sqlCommandString, _connection))
        {
            command.ExecuteNonQuery();
        }
    }

    Console.WriteLine("Generated new tokens");
    Console.WriteLine("Back to menu");
}


public void GetTokens()
{
    Console.WriteLine("Enter user Id: ");
    string inputUserId = Console.ReadLine();
    string sqlCommandString = $"SELECT token_id FROM user_tokens WHERE user_id = '{inputUserId}'";
    var command = new SQLiteCommand(sqlCommandString, _connection);
    var executeReader = command.ExecuteReader();
    while (executeReader.Read())
    {
        string tokenId = executeReader["token_id"].ToString();
        sqlCommandString = $"SELECT token FROM auth_tokens WHERE token_id = '{tokenId}';";
        command = new SQLiteCommand(sqlCommandString, _connection);
        string token = command.ExecuteScalar().ToString();
        Console.WriteLine($"Token: {token}");

    }
    Console.WriteLine("Operation Complete");
    Console.WriteLine("Back to menu");
    
}

public void ShredToken()
{
    Console.WriteLine("Enter Token: ");
    string inputToken = Console.ReadLine();
    string sqlCommandString = $"SELECT token_id FROM auth_tokens WHERE token = '{inputToken}'";
    var command = new SQLiteCommand(sqlCommandString, _connection);
    string tokenId = command.ExecuteScalar().ToString();
    sqlCommandString = $"DELETE FROM user_tokens WHERE token_id = '{tokenId}';";
    command = new SQLiteCommand(sqlCommandString, _connection);
    command.ExecuteNonQuery();
    sqlCommandString = $"DELETE FROM auth_tokens WHERE token_id = '{tokenId}';";
    command = new SQLiteCommand(sqlCommandString, _connection);
    command.ExecuteNonQuery();
    Console.WriteLine("Token Shreded");
    Console.WriteLine("Back to menu");
}

public void ChangePassword()
{
    Console.WriteLine("Enter User id: ");
    string inputUserId = Console.ReadLine();
    string sqlCommandString = $"SELECT password FROM users WHERE user_id = '{inputUserId}'";
    var command = new SQLiteCommand(sqlCommandString, _connection);
    var reader = command.ExecuteReader();
    string oldPassword="";
    while (reader.Read())
    {
        oldPassword = reader["password"].ToString();
    }
    if (oldPassword == "")
    {
        Console.WriteLine("User not found");
        Console.WriteLine("Back to menu");
        return;
    }
    Console.WriteLine("Enter New Password: ");
    string newPassword = Console.ReadLine();
    string newSalt = RandomStringGenerator(10);
    string hashedPassword = Argon2HashGenerator(newPassword, newSalt);
    sqlCommandString = $"UPDATE  users  SET password = '{hashedPassword}', salt='{newSalt}' WHERE user_id = '{inputUserId}';";
    command = new SQLiteCommand(sqlCommandString, _connection);
    command.ExecuteNonQuery();
    Console.WriteLine("Password Changed");
    Console.WriteLine("Back to menu");
    
}

public void RemovePassword()
{
    Console.WriteLine("Enter User id: ");
    string inputUserId = Console.ReadLine();
    string sqlCommandString = $"SELECT password FROM users WHERE user_id = '{inputUserId}'";
    var command = new SQLiteCommand(sqlCommandString, _connection);
    var reader = command.ExecuteReader();
    string oldPassword="";
    while (reader.Read())
    {
        oldPassword = reader["password"].ToString();
    }
    if (oldPassword == "")
    {
        Console.WriteLine("User not found");
        Console.WriteLine("Back to menu");
        return;
    }
    string newPassword ="No Password";
    string newSalt = RandomStringGenerator(10);
    sqlCommandString = $"UPDATE  users  SET password = '{newPassword}', salt='{newSalt}' WHERE user_id = '{inputUserId}';";
    command = new SQLiteCommand(sqlCommandString, _connection);
    command.ExecuteNonQuery();
    Console.WriteLine("Password Changed");
         
    sqlCommandString = $"SELECT token_id FROM user_tokens WHERE user_id = '{inputUserId}';";
    using ( command = new SQLiteCommand(sqlCommandString, _connection))
    {
        var validateUserId = command.ExecuteScalar();
        if (validateUserId == null)
        {
            Console.WriteLine("User not found");
            Console.WriteLine("Back to menu");
            return;
        }
    }

    Queue<string> tokensToDelete = new Queue<string>();
    sqlCommandString = $"SELECT token_id FROM user_tokens WHERE user_id = '{inputUserId}';";
    using ( command = new SQLiteCommand(sqlCommandString, _connection))
    using ( reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            tokensToDelete.Enqueue(reader["token_id"].ToString());
        }
    }

    sqlCommandString = $"DELETE FROM user_tokens WHERE user_id = '{inputUserId}';";
    using (command = new SQLiteCommand(sqlCommandString, _connection))
    {
        command.ExecuteNonQuery();
    }
    Console.WriteLine("Deleted tokens from user_tokens table");

    while (tokensToDelete.Count > 0)
    {
        string currentTokenId = tokensToDelete.Dequeue();
        sqlCommandString = $"DELETE FROM auth_tokens WHERE token_id = '{currentTokenId}';";
        using (command = new SQLiteCommand(sqlCommandString, _connection))
        {
            command.ExecuteNonQuery();
        }
    }
    Console.WriteLine("Deleted tokens from auth_tokens table");

    for (int i = 0; i < 3; i++)
    {
        string newToken = RandomStringGenerator(10);

        sqlCommandString = $"INSERT INTO auth_tokens (token) VALUES ('{newToken}');";
        using (command = new SQLiteCommand(sqlCommandString, _connection))
        {
            command.ExecuteNonQuery();
        }

        sqlCommandString = $"SELECT token_id FROM auth_tokens WHERE token = '{newToken}';";
        string newTokenId;
        using (command = new SQLiteCommand(sqlCommandString, _connection))
        {
            newTokenId = command.ExecuteScalar().ToString();
        }

        sqlCommandString = $"INSERT INTO user_tokens (user_id, token_id) VALUES ('{inputUserId}', '{newTokenId}');";
        using (command = new SQLiteCommand(sqlCommandString, _connection))
        {
            command.ExecuteNonQuery();
        }
    }

    Console.WriteLine("Generated new tokens");
    Console.WriteLine("Back to menu");


    
    

}


    
    private static string RandomStringGenerator(int length)
    {
        const string pool = "abcdefghijklmnopqrstuvwxyz0123456789+-_{}[]()!@#;:<>?$%^&*";
        var builder = new StringBuilder();

        for (var i = 0; i < length; i++)
        {
            var c = pool[_random.Next(0, pool.Length)];
            builder.Append(c);
        }

        return builder.ToString();
    }

    private static string Argon2HashGenerator(string password, string salt)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
        string finalHash;
        using (var argon2 = new Argon2id(passwordBytes))
        {
            argon2.Salt = saltBytes;
            argon2.DegreeOfParallelism = 1; // Number of threads to use
            argon2.MemorySize = 65536;     // Memory size in KB
            argon2.Iterations = 4;         // Number of iterations

            // Generate the hash
            byte[] hashBytes = argon2.GetBytes(32); // Output size in bytes

            // Convert the hash to a base64 string for storage
            finalHash = Convert.ToBase64String(hashBytes);
        }

        return finalHash;

    }

    public void MainRuntime()
    {
        Console.WriteLine("Type options to list all possible functions");
        while (true)
        {

            string input = Console.ReadLine() ?? throw new InvalidOperationException();
            switch (input)
            {
                case "exit":
                    return;

                case "create room":
                    CreateRoom();
                    break;

                case "delete room":
                    DeleteRoom();
                    break;

                case "create user":
                    CreateUser();
                    break;

                case "delete user":
                    DeleteUser();
                    break;

                case "add user":
                    AddUser();
                    break;

                case "remove user":
                    RemoveUser();
                    break;

                case "list rooms":
                    ListRooms();
                    break;
                
                case "list users":
                    ListUsers();
                    break;
                case "regen tokens":
                    RegenTokens();
                    break;
                case "get tokens":
                    GetTokens();
                    break;
                case "shred token":
                    ShredToken();
                    break;
                case "change password":
                    ChangePassword();
                    break;
                case "remove password":
                    RemovePassword();
                    break;

                case "options":
                    Console.WriteLine("list rooms - List all rooms; each room followed by the names of the users who are allowed in it");
                    Console.WriteLine("list users - List all users; each username followed the names of the rooms they are allowed in");
                    Console.WriteLine("create room - Create a new room");
                    Console.WriteLine("delete room - Delete a room");
                    Console.WriteLine("create user - Create a new user");
                    Console.WriteLine("add user - Add an existing user to a room");
                    Console.WriteLine("remove user - Remove existing user from a room");
                    Console.WriteLine("regen tokens - Regenerate 3 new tokens for a given user and shred old tokens");
                    Console.WriteLine("get tokens - Get tokens of given user");
                    Console.WriteLine("shred token - Shred the token");
                    Console.WriteLine("change password - Change password of a user (doesn't reset tokens)");
                    Console.WriteLine("remove password - Remove user password setting it to NULL and resets tokens");
                    Console.WriteLine("exit - Shutdown entire server");
                    break;

                default:
                    Console.WriteLine("Invalid input");
                    break;
            }


        }
    }

    void SocketServerThread()
    {
        //Moving all rooms and users from DB to live storage
        DbToServer();

        //Checking existence of main chat room (websocket)
        CheckMain();

    }


    void ApiThread()
    {
        var builder = WebApplication.CreateBuilder();


        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost3000",
                policy => policy
                    .WithOrigins("http://localhost:3000") // Allow requests from your frontend URL
                    .AllowAnyHeader() // Allow any headers
                    .AllowAnyMethod()); // Allow any HTTP methods
        });


        var app = builder.Build();
        app.UseCors("AllowLocalhost3000");


        app.MapGet("/getAllRooms", async () =>
        {
            string sqlCommandString = "SELECT room_name FROM rooms;";
            using var command = new SQLiteCommand(sqlCommandString, _connection);
            var output = await command.ExecuteReaderAsync();
            List<string> roomNames = new List<string>();
            while (output.Read())
            {
                string roomName = output["room_name"].ToString();
                roomNames.Add(roomName);

            }

            return roomNames;

        });


        app.MapGet("/getAllUsers", async () =>
        {
            List<Object> users = new List<Object>();
            string sqlCommandString = "SELECT user_id,username FROM users;";
            var command = new SQLiteCommand(sqlCommandString, _connection);
            var output = await command.ExecuteReaderAsync();
            while (output.Read())
            {
                string userId = output["user_id"].ToString();
                string username = output["username"].ToString();
                users.Add(new { id = userId, username = username });

            }

            return users;

        });


        app.MapGet("/getUserRooms/{userId}", async (string userId) =>
        {
            string sqlCommandString =
                $"SELECT DISTINCT r.room_name FROM users u JOIN room_users ru ON u.user_id = ru.user_id JOIN rooms r ON ru.room_id = r.room_id WHERE u.user_id = '{userId}';";
            using var command = new SQLiteCommand(sqlCommandString, _connection);
            var output = await command.ExecuteReaderAsync();
            List<string> roomNames = new List<string>();
            while (await output.ReadAsync())
            {
                string roomName = output["room_name"].ToString();
                roomNames.Add(roomName);
            }

            return roomNames;
        });



        app.MapGet("/getRoomFiles/{roomname}", async (string roomname) =>
        {
            string sqlCommandString =
                $"SELECT message_data FROM messages m JOIN rooms r ON m.room_id = r.room_id WHERE r.room_name = @roomname;";
            using var command = new SQLiteCommand(sqlCommandString, _connection);
            command.Parameters.AddWithValue("@roomname", roomname);

            var output = await command.ExecuteReaderAsync();
            List<object> messages = new List<object>();
            while (await output.ReadAsync())
            {
                // Parse the JSON string into a dynamic object
                var rawMessage = output["message_data"].ToString();
                var deserializedMessage = JsonSerializer.Deserialize<object>(rawMessage);
                if (deserializedMessage is JsonElement element &&
                    element.TryGetProperty("isFile", out JsonElement isFileProperty) && isFileProperty.GetBoolean())
                {
                    messages.Add(deserializedMessage);

                }

            }

            return messages;
        });

        app.MapGet("/getAllFiles", async () =>
        {
            string sqlCommandString = $"SELECT message_data FROM messages m JOIN rooms r ON m.room_id = r.room_id;";
            using var command = new SQLiteCommand(sqlCommandString, _connection);

            var output = await command.ExecuteReaderAsync();
            List<object> messages = new List<object>();
            while (await output.ReadAsync())
            {
                // Parse the JSON string into a dynamic object
                var rawMessage = output["message_data"].ToString();
                var deserializedMessage = JsonSerializer.Deserialize<object>(rawMessage);
                if (deserializedMessage is JsonElement element &&
                    element.TryGetProperty("isFile", out JsonElement isFileProperty) && isFileProperty.GetBoolean())
                {
                    messages.Add(deserializedMessage);

                }

            }

            return messages;
        });

        app.MapGet("/getRoomMessages/{roomname}", async (string roomname) =>
        {
            string sqlCommandString =
                $"SELECT message_data FROM messages m JOIN rooms r ON m.room_id = r.room_id WHERE r.room_name = @roomname;";
            using var command = new SQLiteCommand(sqlCommandString, _connection);
            command.Parameters.AddWithValue("@roomname", roomname);

            var output = await command.ExecuteReaderAsync();
            List<object> messages = new List<object>();
            while (await output.ReadAsync())
            {
                // Parse the JSON string into a dynamic object
                var rawMessage = output["message_data"].ToString();
                var deserializedMessage = JsonSerializer.Deserialize<object>(rawMessage);
                messages.Add(deserializedMessage);
            }

            return messages;
        });

        app.MapGet("/login/{username}/{password}", async (string username, string password) =>
        {
            string sqlCommandString = "SELECT user_id, password, salt FROM users WHERE username = @username;";

            using var command = new SQLiteCommand(sqlCommandString, _connection);
            command.Parameters.AddWithValue("@username",
                username); // Use parameterized queries to prevent SQL injection

            var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                string dbPassword = reader["password"].ToString();
                string dbSalt = reader["salt"].ToString();
                if (dbPassword == Argon2HashGenerator(password, dbSalt))
                {
                    return reader["user_id"];
                }

            }

            return false;

        });

        app.MapGet("/userExists/{userId}", async (string userId) =>
        {
            string sqlCommandString = "SELECT user_id FROM users WHERE user_id = @userId;";
            using var command = new SQLiteCommand(sqlCommandString, _connection);
            command.Parameters.AddWithValue("@userId", userId);
            var output = await command.ExecuteScalarAsync();
            if (output == null)
            {
                return Results.NotFound(new
                    { error = "NotFound", message = $"The user with id '{userId}' does not exist." });
            }
            else
            {
                return Results.NoContent();
            }
        });


        app.MapGet("/getRoomUsers/{roomname}", async (string roomname) =>
        {
            string sqlCommandString = "SELECT u.username FROM users u " +
                                      "JOIN room_users ru ON u.user_id = ru.user_id " +
                                      "JOIN rooms r ON ru.room_id = r.room_id " +
                                      "WHERE r.room_name = @roomname;";

            using var command = new SQLiteCommand(sqlCommandString, _connection);
            command.Parameters.AddWithValue("@roomname", roomname);

            var output = await command.ExecuteReaderAsync();
            List<string> usernames = new List<string>();

            while (await output.ReadAsync())
            {
                string username = output["username"].ToString();
                usernames.Add(username);
            }

            return usernames;
        });

        app.MapPost("/createRoom/{inputRoomName}", async (string inputRoomName) =>
        {
            string sqlCommandString = $"SELECT room_name FROM rooms WHERE room_name='{inputRoomName}';";
            SQLiteCommand command = new SQLiteCommand(sqlCommandString, _connection);
            var commandOutputReader = command.ExecuteReader();
            bool roomExists = false;
            while (commandOutputReader.Read())
            {
                String roomName = commandOutputReader["room_name"].ToString();
                if (roomName == inputRoomName)
                {
                    roomExists = true;
                    break;
                }
            }

            if (roomExists)
            {
                return Results.Conflict(new
                    { error = "Conflict", message = $"The name '{inputRoomName}' already exists in the list." });
            }

            sqlCommandString = $"INSERT INTO rooms (room_name) VALUES('{inputRoomName}');";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();


            return Results.Created();

        });

        app.MapDelete("/deleteRoom/{roomName}/", async (string roomName) =>
        {
            if (roomName == "Main")
            {
                return Results.Json(new { error = "Forbidden", message = "Deleting main chat is forbidden" },
                    statusCode: 403);
            }

            string sqlCommandString = $"SELECT room_id FROM rooms WHERE room_name='{roomName}';";
            var command = new SQLiteCommand(sqlCommandString, _connection);
            var roomId = command.ExecuteScalar();

            if (roomId == null)
            {
                return Results.NotFound(new { error = "NotFound", message = $"The room '{roomName}' does not exist." });
            }

            sqlCommandString = $"DELETE FROM room_users WHERE room_id='{roomId}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            Console.WriteLine("Removed room from room_users table");

            sqlCommandString = $"DELETE FROM rooms WHERE room_name='{roomName}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();

            return Results.NoContent();
        });

        app.MapPost("/createUser/{username}", async (string username) =>
        {
            string inputUsername = username;
            string inputPassword = "No Password";
            string inputSalt = RandomStringGenerator(10);
            string finalHash = Argon2HashGenerator(inputPassword, inputSalt);
            string sqlCommandString =
                $"INSERT INTO users (username, password,salt) VALUES('{inputUsername}', '{inputPassword}','{inputSalt}');";
            var command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            Console.WriteLine("User Created");
            sqlCommandString = $"SELECT user_id FROM users WHERE username='{inputUsername}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            var userId = command.ExecuteScalar().ToString();
            string tokken;
            for (int i = 0; i < 3; i++)
            {

                tokken = RandomStringGenerator(10);
                sqlCommandString = $"INSERT INTO auth_tokens (token) VALUES ('{tokken}');";
                command = new SQLiteCommand(sqlCommandString, _connection);
                command.ExecuteNonQuery();
                sqlCommandString = $"select token_id from auth_tokens WHERE token='{tokken}';";
                command = new SQLiteCommand(sqlCommandString, _connection);
                var tokenId = command.ExecuteScalar().ToString();
                sqlCommandString = $"insert into user_tokens (user_id, token_id) VALUES ('{userId}', '{tokenId}');";
                command = new SQLiteCommand(sqlCommandString, _connection);
                command.ExecuteNonQuery();

            }

            return Results.Created();
        });

        app.MapDelete("/deleteUser/{inputUserId}/", async (string inputUserid) =>
        {
            string sqlCommandString = $"SELECT username FROM users WHERE user_id='{inputUserid}';";
            var command = new SQLiteCommand(sqlCommandString, _connection);
            var username = command.ExecuteScalar();
            if (username == null)
            {
                return Results.NotFound(new
                    { error = "NotFound", message = $"The user with id '{inputUserid}' does not exist." });
            }
            else
            {
                sqlCommandString = $"select token_id from user_tokens where user_id='{inputUserid}';";
                command = new SQLiteCommand(sqlCommandString, _connection);
                var output = command.ExecuteReader();
                while (output.Read())
                {

                    string token = output["token_id"].ToString();
                    sqlCommandString = $"DELETE FROM user_tokens WHERE token_id='{token}';";
                    command = new SQLiteCommand(sqlCommandString, _connection);
                    command.ExecuteNonQuery();
                    sqlCommandString = $"DELETE FROM auth_tokens WHERE token_id='{token}';";
                    command = new SQLiteCommand(sqlCommandString, _connection);
                    command.ExecuteNonQuery();


                }

                sqlCommandString = $"DELETE FROM room_users WHERE user_id='{inputUserid}';";
                command = new SQLiteCommand(sqlCommandString, _connection);
                var modifiedRows = command.ExecuteNonQuery();
                Console.WriteLine($"{modifiedRows} rows deleted from room_users table");
                sqlCommandString = $"DELETE FROM users WHERE user_id='{inputUserid}';";
                command = new SQLiteCommand(sqlCommandString, _connection);
                command.ExecuteNonQuery();
                return Results.NoContent();
            }
        });

        app.MapDelete("/deleteUserRoomMessages/{userId}/{roomName}", async (string userId, string roomName) =>
        {
            if (roomName == "Main")
            {
                return Results.Conflict(new { error = "Conflict", message = $"The user can't be removed from main." });
            }
            string sqlCommandString = $"SELECT room_id FROM rooms WHERE room_name='{roomName}';";
            var command = new SQLiteCommand(sqlCommandString, _connection);
            var roomId = command.ExecuteScalar().ToString();
            if (roomId == null)
            {
                return Results.NotFound(new { error = "NotFound", message = $"The room doesnt exist." });
            }

            using var sqlCommand = _connection.CreateCommand();
            command.CommandText = @"
            DELETE FROM messages 
            WHERE json_valid(message_data) = 1 
            AND json_extract(message_data, '$.senderId') = @senderId AND room_id = @roomId;";
            ;
            command.Parameters.AddWithValue("@senderId", userId);
            command.Parameters.AddWithValue("@roomId", roomId);


            int affectedRows = await command.ExecuteNonQueryAsync();
            if (affectedRows == 0)
            {
                return Results.NotFound(new
                {
                    message = $"No messages found for userId '{userId}'."
                });
            }

            return Results.Ok(new
            {
                message = $"Messages from userId '{userId}' deleted successfully.",
                count = affectedRows
            });



        });

    app.MapDelete("/deleteUserMessages/{userId}", async (string userId) =>
        {
            using var command = _connection.CreateCommand();

            // Prepare SQL DELETE query with JSON extraction for the userId
            command.CommandText = @"
            DELETE FROM messages 
            WHERE json_valid(message_data) = 1 
            AND json_extract(message_data, '$.senderId') = @senderId";
            command.Parameters.AddWithValue("@senderId", userId);

            // Execute the query and get the number of affected rows
            int affectedRows = await command.ExecuteNonQueryAsync();
            if (affectedRows == 0)
            {
                return Results.NotFound(new 
                { 
                    message = $"No messages found for userId '{userId}'."
                });
            }
            return Results.Ok(new 
            { 
                message = $"Messages from userId '{userId}' deleted successfully.", 
                count = affectedRows 
            });
        });
        app.MapPost("/addUser/{inputUserId}/{inputRoomName}", async (string inputUserId, string inputRoomName) =>
        {
            
            string sqlCommandString = $"SELECT user_id FROM users WHERE user_id='{inputUserId}';";
            var command = new SQLiteCommand(sqlCommandString, _connection);
            var userId = command.ExecuteScalar();
            if (userId == null)
            {
                return Results.NotFound(new { error = "NotFound", message = $"The user id does not exist." });

            } 
            sqlCommandString=$"SELECT room_id FROM rooms WHERE room_name='{inputRoomName}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            var roomId = command.ExecuteScalar();
            if (roomId == null)
            {
                return Results.NotFound(new { error = "NotFound", message = $"The room does not exist." });

            }
            sqlCommandString=$"SELECT user_id from room_users WHERE room_id='{roomId}' AND user_id='{inputUserId}';";
            command = new SQLiteCommand(sqlCommandString, _connection); 
            userId = command.ExecuteScalar();
            if (userId == null)
            {
                sqlCommandString=$"INSERT INTO room_users (room_id,user_id) VALUES('{roomId}','{inputUserId}');";
                command = new SQLiteCommand(sqlCommandString, _connection);
                command.ExecuteNonQuery();
                return Results.NoContent();
            }
            else
            {
                return Results.Conflict(new {error="Conflict", message = "The user id is already present in the room"});

            }
            
        });

        app.MapDelete("removeUser/{inputUserId}/{inputRoomName}", async (string inputUserId, string inputRoomName) =>
        {

            if (inputRoomName == "Main")
            {
                return Results.Conflict(new { error = "Conflict", message = $"The user can't be removed from main." });
            }
            string sqlCommandString = $"SELECT room_id FROM rooms WHERE room_name='{inputRoomName}';";
            var command = new SQLiteCommand(sqlCommandString, _connection);
            var roomId = command.ExecuteScalar().ToString();
            if (roomId == null)
            {
                return Results.NotFound(new { error = "NotFound", message = $"The room does not exist." });
            }
             sqlCommandString = $"SELECT room_id, user_id FROM room_users WHERE room_id='{roomId}' AND user_id='{inputUserId}';";
             command = new SQLiteCommand(sqlCommandString, _connection);
            var output = command.ExecuteScalar();
            if (output == null)
            {
                return Results.NotFound(new
                    { error = "NotFound", message = $"User not found in room." });
       
            }
            else
            {
                sqlCommandString = $"DELETE FROM room_users WHERE user_id='{inputUserId}' AND room_id='{roomId}';";
                command = new SQLiteCommand(sqlCommandString, _connection);
                command.ExecuteNonQuery();
                return Results.NoContent();

            }
        });
        
        
        ///this will be used by user when a password is forgotten
        app.MapPut("/changePassword/{userId}/{token}/{newPassword}", async (string userId, string token, string newPassword) =>
        {
            
            string sqlCommandString = $"SELECT token_id FROM auth_tokens WHERE token = '{token}'";
            var command = new SQLiteCommand(sqlCommandString, _connection);
            object tokenIdObj = command.ExecuteScalar();
            

            if (tokenIdObj == null)
            {
                return Results.NotFound(new { error = "NotFound", message = $"Token not found." });
            }
            string tokenId = tokenIdObj.ToString();
            sqlCommandString = $"SELECT password FROM users WHERE user_id = '{userId}'";
            command = new SQLiteCommand(sqlCommandString, _connection);
            var reader = command.ExecuteReader();
            string oldPassword="";
            while (reader.Read())
            {
                oldPassword = reader["password"].ToString();
            }
            if (oldPassword == "")
            {
                return Results.NotFound(new { error = "NotFound", message = $"User not found." });
            }
            
            sqlCommandString = $"DELETE FROM user_tokens WHERE token_id = '{tokenId}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            sqlCommandString = $"DELETE FROM auth_tokens WHERE token_id = '{tokenId}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            string newSalt = RandomStringGenerator(10);
            string hashedPassword = Argon2HashGenerator(newPassword, newSalt);
            sqlCommandString = $"UPDATE  users  SET password = '{hashedPassword}', salt='{newSalt}' WHERE user_id = '{userId}';";
            command = new SQLiteCommand(sqlCommandString, _connection);
            command.ExecuteNonQuery();
            return Results.NoContent();

            
        }); 
        app.MapPut("/regenTokens/{userId}", async (string userId) => 
        {
    // Validate if user exists
    string sqlCommandString = $"SELECT token_id FROM user_tokens WHERE user_id = '{userId}';";
    using (var command = new SQLiteCommand(sqlCommandString, _connection))
    {
        var validateUserId = command.ExecuteScalar();
        if (validateUserId == null)
        {
            return Results.NotFound(new { error = "NotFound", message = $"User not found." });
        }
    }

    // Gather tokens to delete
    Queue<string> tokensToDelete = new Queue<string>();
    sqlCommandString = $"SELECT token_id FROM user_tokens WHERE user_id = '{userId}';";
    using (var command = new SQLiteCommand(sqlCommandString, _connection))
    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            tokensToDelete.Enqueue(reader["token_id"].ToString());
        }
    }

    // Delete user tokens
    sqlCommandString = $"DELETE FROM user_tokens WHERE user_id = '{userId}';";
    using (var command = new SQLiteCommand(sqlCommandString, _connection))
    {
        command.ExecuteNonQuery();
    }

    // Delete tokens from auth_tokens
    while (tokensToDelete.Count > 0)
    {
        string currentTokenId = tokensToDelete.Dequeue();
        sqlCommandString = $"DELETE FROM auth_tokens WHERE token_id = '{currentTokenId}';";
        using (var command = new SQLiteCommand(sqlCommandString, _connection))
        {
            command.ExecuteNonQuery();
        }
    }

    // Generate and insert new tokens
    for (int i = 0; i < 3; i++)
    {
        string newToken = RandomStringGenerator(10);

        // Insert into auth_tokens
        sqlCommandString = $"INSERT INTO auth_tokens (token) VALUES ('{newToken}');";
        using (var command = new SQLiteCommand(sqlCommandString, _connection))
        {
            command.ExecuteNonQuery();
        }

        // Get new token_id
        sqlCommandString = $"SELECT token_id FROM auth_tokens WHERE token = '{newToken}';";
        string newTokenId;
        using (var command = new SQLiteCommand(sqlCommandString, _connection))
        {
            newTokenId = command.ExecuteScalar().ToString();
        }

        // Insert into user_tokens
        sqlCommandString = $"INSERT INTO user_tokens (user_id, token_id) VALUES ('{userId}', '{newTokenId}');";
        using (var command = new SQLiteCommand(sqlCommandString, _connection))
        {
            command.ExecuteNonQuery();
        }
    }

    return Results.NoContent();
});
        
        
        app.MapGet("/getTokens/{userId}", async (string userId) =>
        {
            string sqlCommandString = @"
        SELECT auth_tokens.token_id, auth_tokens.token 
        FROM user_tokens 
        JOIN auth_tokens ON user_tokens.token_id = auth_tokens.token_id 
        WHERE user_tokens.user_id = @userId;";

            var tokens = new List<Dictionary<string, string>>();

            using (var command = new SQLiteCommand(sqlCommandString, _connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tokens.Add(new Dictionary<string, string>
                        {
                            { "tokenId", reader["token_id"].ToString() },
                            { "token", reader["token"].ToString() }
                        });
                    }
                }
            }

            if (tokens.Count == 0)
            {
                return Results.NotFound(new { error = "NotFound", message = $"No tokens found for user '{userId}'." });
            }

            return Results.Json(new { userId, tokens });
        });



        app.MapDelete("/shredToken/{token}", async (string token) =>
        {
            string sqlCommandString = $"SELECT token_id FROM auth_tokens WHERE token = '{token}';";
            string tokenId;

            using (var command = new SQLiteCommand(sqlCommandString, _connection))
            {
                var result = command.ExecuteScalar();
                if (result == null)
                {
                    return Results.NotFound(new { error = "NotFound", message = $"The token '{token}' does not exist." });
                }
                tokenId = result.ToString();
            }

            sqlCommandString = $"DELETE FROM user_tokens WHERE token_id = '{tokenId}';";
            using (var command = new SQLiteCommand(sqlCommandString, _connection))
            {
                command.ExecuteNonQuery();
            }

            sqlCommandString = $"DELETE FROM auth_tokens WHERE token_id = '{tokenId}';";
            using (var command = new SQLiteCommand(sqlCommandString, _connection))
            {
                command.ExecuteNonQuery();
            }

            return Results.NoContent();
        });


        app.MapDelete("/removePassword/{userId}", async (string userId) =>
        {
    
        string sqlCommandString = $"SELECT password FROM users WHERE user_id = '{userId}'";
        var command = new SQLiteCommand(sqlCommandString, _connection);
        var reader = command.ExecuteReader();
        string oldPassword="";
        while (reader.Read())
        {
            oldPassword = reader["password"].ToString();
        }
        if (oldPassword == "")
        {
            return Results.NotFound(new { error = "NotFound", message = $"The user does not exist." });
        }
        string newPassword ="No Password";
        string newSalt = RandomStringGenerator(10);
        sqlCommandString = $"UPDATE  users  SET password = '{newPassword}', salt='{newSalt}' WHERE user_id = '{userId}';";
        command = new SQLiteCommand(sqlCommandString, _connection);
        command.ExecuteNonQuery();
             
        sqlCommandString = $"SELECT token_id FROM user_tokens WHERE user_id = '{userId}';";
        using ( command = new SQLiteCommand(sqlCommandString, _connection))
        {
            var validateUserId = command.ExecuteScalar();
            if (validateUserId == null)
            {
                return Results.NotFound(new { error = "NotFound", message = $"The user does not exist." });

            }
        }

        Queue<string> tokensToDelete = new Queue<string>();
        sqlCommandString = $"SELECT token_id FROM user_tokens WHERE user_id = '{userId}';";
        using ( command = new SQLiteCommand(sqlCommandString, _connection))
        using ( reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                tokensToDelete.Enqueue(reader["token_id"].ToString());
            }
        }

        sqlCommandString = $"DELETE FROM user_tokens WHERE user_id = '{userId}';";
        using (command = new SQLiteCommand(sqlCommandString, _connection))
        {
            command.ExecuteNonQuery();
        }

        while (tokensToDelete.Count > 0)
        {
            string currentTokenId = tokensToDelete.Dequeue();
            sqlCommandString = $"DELETE FROM auth_tokens WHERE token_id = '{currentTokenId}';";
            using (command = new SQLiteCommand(sqlCommandString, _connection))
            {
                command.ExecuteNonQuery();
            }
        }

        for (int i = 0; i < 3; i++)
        {
            string newToken = RandomStringGenerator(10);

            sqlCommandString = $"INSERT INTO auth_tokens (token) VALUES ('{newToken}');";
            using (command = new SQLiteCommand(sqlCommandString, _connection))
            {
                command.ExecuteNonQuery();
            }

            sqlCommandString = $"SELECT token_id FROM auth_tokens WHERE token = '{newToken}';";
            string newTokenId;
            using (command = new SQLiteCommand(sqlCommandString, _connection))
            {
                newTokenId = command.ExecuteScalar().ToString();
            }

            sqlCommandString = $"INSERT INTO user_tokens (user_id, token_id) VALUES ('{userId}', '{newTokenId}');";
            using (command = new SQLiteCommand(sqlCommandString, _connection))
            {
                command.ExecuteNonQuery();
            }
        }

    
        return Results.NoContent();

        });

        app.MapGet("/isAdmin/{UserId}", (string UserId) =>
        {
            string sqlCommandString = $"SELECT is_admin FROM users WHERE user_id = '{UserId}'";
            var command = new SQLiteCommand(sqlCommandString, _connection);
            var output = command.ExecuteScalar().ToString();
            if (output == "1")
            {
                return true;
            }
            return false;



        });

        app.MapGet("/isNewUser/{Username}", (string Username) =>
        {

            string sqlCommandString = $"SELECT password FROM users WHERE username = '{Username}'";
            var command = new SQLiteCommand(sqlCommandString, _connection);
            
            var output = command.ExecuteScalar();
            if (output == null)
            {
                return Results.NotFound($"Username '{Username}' not found.");            }
            
            if (output.ToString() == "No Password")
            {
                return Results.Ok(true);
            }
            return Results.Ok(false);            
           

        });

        app.MapPost("/changePasswordNewUser/{Username}/{NewPassword}", (string Username, string NewPassword) =>
        {
            string sqlCommandString = $"SELECT password FROM users WHERE username = '{Username}'";
            var command = new SQLiteCommand(sqlCommandString, _connection);

            var output = command.ExecuteScalar();
            if (output == null)
            {
                return Results.NotFound($"Username '{Username}' not found.");
            }

            if (output.ToString() == "No Password")
            {
                
                string newSalt = RandomStringGenerator(10);
                string hashedPassword = Argon2HashGenerator(NewPassword, newSalt);
                sqlCommandString = $"UPDATE  users  SET password = '{hashedPassword}', salt='{newSalt}' WHERE username = '{Username}';";
                command = new SQLiteCommand(sqlCommandString, _connection);
                command.ExecuteNonQuery();
                return Results.NoContent();
            }

            return Results.Conflict($"Username '{Username}' does not belong to a new user.");


        });

        app.MapPost("/forgotPassword/{Username}/{Password}/{Tokken}",
            (string Username, string Password, string Tokken) =>
            {
                string sqlCommandString = $"SELECT token_id FROM auth_tokens WHERE token='{Tokken}';";
                var command = new SQLiteCommand(sqlCommandString, _connection);
                var output = command.ExecuteScalar();
                if (output == null)
                {
                    return Results.NotFound($"Token not found.");
                }
                string tokenId = output.ToString();
                sqlCommandString = $"SELECT user_id from user_tokens WHERE token_id = '{tokenId}';";
                 command = new SQLiteCommand(sqlCommandString, _connection);
                 output = command.ExecuteScalar();
                 if (output == null)
                 {
                     return Results.NotFound($"Token belongs to no one.");
                 }
                 string userId = output.ToString();
                 sqlCommandString = $"SELECT user_id from users WHERE username = '{Username}';";
                 command = new SQLiteCommand(sqlCommandString, _connection);
                 output = command.ExecuteScalar();
                 if (output == null || output.ToString() !=userId)
                 {
                     return Results.NotFound($"User id not found.");
                 }
                 
                 
                 sqlCommandString = $"DELETE from user_tokens WHERE token_id = '{tokenId}';";
                 command = new SQLiteCommand(sqlCommandString, _connection);
                 command.ExecuteNonQuery();
                 sqlCommandString =  $"DELETE from auth_tokens WHERE token_id = '{tokenId}';";
                 command = new SQLiteCommand(sqlCommandString, _connection);
                 command.ExecuteNonQuery();
                 
                 string newSalt = RandomStringGenerator(10);    
                 string hashedPassword = Argon2HashGenerator(Password, newSalt);
                 sqlCommandString = $"UPDATE  users  SET password = '{hashedPassword}', salt='{newSalt}' WHERE user_id = '{userId}';";
                 command = new SQLiteCommand(sqlCommandString, _connection);
                 command.ExecuteNonQuery();

                 
                 
                 
                 
                
                 
                 
               return Results.Ok("Password changed succesfully");

            });
        
        app.Run();

    }
    
    

    public Server(string ipAddress, int port, string connectionString)
    {
        _connection = new SQLiteConnection(connectionString);
        
        
        //Starting server (websocket and DB connection)
        _connection.Open();
        string uri = $"ws://{ipAddress}:{port}";
        _server = new WebSocketServer(uri);
        Console.WriteLine(uri);
        _server.Start();     
        Console.WriteLine("Server started");
        
        Thread thread1 = new Thread(new ThreadStart(ApiThread));
        Thread thread2 = new Thread(new ThreadStart(SocketServerThread));
        thread1.Start();
        thread2.Start();
    }
    


}
