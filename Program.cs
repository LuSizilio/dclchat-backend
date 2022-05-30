using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

String ip = "192.168.0.10";
Int32 port = 13000;
TcpClient client;

Dictionary<string,User> users = new Dictionary<string,User>();

Message? messageReadJson(string jsonString){
  return JsonSerializer.Deserialize<Message>(jsonString);
}

void userConnected(string socketId, string name){
  users.Add(socketId, new User(name));
  string userJoinJson = JsonSerializer.Serialize(new Dictionary<string,Message>(){{"broadcast", new Message{
    eventName = "userJoin",
    socketId = socketId,
    name = name
  }}});
  emit(userJoinJson);
  Dictionary<string,string> usersList = new Dictionary<string,string>();
  foreach(KeyValuePair<string, User> userData in users){
    usersList.Add(userData.Key, userData.Value.name);
  }
  string usersUpdateJson = JsonSerializer.Serialize(new Message{
    eventName = "usersUpdate",
    socketId = socketId,
    users = usersList
  });
  emit(usersUpdateJson);
}

void userDisconnected(string socketId){
  string userDisconnectedJson = JsonSerializer.Serialize(new Dictionary<string,Message>(){{"broadcast", new Message{
    eventName = "userDisconnectedJson",
    socketId = socketId,
    name = users[socketId].name
  }}});
  emit(userDisconnectedJson);
  users.Remove(socketId);
  Dictionary<string,string> usersList = new Dictionary<string,string>();
  foreach(KeyValuePair<string, User> userData in users){
    usersList.Add(userData.Key, userData.Value.name);
  }
  string usersUpdateJson = JsonSerializer.Serialize(new Message{
    eventName = "usersUpdate",
    socketId = socketId,
    users = usersList
  });
  emit(usersUpdateJson);
}

void userSendMessage(string socketId, string text){
  string userSendMessageJson = JsonSerializer.Serialize(new Dictionary<string,Message>(){{"broadcast", new Message{
    eventName = "userMessage",
    socketId = socketId,
    name = users[socketId].name,
    text = text
  }}});
  emit(userSendMessageJson);
  string userStopTypingJson = JsonSerializer.Serialize(new Dictionary<string,Message>(){{"broadcast", new Message{
    eventName = "userStopTyping",
    socketId = socketId,
  }}});
  emit(userStopTypingJson);
}

void userTyping(string socketId){
  string userTypingJson = JsonSerializer.Serialize(new Dictionary<string,Message>(){{"broadcast", new Message{
    eventName = "userTyping",
    socketId = socketId,
    name = users[socketId].name
  }}});
  emit(userTypingJson);
}

void stopTyping(string socketId){
  string stopTypingJson = JsonSerializer.Serialize(new Dictionary<string,Message>(){{"broadcast", new Message{
    eventName = "stopTyping",
    socketId = socketId
  }}});
  emit(stopTypingJson);
}

void emit(string message){
  NetworkStream stream = client.GetStream();
  byte[] msg = System.Text.Encoding.ASCII.GetBytes(message + "endMessageDCL");
  stream.Write(msg, 0, msg.Length);
  Console.WriteLine("Sent emit: {0}", message);
}

IPAddress address = IPAddress.Parse(ip);
TcpListener listener = new TcpListener(address, port);;
try {
  listener.Start();
  Console.WriteLine($"Server started. Listening to TCP clients at {ip}:{port}");
  // Buffer para ler dados
  Byte[] bytes = new Byte[256];
  String data;

  // Entrar no loop de listening
  while (true) {
    Console.Write("Aguardando conexão... ");

    client = listener.AcceptTcpClient();
    Console.WriteLine("Conectado!");

    // Recebe objeto stream para leitura e escrita
    NetworkStream stream = client.GetStream();

    int i;

    // Loop para receber todos os dados enviados pelo client
    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0) {
      // Traduzir bytes para texto ASCII
      data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
      Console.WriteLine("Received: {0}", data);

      try{
        Message? messageData = messageReadJson(data);

        if(messageData != null){
          Console.WriteLine($"Event: {messageData.eventName}");
          switch(messageData!.eventName){
            case "connect":
              userConnected(messageData.socketId!, messageData.name!);
              break;
            case "disconnect":
              userDisconnected(messageData.socketId!);
              break;
            case "msg":
              userSendMessage(messageData.socketId!, messageData.text!);
              break;
            case "typing":
              userTyping(messageData.socketId!);
              break;
            case "stopTyping":
              stopTyping(messageData.socketId!);
              break;
            default:
              Console.WriteLine($"Evento não reconhecido: {messageData.eventName}");
              break;
          }
        }
      }
      catch(Exception e){

      }
    }

    // Desligar e finalizar conexão
    client.Close();
  }
} catch (SocketException e) {
  Console.WriteLine("SocketException: {0}", e);
} finally {
  // Parar servidor para todos clientes
  listener.Stop();
}