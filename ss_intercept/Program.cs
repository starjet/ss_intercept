using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;

namespace ss_intercept
{
    class Program
    {
        static string url = "";

        static void DoForward(HttpListenerContext context)
        {
            HttpListenerResponse response;
            response = context.Response;
            Console.WriteLine("Accessing " + url + context.Request.RawUrl);
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url + context.Request.RawUrl);
            hwr.Method = "GET";
            HttpWebResponse hwresp = (HttpWebResponse)hwr.GetResponse();
            Stream s1 = hwresp.GetResponseStream();
            Task t = s1.CopyToAsync(response.OutputStream);
            t.Wait();
            response.OutputStream.Close();
            response.OutputStream.Dispose();
            s1.Close();
            s1.Dispose();
        }

        static void DoSend(HttpListenerContext context, string path)
        {
            HttpListenerResponse response;
            byte[] buffer;
            Stream output;
            response = context.Response;
            Console.WriteLine("Sending " + path);
            FileStream fs = null;
            try
            {
                fs = File.OpenRead(path);
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            output.Dispose();
        }

        static void Main(string[] args)
        {
            url = "http://storage.game.starlight-stage.jp";
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://*:80/");
            listener.Start();
            HttpListenerContext context;
            Console.WriteLine("Listening...");
            while (true)
            {
                context = listener.GetContext();
                string filename = context.Request.RawUrl.Split('/').Last();
                if (File.Exists(filename))
                {
                    DoSend(context, filename);
                }
                else
                {
                    DoForward(context);
                }
            }
        }
    }
}
