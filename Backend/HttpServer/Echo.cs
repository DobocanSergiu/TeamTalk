using System.Data.SQLite;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Web;

using System.Text.Json;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;


namespace HttpServer;

public class Echo : WebSocketBehavior
{
    /// List of active sessions (live connected users)
    private static Dictionary<string, (string name,string room)> _idNameRoomList =new Dictionary<string, (string name,string room)>();
    /// List of allowed usernames
    private List<string> _allowedUsers=new List<string>();
    private string currentRoom;
    


    public Echo(List<string> allowedUsers)
    {
        foreach (var user in allowedUsers)
        {
            this._allowedUsers.Add(user);
        }
        
    }
    
    protected override void OnOpen()
    {
        var uri = Context.RequestUri;
        var queryString = uri.Query;
        var queryParams = HttpUtility.ParseQueryString(queryString);                    
        // Retrieve the 'name' parameter from the query string
        string name = queryParams["name"];
        string path = uri.AbsolutePath;

        // Split the path to get the room name
        string[] segments = path.Split('/');

        // The room name is the last segment
        string roomName = segments[^1];
        currentRoom = roomName;




        if (name == null)
        {
            Console.WriteLine("Invalid Parameters: No 'name' provided.");
            Context.WebSocket.Close(); // Close the WebSocket connection if no 'name' is provided
        }    
        else if (_allowedUsers.Contains(name) == false)
        {
            Console.WriteLine("User name not allowed.");
            Context.WebSocket.Close();//Close the WebsSocket connection if the username is not on whitelist.
        }
        else
        {
            // Successfully received the 'name' parameter, register the session
            Console.WriteLine($"Session {ID} connected with name: {name}");
            
            // Add the session ID and name to the dictionary
            _idNameRoomList.Add(ID, (name, roomName));

            ////idNameList[ID] = name; 
        }
       

    }
    

    // Called when a message is received
    protected override void OnMessage(MessageEventArgs e)
    {
        Console.WriteLine("Message received: " + e.Data);
        try
        {
            using (var connection = new SQLiteConnection("Data Source=C:\\Users\\sergi\\RiderProjects\\HttpServer\\HttpServer\\chat.db;Version=3;"))
            {
                connection.Open();

                // Get room_id using a parameterized query
                string sqlCommandString = "SELECT room_id FROM rooms WHERE room_name = @RoomName";
                using (var sqlCommand = new SQLiteCommand(sqlCommandString, connection))
                {
                    sqlCommand.Parameters.AddWithValue("@RoomName", currentRoom);
                    var roomId = sqlCommand.ExecuteScalar()?.ToString();

                    if (roomId != null)
                    {
                        // Insert message using a parameterized query
                        sqlCommandString = "INSERT INTO messages (room_id, message_data) VALUES (@RoomId, @MessageData)";
                        using (var insertCommand = new SQLiteCommand(sqlCommandString, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@RoomId", roomId);
                            insertCommand.Parameters.AddWithValue("@MessageData", e.Data);
                            insertCommand.ExecuteNonQuery();
                        }

                        // Broadcast message to all sessions
                        foreach (var session in Sessions.Sessions)
                        {
                            Sessions.SendTo(e.Data, session.ID);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Room ID not found for the current room.");
                    }
                }
            }
        }
        catch (SQLiteException ex)
        {
            Console.WriteLine($"Database error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }


    // Called when the client disconnects
    protected override void OnClose(CloseEventArgs e)
    {
        Console.WriteLine($"Client {ID} disconnected.");
        
        // Use lock to ensure thread-safe removal
        lock(_idNameRoomList)
        {
            if (_idNameRoomList.ContainsKey(ID))
            {
                _idNameRoomList.Remove(ID);
            }
        }
         

        //Sessions.CloseSession(ID);        
    }

    protected override void OnError(ErrorEventArgs e)
    {
        Console.WriteLine($"Client {ID} disconnected with error.");
        _idNameRoomList.Remove(ID);
    }
    
    public static bool IsValidJson(string jsonString)
    {
        try
        {
            using (JsonDocument document = JsonDocument.Parse(jsonString))
            {
                return true;
            }
        }
        catch (JsonException)
        {
            // JSON is invalid
            return false;
        }
    }

    
}