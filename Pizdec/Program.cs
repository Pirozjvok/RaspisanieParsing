using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

string url = "https://asu.bspu.ru/Rasp/Rasp.aspx?group=20128&sem=2";

SocketsHttpHandler handler = new SocketsHttpHandler();

handler.UseCookies = false;

HttpClient client = new HttpClient(handler);

HttpResponseMessage first_resp_msg = await client.GetAsync(url);

string first_resp_str = await first_resp_msg.Content.ReadAsStringAsync();

string anti_xsrf_token = first_resp_msg.Headers.GetValues("Set-Cookie").First(x => x.StartsWith("__AntiXsrfToken")).Split(';')[0];

string view_state = Regex.Match(first_resp_str, @"id=""__VIEWSTATE"" value=""(.*?)"" />").Groups[1].Value;

Dictionary<string, string> form_data = new Dictionary<string, string>()
{
    {"__EVENTTARGET", "ctl00$MainContent$ASPxGridView1"},
    {"__EVENTARGUMENT", @"6|EXPORT3|Xls"},
    {"__VIEWSTATE", view_state }
};

FormUrlEncodedContent content = new FormUrlEncodedContent(form_data);

HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://asu.bspu.ru/Rasp/Rasp.aspx?group=20128&sem=2");

content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

request.Content = content;

request.Headers.Add("Cookie", $"Path=/; {anti_xsrf_token}");

HttpResponseMessage response = await client.SendAsync(request);

byte[] data = await response.Content.ReadAsByteArrayAsync();

File.WriteAllBytes("test.xls", data);

Console.ReadLine();
