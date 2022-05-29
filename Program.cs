using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

string createJsonResponse(string eventName, string socketId, string? name, long? timestamp, string? text){
  var messageResponse = new Message{
          eventName = eventName,
          socketId = socketId,
          name = name,
          timestamp = timestamp,
          text = text
      };

  return JsonSerializer.Serialize(messageResponse);
}

Message? readJson(string jsonString){
  return JsonSerializer.Deserialize<Message>(jsonString);
}

TcpListener listener = null;
try {
  String ip = "IP_ADDRESS";
  Int32 port = 13000;
  IPAddress address = IPAddress.Parse(ip);
  listener = new TcpListener(address, port);

  listener.Start();
  Console.WriteLine($"Server started. Listening to TCP clients at {ip}:{port}");
  // Buffer para ler dados
  Byte[] bytes = new Byte[256];
  String data = null;

  // Entrar no loop de listening
  while (true) {
    Console.Write("Aguardando conexão... ");

    TcpClient client = listener.AcceptTcpClient();
    Console.WriteLine("Conectado!");

    // Recebe objeto stream para leitura e escrita
    NetworkStream stream = client.GetStream();

    int i;

    // Loop para receber todos os dados enviados pelo client
    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0) {
      // Traduzir bytes para texto ASCII
      data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
      Console.WriteLine("Received: {0}", data);

      Message? messageData = readJson(data);

      Console.WriteLine($"Event: {messageData?.eventName}");

      if(messageData != null){
        string jsonString = createJsonResponse(messageData.eventName, messageData.socketId, messageData.name, messageData.timestamp, messageData.text);
        byte[] msg = System.Text.Encoding.ASCII.GetBytes(jsonString);

        // Enviar resposta
        stream.Write(msg, 0, msg.Length);
        Console.WriteLine("Sent: {0}", data);
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