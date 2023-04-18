using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        // Создаем объект TcpListener для прослушивания входящих подключений
        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
        server.Start();

        Console.WriteLine("Server has started on 127.0.0.1:8080.");

        while (true)
        {
            // Принимаем входящее подключение
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Client connected.");

            // Получаем поток для чтения данных от клиента
            NetworkStream stream = client.GetStream();

            // Создаем буфер для чтения данных
            byte[] buffer = new byte[1024];

            // Читаем данные из потока
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received: {0}", data);

            // Обрабатываем запрос
            string response = HandleRequest(data);

            // Отправляем ответ клиенту
            byte[] responseData = Encoding.ASCII.GetBytes(response);
            stream.Write(responseData, 0, responseData.Length);
            Console.WriteLine("Sent: {0}", response);

            // Закрываем подключение
            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }
    static string HandleRequest(string request)
    {
        // Разбиваем строку запроса на параметры
        string[] parameters = request.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Получаем запрос из первой части строки запроса
        string query = parameters[1];

        // Разбиваем запрос на параметры
        string[] queryParams = query.Split(new[] { '/', '?', '&' }, StringSplitOptions.RemoveEmptyEntries);

        int num1 = 0, num2 = 0, num3 = 0, num4 = 0;

        List<string> invalidParams = new List<string>();

        // Проверяем, что все параметры присутствуют и имеют правильный формат
        if (queryParams.Length < 3 ||
            !int.TryParse(queryParams[0].Substring(queryParams[0].IndexOf('=') + 1), out num1))
        {
            invalidParams.Add("num1");
        }
        if (!int.TryParse(queryParams[1].Substring(queryParams[1].IndexOf('=') + 1), out num2))
        {
            invalidParams.Add("num2");
        }
        if (!int.TryParse(queryParams[2].Substring(queryParams[2].IndexOf('=') + 1), out num3))
        {
            invalidParams.Add("num3");
        }

        // Проверяем значения параметров
        if (num1 < 0 || num1 > 100)
        {
            invalidParams.Add("num1");
        }
        if (num2 < 0 || num2 > 100)
        {
            invalidParams.Add("num2");
        }
        if (num3 < 0 || num3 > 100)
        {
            invalidParams.Add("num3");
        }

        if (invalidParams.Count > 0)
        {
            string errorMsg = string.Format("{{\"error\":\"Invalid parameters: {0}\"}}", string.Join(",", invalidParams));
            return CreateHTTPResponse(400, "Bad Request", errorMsg);
        }

        // Считаем сумму параметров и возвращаем результат
        int sum = num1 + num2 + num3;
        string response = string.Format("{{\"result\":{0}}}", sum);

        return CreateHTTPResponse(200, "OK", response);
    }
   
    static string CreateHTTPResponse(int statusCode, string statusMessage, string data)
    {
        // Формируем HTTP-ответ
        string httpResponse = string.Format("HTTP/1.1 {0} {1}\r\n", statusCode, statusMessage);
        httpResponse += "Content-Type: application/json\r\n";
        httpResponse += "Access-Control-Allow-Origin: *\r\n";
        httpResponse += string.Format("Content-Length: {0}\r\n", Encoding.ASCII.GetByteCount(data));
        httpResponse += "\r\n";
        httpResponse += data;

        return httpResponse;
    }
}