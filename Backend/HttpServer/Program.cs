namespace HttpServer;

class Program
{
    static void Main()
    {
      
        string dbPath = "C:\\Users\\sergi\\RiderProjects\\HttpServer\\HttpServer\\chat.db";
        string connectionString= $"Data Source={dbPath};version=3;";
        Server myserver = new Server("127.0.0.1", 8080,connectionString);
        myserver.MainRuntime();
       

    }
}