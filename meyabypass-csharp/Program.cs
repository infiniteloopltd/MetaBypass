// Decompiled with JetBrains decompiler
// Type: metabypass_csharp.Program
// Assembly: meyabypass-csharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7317E7B4-D266-48E4-9DBF-2CA182E8FC49
// Assembly location: C:\Projects\captcha-solver-csharp\meyabypass-csharp.dll

using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;

namespace metabypass_csharp
{
  public class Program
  {
    public static string ClientId { get; set; }

    public static string ClientSecret { get; set; }

    public static string Username { get; set; }

    public static string Password { get; set; }

    private static void GetAccessToken()
    {
      RestClient restClient = new RestClient("https://app.metabypass.tech/CaptchaSolver/oauth/token");
      restClient.Timeout = 30000;
      RestRequest restRequest = new RestRequest((Method) 1);
      restRequest.AddHeader("Content-Type", "application/json");
      restRequest.AddHeader("Accept", "application/json");
      string str = "{\n    \"grant_type\":\"password\",\n    \"client_id\":" + Program.ClientId + ",\n    \"client_secret\":\"" + Program.ClientSecret + "\",\n    \"username\":\"" + Program.Username + "\",\n    \"password\":\"" + Program.Password + "\"\n}";
      restRequest.AddParameter("application/json", (object) str, (ParameterType) 4);
      JObject jobject = JObject.Parse(restClient.Execute((IRestRequest) restRequest).Content);
      string path = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "token.txt");
      System.IO.File.WriteAllText(path, "Bearer " + (string) jobject["access_token"]);
      Console.WriteLine("Data written to the file: " + path);
    }

    private static string SelectAccessToken()
    {
      string path = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "token.txt");
      System.IO.File.ReadAllText(path);
      string[] strArray = System.IO.File.ReadAllLines(path);
      string str1 = "";
      foreach (string str2 in strArray)
        str1 += str2;
      return str1;
    }

    public static string ImageToBase64(string image_path) => Convert.ToBase64String(System.IO.File.ReadAllBytes(image_path));

    public static string TextCaptcha(string base64_image)
    {
      if (base64_image.Contains(";base64,"))
        base64_image = base64_image.Split(',')[1];
      JObject jobject = JObject.Parse(Program.send_request("https://app.metabypass.tech/CaptchaSolver/api/v1/services/captchaSolver", "{\n    \"image\":\"" + base64_image + "\"\n}").Content);
      string str = "";
      if ((int) jobject["status_code"] == 200)
        str = (string) jobject["data"][(object) "result"];
      return str;
    }

    public static string RecaptchaV3(string site_key, string domain)
    {
      JObject jobject = JObject.Parse(Program.send_request("https://app.metabypass.tech/CaptchaSolver/api/v1/services/bypassReCaptcha", "{\n    \"version\":\"3\",\n    \"sitekey\":\"" + site_key + "\",\n    \"url\":\"" + domain + "\"\n}").Content);
      string str = "";
      if ((int) jobject["status_code"] == 200)
        str = (string) jobject["data"][(object) "RecaptchaResponse"];
      return str;
    }

    private static string RecaptchaInvisible(string site_key, string domain)
    {
      JObject jobject = JObject.Parse(Program.send_request("https://app.metabypass.tech/CaptchaSolver/api/v1/services/bypassReCaptcha", "{\n    \"version\":\"invisible\",\n    \"sitekey\":\"" + site_key + "\",\n    \"url\":\"" + domain + "\"\n}").Content);
      string str = "";
      if ((int) jobject["status_code"] == 200)
        str = (string) jobject["data"][(object) "RecaptchaResponse"];
      return str;
    }

    public static string RecaptchaV2(string site_key, string domain)
    {
      JObject jobject1 = JObject.Parse(Program.send_request("https://app.metabypass.tech/CaptchaSolver/api/v1/services/bypassReCaptcha", "{\n    \"version\":\"2\",\n    \"sitekey\":\"" + site_key + "\",\n    \"url\":\"" + domain + "\"\n}").Content);
      string str1 = "";
      if ((int) jobject1["status_code"] == 200)
        str1 = (string) jobject1["data"][(object) "RecaptchaId"];
      Thread.Sleep(20000);
      string str2 = "";
      for (int index = 0; index < 15; ++index)
      {
        JObject jobject2 = JObject.Parse(Program.send_request("https://app.metabypass.tech/CaptchaSolver/api/v1/services/getCaptchaResult?recaptcha_id=" + str1, "", (Method) 0).Content);
        if ((int) jobject2["status_code"] == 200)
        {
          str2 = (string) jobject2["data"][(object) "RecaptchaResponse"];
          break;
        }
        if ((int) jobject2["status_code"] == 201)
          Thread.Sleep(5000);
        else
          break;
      }
      return str2;
    }

    private static IRestResponse send_request(string url, string body, Method method = 1)
    {
      Console.WriteLine(url);
      RestClient restClient = new RestClient(url);
      restClient.Timeout = 30000;
      RestRequest restRequest = new RestRequest(method);
      restRequest.AddHeader("Content-Type", "application/json");
      restRequest.AddHeader("Accept", "application/json");
      restRequest.AddHeader("Authorization", Program.SelectAccessToken());
      restRequest.AddParameter("application/json", (object) body, (ParameterType) 4);
      IRestResponse irestResponse = restClient.Execute((IRestRequest) restRequest);
      Console.WriteLine(irestResponse.Content);
      if (irestResponse.StatusCode != HttpStatusCode.Unauthorized)
        return irestResponse;
      Console.WriteLine("unauthorized");
      Program.GetAccessToken();
      return Program.send_request(url, body);
    }
  }
}
